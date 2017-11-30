using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Components;
using System;

namespace SocialPoint.Components
{
    public enum LifecyclePhase
    {
        None,
        Setup,
        Start,
        Update,
        Cleanup,
    }

    public interface ISetupComponent
    {
        void Start();
        void Update(float dt);
        bool Finished { get; }
    }

    public interface IUpdateComponent
    {
        void Update(float dt);
    }

    public interface ICleanupComponent
    {
        void Cleanup();
    }

    public interface IStopListener
    {
        void OnStopped(bool successful);
    }

    public interface IStopComponent
    {
        IStopListener Listener { get; set; }

        void Stop();
    }

    public interface IStartComponent
    {
        void Start();
    }

    public interface IErrorHandler
    {
        void OnError(BattleError battleError);
    }

    public interface IErrorDispatcher
    {
        IErrorHandler Handler { get; set; }
    }

    public class LifecycleController : IDeltaUpdateable, IStopListener, IErrorHandler, IDisposable
    {
        ISetupComponent _currentSetupComponent;
        Queue<ISetupComponent> _setupComponents;
        List<IUpdateComponent> _updateComponents;
        List<ICleanupComponent> _cleanupComponents;
        List<IStartComponent> _startComponents;
        List<IStopComponent> _stopComponents;
        List<IStopListener> _stopListeners;
        List<IErrorHandler> _errorHandlers;

        ActionProcessor _actions;
        protected IUpdateScheduler _scheduler;
        int _successfulStopEventsCount;
        int _totalStopEventsCount;
        bool _componentsRegistered;

        public LifecyclePhase Phase { get; private set; }

        public LifecycleController(IUpdateScheduler scheduler)
        {
            _setupComponents = new Queue<ISetupComponent>();
            _startComponents = new List<IStartComponent>();
            _updateComponents = new List<IUpdateComponent>();
            _cleanupComponents = new List<ICleanupComponent>();
            _stopComponents = new List<IStopComponent>();
            _stopListeners = new List<IStopListener>();
            _actions = new ActionProcessor();
            Phase = LifecyclePhase.None;
            _scheduler = scheduler;
            _successfulStopEventsCount = 0;
            _totalStopEventsCount = 0;
        }

        protected virtual void RegisterComponents()
        {
        }

        public void Start()
        {
            if(!_componentsRegistered)
            {
                _componentsRegistered = true;
                RegisterComponents();
            }
            Phase = LifecyclePhase.Setup;
            _scheduler.Add(this, UpdateableTimeMode.GameTimeScaled);
        }

        public void Stop()
        {
            StopComponents();
        }

        public void Dispose()
        {
            Phase = LifecyclePhase.Cleanup;
            _scheduler.Remove(this);
            RunCleanupComponents();
        }

        public void Update(float dt)
        {
            switch(Phase)
            {
            case LifecyclePhase.Setup:
                UpdateSetupStep(dt);
                break;
            case LifecyclePhase.Start:
                RunStartStep();
                break;
            case LifecyclePhase.Update:
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
            case SetupStepState.Success:
                _phase = LifecyclePhase.Start;
                break;
            default:
                break;
            }
        }

        void RunStartStep()
        {
            for(int i = 0; i < _startComponents.Count && _phase != LifecyclePhase.Cleanup; i++)
            {
                _startComponents[i].Start();
            }
            _phase = LifecyclePhase.Update;
        }

        SetupStepState RunSetupComponents(float dt)
        {
            if(_currentSetupComponent == null && _setupComponents.Count > 0)
            {
                _currentSetupComponent = _setupComponents.Dequeue();
                _currentSetupComponent.Start();
            }
            if(_currentSetupComponent != null)
            {
                var setupState = _currentSetupComponent.Update(dt);
                if(setupState == SetupStepState.Success)
                {
                    _currentSetupComponent = null;
                    if(_setupComponents.Count > 0)
                    {
                        setupState = SetupStepState.Processing;
                    }
                }
                return setupState;
            }
            return SetupStepState.Success;
        }

        void RunUpdateComponents(float dt)
        {
            for(int i = 0; i < _updateComponents.Count && _phase != LifecyclePhase.Cleanup; i++)
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
            if(_phase == LifecyclePhase.Update)
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

        void IStopListener.OnStopped(bool successful)
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
            RegisterSetupComponent(component as ISetupComponent);
            RegisterStartComponent(component as IStartComponent);
            RegisterUpdateComponent(component as IUpdateComponent);
            RegisterCleanupComponent(component as ICleanupComponent);
            RegisterStopComponent(component as IStopComponent);
            RegisterStopListener(component as IStopListener);
            RegisterErrorDispatcherComponent(component as IBattleErrorDispatcher);
            RegisterErrorHandlerComponent(component as IBattleErrorHandler);
            return component;
        }

        public void RegisterSetupComponent(ISetupComponent setupComp)
        {
            if(setupComp != null)
            {
                _setupComponents.Enqueue(setupComp);
            }
        }

        public void RegisterStartComponent(IStartComponent startComp)
        {
            if (startComp != null)
            {
                _startComponents.Add(startComp);
            }
        }

        public void RegisterUpdateComponent(IUpdateComponent updateComp)
        {
            if(updateComp != null)
            {
                _updateComponents.Add(updateComp);
            }
        }

        public void RegisterCleanupComponent(ICleanupComponent cleanupComp)
        {
            if(cleanupComp != null)
            {
                _cleanupComponents.Add(cleanupComp);
            }
        }

        public void RegisterStopComponent(IStopComponent stopComp)
        {
            if(stopComp != null)
            {
                _stopComponents.Add(stopComp);
                stopComp.RegisterListener(this);
            }
        }

        public void RegisterStopListener(IStopListener listener)
        {
            if(listener != null)
            {
                _stopListeners.Add(listener);
            }
        }

        public void UnregisterStopListener(IStopListener listener)
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
