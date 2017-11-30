using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Components;
using System;

namespace SocialPoint.Components
{
    public class BattleControllerBase : IDeltaUpdateable, IBattleStopListener, IBattleErrorDispatcher, IBattleErrorHandler, IDisposable
    {
        IBattleSetup _currentSetupComponent;
        Queue<IBattleSetup> _setupComponents;
        List<IBattleUpdate> _updateComponents;
        List<IBattleCleanup> _cleanupComponents;
        List<IBattleStart> _startComponents;
        List<IBattleStop> _stopComponents;
        List<IBattleStopListener> _stopListeners;
        ActionProcessor _actions;
        BattleErrorDispatcherBase _errorDispatcher;
        BattleStep _battleStep;
        protected IUpdateScheduler _scheduler;
        int _successfulStopEventsCount;
        int _totalStopEventsCount;
        bool _componentsRegistered;

        public BattleStep Step
        {
            get
            {
                return _battleStep;
            }
        }

        public BattleControllerBase(IUpdateScheduler scheduler)
        {
            _setupComponents = new Queue<IBattleSetup>();
            _startComponents = new List<IBattleStart>();
            _updateComponents = new List<IBattleUpdate>();
            _cleanupComponents = new List<IBattleCleanup>();
            _stopComponents = new List<IBattleStop>();
            _stopListeners = new List<IBattleStopListener>();
            _actions = new ActionProcessor();
            _errorDispatcher = new BattleErrorDispatcherBase();
            _battleStep = BattleStep.None;
            _scheduler = scheduler;
            _successfulStopEventsCount = 0;
            _totalStopEventsCount = 0;
        }

        // This method was added to ensure that certain components are registered before anything else. 
        // Atm the overrides for RegisterComponents() on the subclasses execute their own logic 
        // before deferring to base.RegisterComponents(), so I'm assuming that it's because of some order dependencies
        //
        // Alternatively, since the only component that requires this right now is the BattleControllerErrorHandler 
        // we might even add it directly to BattleControllerBase.Start()
        protected virtual void RegisterBootstrapComponents()
        {
        }

        protected virtual void RegisterComponents()
        {
        }

        public void Start()
        {
            if(!_componentsRegistered)
            {
                
                _componentsRegistered = true;
                RegisterBootstrapComponents();
                RegisterComponents();
            }
            _battleStep = BattleStep.Setup;
            _scheduler.Add(this, UpdateableTimeMode.GameTimeScaled, -1.0f);
        }

        public void Stop()
        {
            StopComponents();
        }

        public void Dispose()
        {
            _battleStep = BattleStep.Cleanup;
            _scheduler.Remove(this);
            _errorDispatcher.Dispose();
            RunCleanupComponents();
        }

        //IUpdateable
        public void Update(float dt)
        {
            switch(_battleStep)
            {
            case BattleStep.Setup:
                UpdateSetupStep(dt);
                break;
            case BattleStep.Start:
                RunStartStep();
                break;
            case BattleStep.Update:
                RunUpdateComponents(dt);
                break;
            default:
                break;
            }
        }

        void UpdateSetupStep(float dt)
        {
            var setupState = RunSetupComponents(dt);
            switch(setupState)
            {
            case BattleSetupState.Success:
                _battleStep = BattleStep.Start;
                break;
            default:
                break;
            }
        }

        void RunStartStep()
        {
            for(int i = 0; i < _startComponents.Count && _battleStep != BattleStep.Cleanup; i++)
            {
                _startComponents[i].Start();
            }
            _battleStep = BattleStep.Update;
        }

        BattleSetupState RunSetupComponents(float dt)
        {
            if(_currentSetupComponent == null && _setupComponents.Count > 0)
            {
                _currentSetupComponent = _setupComponents.Dequeue();
                _currentSetupComponent.Start();
            }
            if(_currentSetupComponent != null)
            {
                var setupState = _currentSetupComponent.Update(dt);
                if(setupState == BattleSetupState.Success)
                {
                    _currentSetupComponent = null;
                    if(_setupComponents.Count > 0)
                    {
                        setupState = BattleSetupState.Processing;
                    }
                }
                return setupState;
            }
            return BattleSetupState.Success;
        }

        void RunUpdateComponents(float dt)
        {
            for(int i = 0; i < _updateComponents.Count && _battleStep != BattleStep.Cleanup; i++)
            {
                _updateComponents[i].Update(dt);
            }
        }

        void RunCleanupComponents()
        {
            for(int i = 0; i < _cleanupComponents.Count; i++)
            {
                _cleanupComponents[i].Cleanup();
            }

            _cleanupComponents.Clear();
        }

        void StopComponents()
        {
            ResetStopComponentsListener();

            _successfulStopEventsCount = 0;
            _totalStopEventsCount = 0;
            OnStopCountUpdate();

            for(int i = 0; i < _stopComponents.Count; i++)
            {
                _stopComponents[i].Stop();
            }
        }

