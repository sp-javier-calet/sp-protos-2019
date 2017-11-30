using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Components;
using System;

namespace SocialPoint.Components
{
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
        void OnError(Error error);
    }

    public interface IErrorDispatcher
    {
        IErrorHandler Handler { get; set; }
    }

    public class LifecycleController : IDeltaUpdateable, IStopListener, IErrorHandler, IDisposable
    {
        List<ISetupComponent> _setupComponents;
        List<IUpdateComponent> _updateComponents;
        List<ICleanupComponent> _cleanupComponents;
        List<IStartComponent> _startComponents;
        List<IStopComponent> _stopComponents;
        List<IStopListener> _stopListeners;
        List<IErrorDispatcher> _errorDispatchers;
        List<IErrorHandler> _errorHandlers;

        IUpdateScheduler _scheduler;

        int _currentSetupComponent;
        bool _currentSetupComponentStarted;

        int _successfulStopEventsCount;
        int _totalStopEventsCount;

        public enum PhaseType
        {
            None,
            Setup,
            Start,
            Update,
            Cleanup,
        }

        public PhaseType Phase { get; private set; }

        public LifecycleController(IUpdateScheduler scheduler = null)
        {
            _setupComponents = new List<ISetupComponent>();
            _startComponents = new List<IStartComponent>();
            _updateComponents = new List<IUpdateComponent>();
            _cleanupComponents = new List<ICleanupComponent>();
            _stopComponents = new List<IStopComponent>();
            _stopListeners = new List<IStopListener>();
            _errorDispatchers = new List<IErrorDispatcher>();
            _errorHandlers = new List<IErrorHandler>();
            Phase = PhaseType.None;
            _scheduler = scheduler;
            _successfulStopEventsCount = 0;
            _totalStopEventsCount = 0;

            if(_scheduler != null)
            {
                _scheduler.Add(this, UpdateableTimeMode.GameTimeScaled, 0.0f);
            }
            RegisterComponents();
        }

        protected virtual void RegisterComponents()
        {
        }

        public void Start()
        {
            Phase = PhaseType.Setup;
            _currentSetupComponent = 0;
            _currentSetupComponentStarted = false;
        }

        public void Stop()
        {
            for(int i = 0; i < _stopComponents.Count; i++)
            {
                _stopComponents[i].Listener = this;
            }
            _successfulStopEventsCount = 0;
            _totalStopEventsCount = 0;
            OnStopCountUpdate();
            for(int i = 0; i < _stopComponents.Count; i++)
            {
                _stopComponents[i].Stop();
            }
        }

        public void Dispose()
        {
            Phase = PhaseType.Cleanup;
            _scheduler.Remove(this);
            for(int i = 0; i < _cleanupComponents.Count; i++)
            {
                _cleanupComponents[i].Cleanup();
            }
            _cleanupComponents.Clear();
            for(int i = 0; i < _errorDispatchers.Count; i++)
            {
                _errorDispatchers[i].Handler = null;
            }
            _errorDispatchers.Clear();
            for(int i = 0; i < _stopComponents.Count; i++)
            {
                _stopComponents[i].Listener = null;
            }
            _stopComponents.Clear();
            _setupComponents.Clear();
            _startComponents.Clear();
            _updateComponents.Clear();
            _stopListeners.Clear();
            _errorHandlers.Clear();
        }

        public void Update(float dt)
        {
            switch(Phase)
            {
            case PhaseType.Setup:
                RunSetupPhase(dt);
                break;
            case PhaseType.Start:
                RunStartPhase();
                break;
            case PhaseType.Update:
                RunUpdatePhase(dt);
                break;
            default:
                break;
            }
        }

        void RunSetupPhase(float dt)
        {
            if(_currentSetupComponent < _setupComponents.Count)
            {
                var comp = _setupComponents[_currentSetupComponent];
                if(!_currentSetupComponentStarted)
                {
                    _currentSetupComponentStarted = true;
                    comp.Start();
                }
                comp.Update(dt);
                if(comp.Finished)
                {
                    _currentSetupComponent++;
                    _currentSetupComponentStarted = false;
                }
            }
            if(_currentSetupComponent >= _setupComponents.Count)
            {
                Phase = PhaseType.Start;
            }
        }

        void RunStartPhase()
        {
            for(int i = 0; i < _startComponents.Count && Phase != PhaseType.Cleanup; i++)
            {
                _startComponents[i].Start();
            }
            Phase = PhaseType.Update;
        }

        void RunUpdatePhase(float dt)
        {
            for(int i = 0; i < _updateComponents.Count && Phase != PhaseType.Cleanup; i++)
            {
                _updateComponents[i].Update(dt);
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

        void IErrorHandler.OnError(Error error)
        {
            for(var i = 0; i < _errorHandlers.Count; i++)
            {
                _errorHandlers[i].OnError(error);
            }
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
            RegisterErrorDispatcher(component as IErrorDispatcher);
            RegisterErrorHandler(component as IErrorHandler);
            return component;
        }

        public void RegisterSetupComponent(ISetupComponent setup)
        {
            if(setup != null)
            {
                _setupComponents.Add(setup);
            }
        }

        public void RegisterStartComponent(IStartComponent start)
        {
            if (start != null)
            {
                _startComponents.Add(start);
            }
        }

        public void RegisterUpdateComponent(IUpdateComponent update)
        {
            if(update != null)
            {
                _updateComponents.Add(update);
            }
        }

        public void RegisterCleanupComponent(ICleanupComponent cleanup)
        {
            if(cleanup != null)
            {
                _cleanupComponents.Add(cleanup);
            }
        }

        public void RegisterStopComponent(IStopComponent stop)
        {
            if(stop != null)
            {
                _stopComponents.Add(stop);
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

        public void RegisterErrorDispatcher(IErrorDispatcher dispatcher)
        {
            if(dispatcher != null)
            {
                dispatcher.Handler = this;
                _errorDispatchers.Add(dispatcher);
            }
        }

        public void RegisterErrorHandler(IErrorHandler handler)
        {
            if(handler != null)
            {
                _errorHandlers.Add(handler);
            }
        }
    }
}
