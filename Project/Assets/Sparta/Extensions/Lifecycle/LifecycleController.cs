using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Lifecycle
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

    public interface ICleanSetupComponent : ISetupComponent, ICleanupComponent
    {
    }

    public interface ICancelListener
    {
        void OnCancelled(bool successful);
    }

    public interface ICancelComponent
    {
        ICancelListener Listener { get; set; }

        void Cancel();
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

    public class LifecycleController : IDeltaUpdateable, ICancelListener, IErrorHandler, IDisposable
    {
        List<ISetupComponent> _setupComponents;
        List<IUpdateComponent> _updateComponents;
        List<ICleanupComponent> _cleanupComponents;
        List<IStartComponent> _startComponents;
        List<ICancelComponent> _cancelComponents;
        List<ICancelListener> _cancelListeners;
        List<IErrorDispatcher> _errorDispatchers;
        List<IErrorHandler> _errorHandlers;

        IUpdateScheduler _scheduler;

        int _currentSetupComponent;
        bool _currentSetupComponentStarted;

        int _successfulCancelEventsCount;
        int _totalCancelEventsCount;

        public enum PhaseType
        {
            Stopped,
            Setup,
            Start,
            Update,
            Cleanup,
        }

        public PhaseType Phase { get; private set; }

        public bool DisposeAfterCancel = true;

        public LifecycleController(IUpdateScheduler scheduler = null)
        {
            _setupComponents = new List<ISetupComponent>();
            _startComponents = new List<IStartComponent>();
            _updateComponents = new List<IUpdateComponent>();
            _cleanupComponents = new List<ICleanupComponent>();
            _cancelComponents = new List<ICancelComponent>();
            _cancelListeners = new List<ICancelListener>();
            _errorDispatchers = new List<IErrorDispatcher>();
            _errorHandlers = new List<IErrorHandler>();
            Phase = PhaseType.Stopped;
            _scheduler = scheduler;
            _successfulCancelEventsCount = 0;
            _totalCancelEventsCount = 0;

            RegisterComponents();
        }

        protected virtual void RegisterComponents()
        {
        }

        public void Start()
        {
            if(Phase != PhaseType.Stopped)
            {
                throw new InvalidOperationException("Controller can only be started while stopped");
            }
            Phase = PhaseType.Setup;
            _currentSetupComponent = 0;
            _currentSetupComponentStarted = false;
            if(_scheduler != null)
            {
                _scheduler.Add(this, UpdateableTimeMode.GameTimeScaled, 0.0f);
            }
        }

        public void Cancel()
        {
            if(Phase != PhaseType.Setup)
            {
                throw new InvalidOperationException("Controller can only be stopped during setup.");
            }
            for(int i = 0; i < _cancelComponents.Count; i++)
            {
                _cancelComponents[i].Listener = this;
            }
            _successfulCancelEventsCount = 0;
            _totalCancelEventsCount = 0;
            CheckCancelCount();
            for(int i = 0; i < _cancelComponents.Count; i++)
            {
                _cancelComponents[i].Cancel();
            }
        }

        public void Dispose()
        {
            Phase = PhaseType.Cleanup;
            if(_scheduler != null)
            {
                _scheduler.Remove(this);
            }
            for(int i = 0; i < _cleanupComponents.Count; i++)
            {
                var cleanup = _cleanupComponents[i];
                var setup = cleanup as ICleanSetupComponent;
                var idx = -1;
                if(setup != null)
                {
                    idx = _setupComponents.IndexOf(setup);
                }
                if(idx < 0 || idx <= _currentSetupComponent)
                {
                    cleanup.Cleanup();
                }
            }
            _cleanupComponents.Clear();
            for(int i = 0; i < _errorDispatchers.Count; i++)
            {
                _errorDispatchers[i].Handler = null;
            }
            _errorDispatchers.Clear();
            for(int i = 0; i < _cancelComponents.Count; i++)
            {
                _cancelComponents[i].Listener = null;
            }
            _cancelComponents.Clear();
            _setupComponents.Clear();
            _startComponents.Clear();
            _updateComponents.Clear();
            _cancelListeners.Clear();
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

        void CheckCancelCount()
        {
            if(_totalCancelEventsCount < _cancelComponents.Count)
            {
                return;
            }
            var successful = _successfulCancelEventsCount == _totalCancelEventsCount;
            for(int i = 0; i < _cancelListeners.Count; i++)
            {
                _cancelListeners[i].OnCancelled(successful);
            }
            if(successful)
            {
                Phase = PhaseType.Stopped;
                if(_scheduler != null)
                {
                    _scheduler.Remove(this);
                }
                if(DisposeAfterCancel)
                {
                    Dispose();
                }
            }
        }

        void ICancelListener.OnCancelled(bool successful)
        {
            ++_totalCancelEventsCount;
            if(successful)
            {
                ++_successfulCancelEventsCount;
            }
            CheckCancelCount();
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
            RegisterCancelComponent(component as ICancelComponent);
            RegisterCancelListener(component as ICancelListener);
            RegisterErrorDispatcher(component as IErrorDispatcher);
            RegisterErrorHandler(component as IErrorHandler);
            return component;
        }

        public void RegisterSetupComponent(ISetupComponent setup)
        {
            if(setup != null && !_setupComponents.Contains(setup))
            {
                _setupComponents.Add(setup);
            }
        }

        public void RegisterStartComponent(IStartComponent start)
        {
            if (start != null && !_startComponents.Contains(start))
            {
                _startComponents.Add(start);
            }
        }

        public void RegisterUpdateComponent(IUpdateComponent update)
        {
            if(update != null && !_updateComponents.Contains(update))
            {
                _updateComponents.Add(update);
            }
        }

        public void RegisterCleanupComponent(ICleanupComponent cleanup)
        {
            if(cleanup != null && !_cleanupComponents.Contains(cleanup))
            {
                _cleanupComponents.Add(cleanup);
            }
        }

        public void RegisterCancelComponent(ICancelComponent cancel)
        {
            if(cancel != null && !_cancelComponents.Contains(cancel))
            {
                _cancelComponents.Add(cancel);
            }
        }

        public void RegisterCancelListener(ICancelListener listener)
        {
            if(listener != null && !_cancelListeners.Contains(listener))
            {
                _cancelListeners.Add(listener);
            }
        }

        public void RegisterErrorDispatcher(IErrorDispatcher dispatcher)
        {
            if(dispatcher != null && !_errorDispatchers.Contains(dispatcher))
            {
                dispatcher.Handler = this;
                _errorDispatchers.Add(dispatcher);
            }
        }

        public void RegisterErrorHandler(IErrorHandler handler)
        {
            if(handler != null && !_errorHandlers.Contains(handler))
            {
                _errorHandlers.Add(handler);
            }
        }
    }
}
