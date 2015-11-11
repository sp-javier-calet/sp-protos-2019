using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
    public struct EventScriptStepModel
    {
        public string EventName;
        public Attr EventArguments;
        public IScriptCondition Condition;

        public override string ToString()
        {
            return string.Format("[EventScriptStepModel: Event={0},{1} Condition={2}]", EventName, EventArguments, Condition);
        }
    }

    public class EventScriptStep
    {
        EventScriptStepModel _model;
        Action<string, Attr> _callback;
        IScriptEventDispatcher _dispatcher;

        public EventScriptStep(IScriptEventDispatcher dispatcher, EventScriptStepModel model)
        {
            if(dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }
            _dispatcher = dispatcher;
            _model = model;
        }

        public override string ToString()
        {
            return string.Format("[EventScriptStep: Event={0},{1} Condition={2}]", _model.EventName, _model.EventArguments, _model.Condition);
        }

        public void Reset()
        {
            _dispatcher.RemoveListener(_callback);
            _callback = null;
        }

        public bool IsRunning
        {
            get
            {
                return _callback != null;
            }
        }

        public void Run(Action<string, Attr> finished)
        {
            if(IsRunning)
            {
                throw new InvalidOperationException("Step already running.");
            }
            _callback = (evName, evArgs) => {
                Reset();
                if(finished != null)
                {
                    finished(evName, evArgs);
                }
            };
            _dispatcher.AddListener(_model.Condition, _callback);
            _dispatcher.Raise(_model.EventName, _model.EventArguments);
        }
    }
    
    public class EventScript
    {
        readonly List<EventScriptStep> _steps = new List<EventScriptStep>();
        IScriptEventDispatcher _dispatcher;

        public int CurrentStepNum { get; private set; }

        public EventScriptStep CurrentStep
        {
            get
            {
                if(IsRunning)
                {
                    return _steps[CurrentStepNum];
                }
                return null;
            }
        }

        public int StepsCount
        {
            get
            {
                return _steps.Count;
            }
        }
                
        public bool IsRunning
        {
            get
            {
                return CurrentStepNum >= 0 && CurrentStepNum < _steps.Count;
            }
        }

        public bool IsFinished
        {
            get
            {
                return CurrentStepNum >= _steps.Count;
            }
        }

        public EventScript(IScriptEventDispatcher dispatcher, EventScriptStepModel[] stepModels) :
            this(dispatcher, new List<EventScriptStepModel>(stepModels))
        {
        }
        
        public EventScript(IScriptEventDispatcher dispatcher, List<EventScriptStepModel> stepModels)
        {
            if(dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }
            _dispatcher = dispatcher;
            foreach(var stepModel in stepModels)
            {
                _steps.Add(new EventScriptStep(_dispatcher, stepModel));
            }
            Reset();
        }

        public void Reset()
        {
            var step = CurrentStep;
            if(step != null)
            {
                step.Reset();
            }
            CurrentStepNum = -1;
        }

        public void Run(Action finished)
        {
            if(IsRunning)
            {
                throw new InvalidOperationException("Already running.");
            }
            DoRun(finished);
        }

        void DoRun(Action finished)
        {
            CurrentStepNum++;
            var step = CurrentStep;
            if(step == null)
            {
                if(finished != null)
                {
                    finished();
                }
                return;
            }
            step.Run((evName, evArgs) => {
                DoRun(finished);
            });
        }

        public override string ToString()
        {
            var str = new string[_steps.Count];
            var i = 0;
            foreach(var step in _steps)
            {
                str[i] = step.ToString();
            }
            return string.Format("[EventScript:{0}]", string.Join(", ", str));
        }
    }
}