        void ResetStopComponentsListener()
        {
            for(int i = 0; i < _stopComponents.Count; i++)
            {
                _stopComponents[i].UnregisterListener(this);
                _stopComponents[i].RegisterListener(this);
            }
        }

        public void ProcessAction(object action)
        {
            if(_battleStep == BattleStep.Update)
            {
                _actions.Process(action);
            }
            else
            {
                Log.d("Trying to process action while the battle is not running!");
            }
        }

        void OnStopCountUpdate()
        {
            if(_totalStopEventsCount == _stopComponents.Count)
            {
                var successful = _successfulStopEventsCount == _totalStopEventsCount;
                for(int i = 0; i < _stopListeners.Count; i++)
                {
                    _stopListeners[i].OnStopped(successful);
                }
            }
        }

        void IBattleStopListener.OnStopped(bool successful)
        {
            ++_totalStopEventsCount;
            if(successful)
            {
                ++_successfulStopEventsCount;
            }
            OnStopCountUpdate();
        }

        void IBattleErrorDispatcher.RegisterHandler(IBattleErrorHandler handler)
        {
            _errorDispatcher.RegisterHandler(handler);
        }

        void IBattleErrorHandler.OnError(BattleError battleError)
        {
            _errorDispatcher.DispatchError(battleError);
            Dispose();
        }

        public T RegisterComponent<T>(T component) where T : class
        {
            RegisterSetupComponent(component as IBattleSetup);
            RegisterStartComponent(component as IBattleStart);
            RegisterUpdateComponent(component as IBattleUpdate);
            RegisterCleanupComponent(component as IBattleCleanup);
            RegisterStopComponent(component as IBattleStop);
            RegisterStopListener(component as IBattleStopListener);
            RegisterErrorDispatcherComponent(component as IBattleErrorDispatcher);
            RegisterErrorHandlerComponent(component as IBattleErrorHandler);
            return component;
        }

        public void RegisterSetupComponent(IBattleSetup setupComp)
        {
            if(setupComp != null)
            {
                _setupComponents.Enqueue(setupComp);
            }
        }

        public void RegisterStartComponent(IBattleStart startComp)
        {
            if (startComp != null)
            {
                _startComponents.Add(startComp);
            }
        }

        public void RegisterUpdateComponent(IBattleUpdate updateComp)
        {
            if(updateComp != null)
            {
                _updateComponents.Add(updateComp);
            }
        }

        public void RegisterCleanupComponent(IBattleCleanup cleanupComp)
        {
            if(cleanupComp != null)
            {
                _cleanupComponents.Add(cleanupComp);
            }
        }

        public void RegisterStopComponent(IBattleStop stopComp)
        {
            if(stopComp != null)
            {
                _stopComponents.Add(stopComp);
                stopComp.RegisterListener(this);
            }
        }

        public void RegisterStopListener(IBattleStopListener listener)
        {
            if(listener != null)
            {
                _stopListeners.Add(listener);
            }
        }

        public void UnregisterStopListener(IBattleStopListener listener)
        {
            if(listener != null)
            {
                _stopListeners.Remove(listener);
            }
        }

        public void RegisterErrorDispatcherComponent(IBattleErrorDispatcher errDispatcherComp)
        {
            if(errDispatcherComp != null)
            {
                errDispatcherComp.RegisterHandler(this);
            }
        }

        public void RegisterErrorHandlerComponent(IBattleErrorHandler errHandlerComp)
        {
            if(errHandlerComp != null)
            {
                ((IBattleErrorDispatcher)this).RegisterHandler(errHandlerComp);
            }
        }

        protected void RegisterValidator<T>(IActionValidator<T> validator)
        {
            if(validator != null)
            {
                _actions.RegisterValidator(validator);
            }
        }

        protected void RegisterSuccessHandler<T>(IActionHandler<T> handler)
        {
            if(handler != null)
            {
                _actions.RegisterSuccessHandler(handler);
            }
        }

        protected void RegisterFailureHandler<T>(IActionHandler<T> handler)
        {
            if(handler != null)
            {
                _actions.RegisterFailureHandler(handler);
            }
        }

        protected void RegisterValidator<T, R>(IActionValidator<T, R> validator)
        {
            if(validator != null)
            {
                _actions.RegisterValidator(validator);
            }
        }

        protected void RegisterSuccessHandler<T, R>(IResultActionHandler<T, R> handler)
        {
            if(handler != null)
            {
                _actions.RegisterSuccessHandler(handler);
            }
        }

        protected void RegisterFailureHandler<T, R>(IResultActionHandler<T, R> handler)
        {
            if(handler != null)
            {
                _actions.RegisterFailureHandler(handler);
            }
        }
    }
}
