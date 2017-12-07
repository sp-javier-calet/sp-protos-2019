using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Utils;

namespace SocialPoint.ScriptEvents
{
    public struct ScriptStepModel
    {
        public string Name;
        public Attr Arguments;
        public IScriptCondition Forward;
        public IScriptCondition Backward;

        public override string ToString()
        {
            return string.Format("[ScriptStepModel: Event={0},{1} Forward={2} Backward={3}]",
                Name, Arguments, Forward, Backward);
        }
    }

    public struct ScriptModel
    {
        public ScriptStepModel[] Steps;

        public override string ToString()
        {
            return string.Format("[ScriptModel:{0}]", StringUtils.Join(Steps));
        }
    }
        
    public sealed class ScriptStepModelParser : IAttrObjParser<ScriptStepModel>
    {
        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";
        const string AttrKeyForward = "forward";
        const string AttrKeyBackward = "backward";

        readonly IAttrObjParser<IScriptCondition> _conditionParser;

        public ScriptStepModelParser(IAttrObjParser<IScriptCondition> conditionParser)
        {
            _conditionParser = conditionParser;
        }

        public ScriptStepModelParser() :
            this(ScriptConditions.BaseParser)
        {
        }

        public ScriptStepModel Parse(Attr data)
        {
            return new ScriptStepModel {
                Name = data.AsDic[AttrKeyName].ToString(),
                Arguments = (Attr)data.AsDic[AttrKeyArguments].Clone(),
                Forward = _conditionParser.Parse(data.AsDic[AttrKeyForward]),
                Backward = _conditionParser.Parse(data.AsDic[AttrKeyBackward])
            };
        }
    }
        
    public sealed class ScriptModelParser : IAttrObjParser<ScriptModel>
    {
        readonly IAttrObjParser<ScriptStepModel> _stepParser;

        public ScriptModelParser() :
            this(ScriptConditions.BaseParser)
        {
        }

        public ScriptModelParser(IAttrObjParser<IScriptCondition> conditionParser) :
            this(new ScriptStepModelParser(conditionParser))
        {
        }

        public ScriptModelParser(IAttrObjParser<ScriptStepModel> stepParser)
        {
            _stepParser = stepParser;
        }

        public ScriptModel Parse(Attr data)
        {
            var steps = new List<ScriptStepModel>();
            var itr = data.AsList.GetEnumerator();
            while(itr.MoveNext())
            {
                var step = itr.Current;
                steps.Add(_stepParser.Parse(step));
            }
            itr.Dispose();

            return new ScriptModel{ Steps = steps.ToArray() };
        }
    }

    public sealed class ScriptStep
    {
        ScriptStepModel _model;
        Action<Decision, string, Attr> _callback;
        readonly IScriptEventProcessor _dispatcher;
        bool _eventRaised;

        public enum Decision
        {
            Forward,
            Backward
        }

        public ScriptStep(IScriptEventProcessor dispatcher, ScriptStepModel model)
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
            return string.Format("[ScriptStep: Event={0},{1} Forward={2} Backward={3}]", _model.Name, _model.Arguments, _model.Forward, _model.Backward);
        }

        public void Reset()
        {
            _dispatcher.UnregisterHandler(OnEvent);
            _callback = null;
            _eventRaised = false;
        }

        public bool IsRunning
        {
            get
            {
                return _callback != null;
            }
        }

        void Finish(Decision decision, string name, Attr args)
        {
            var callback = _callback;
            Reset();
            if(callback != null)
            {
                callback(decision, name, args);
            }
        }

        void OnEvent(string name, Attr args)
        {
            if(_model.Forward == null || _model.Forward.Matches(name, args))
            {
                Finish(Decision.Forward, name, args);
            }
            else if(_eventRaised && _model.Backward != null && _model.Backward.Matches(name, args))
            {
                Finish(Decision.Backward, name, args);
            }
        }

        public void Run(Action<Decision, string, Attr> finished = null)
        {
            if(IsRunning)
            {
                throw new InvalidOperationException("Step already running.");
            }
            _callback = finished;
            if(_model.Forward != null)
            {
                _dispatcher.RegisterHandler(OnEvent);
            }
            _dispatcher.Process(_model.Name, _model.Arguments);
            _eventRaised = true;
            if(_model.Forward == null)
            {
                Finish(Decision.Forward, null, null);
            }
        }
    }

    public sealed class Script
    {
        readonly List<ScriptStep> _steps = new List<ScriptStep>();
        IScriptEventProcessor _dispatcher;
        Action _finished;

        public Action StepStarted;
        public Action<ScriptStep.Decision,string,Attr> StepFinished;

        public int CurrentStepNum { get; private set; }

        public ScriptStep CurrentStep
        {
            get
            {
                return IsRunning ? _steps[CurrentStepNum] : null;
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

        public Script(IScriptEventProcessor dispatcher, ScriptStepModel[] stepModels) :
            this(dispatcher, new ScriptModel{ Steps = stepModels })
        {
        }

        public Script(IScriptEventProcessor dispatcher, ScriptModel model)
        {
            if(dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }
            _dispatcher = dispatcher;
            for(int i = 0, modelStepsLength = model.Steps.Length; i < modelStepsLength; i++)
            {
                var stepModel = model.Steps[i];
                _steps.Add(new ScriptStep(_dispatcher, stepModel));
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

        public void Run(Action finished = null)
        {
            if(IsRunning)
            {
                throw new InvalidOperationException("Already running.");
            }
            _finished = finished;
            CurrentStepNum++;
            RunCurrentStep();
        }

        void RunCurrentStep()
        {
            var step = CurrentStep;
            if(step == null)
            {
                if(_finished != null)
                {
                    _finished();
                }
                return;
            }
            if(StepStarted != null)
            {
                StepStarted();
            }
            step.Run(OnStepDecision);
        }

        void OnStepDecision(ScriptStep.Decision decision, string evName, Attr evArgs)
        {
            if(StepFinished != null)
            {
                StepFinished(decision, evName, evArgs);
            }
            switch(decision)
            {
            case ScriptStep.Decision.Forward:
                CurrentStepNum++;
                break;
            case ScriptStep.Decision.Backward:
                CurrentStepNum--;
                break;
            }
            if(CurrentStepNum < 0)
            {
                CurrentStepNum = 0;
            }
            RunCurrentStep();
        }

        public override string ToString()
        {
            return string.Format("[Script:{0}]", StringUtils.Join(_steps));
        }
    }
}