using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

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

    public class ScriptStepModelParser : IParser<ScriptStepModel>
    {
        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";
        const string AttrKeyForward = "forward";
        const string AttrKeyBackward = "backward";

        IParser<IScriptCondition> _conditionParser;

        public ScriptStepModelParser(IParser<IScriptCondition> conditionParser)
        {
            _conditionParser = conditionParser;
        }

        public ScriptStepModelParser():
            this(ScriptConditions.BaseParser)
        {
        }

        public ScriptStepModel Parse(Attr data)
        {
            return new ScriptStepModel
            {
                Name = data.AsDic[AttrKeyName].ToString(),
                Arguments = (Attr)data.AsDic[AttrKeyArguments].Clone(),
                Forward = _conditionParser.Parse(data.AsDic[AttrKeyForward]),
                Backward = _conditionParser.Parse(data.AsDic[AttrKeyBackward])
            };
        }
    }

    public class ScriptStepModelsParser : IParser<List<ScriptStepModel>>
    {
        IParser<ScriptStepModel> _stepParser;

        public ScriptStepModelsParser():
            this(ScriptConditions.BaseParser)
        {
        }

        public ScriptStepModelsParser(IParser<IScriptCondition> conditionParser):
            this(new ScriptStepModelParser(conditionParser))
        {
        }

        public ScriptStepModelsParser(IParser<ScriptStepModel> stepParser)
        {
            _stepParser = stepParser;
        }

        public List<ScriptStepModel> Parse(Attr data)
        {
            var steps = new List<ScriptStepModel>();
            foreach(var step in data.AsList)
            {
                steps.Add(_stepParser.Parse(step));
            }
            return steps;
        }
    }

    public class ScriptParser : IParser<Script>
    {
        IParser<List<ScriptStepModel>> _stepsParser;
        IScriptEventDispatcher _dispatcher;
        
        public ScriptParser(IScriptEventDispatcher dispatcher):
            this(ScriptConditions.BaseParser, dispatcher)
        {
        }

        public ScriptParser(IParser<IScriptCondition> conditionParser, IScriptEventDispatcher dispatcher):
            this(new ScriptStepModelsParser(conditionParser), dispatcher)
        {
        }

        public ScriptParser(IParser<List<ScriptStepModel>> stepsParser, IScriptEventDispatcher dispatcher)
        {
            _stepsParser = stepsParser;
            _dispatcher = dispatcher;
        }
                
        public Script Parse(Attr data)
        {
            return new Script(_dispatcher, _stepsParser.Parse(data));
        }
    }

    public class ScriptStep
    {
        ScriptStepModel _model;
        Action<Decision, string, Attr> _callback;
        IScriptEventDispatcher _dispatcher;
        bool _raiseReceived;

        public enum Decision
        {
            Forward,
            Backward
        }

        public ScriptStep(IScriptEventDispatcher dispatcher, ScriptStepModel model)
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
            _dispatcher.RemoveListener(OnEvent);
            _raiseReceived = false;
            _callback = null;
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
            bool raised = !_raiseReceived && name == _model.Name && args.Equals(_model.Arguments);
            if(raised)
            {
                _raiseReceived = true;
            }
            if(_model.Forward == null || _model.Forward.Matches(name, args))
            {
                Finish(Decision.Forward, name, args);
            }
            else if(_model.Backward == null && raised)
            {
                // this happens when no backward events defined
                // we don't want the raised event to finish the step backwards
            }
            else if(_model.Backward == null || _model.Backward.Matches(name, args))
            {
                Finish(Decision.Backward, name, args);
            }
        }

        public void Run(Action<Decision, string, Attr> finished)
        {
            if(IsRunning)
            {
                throw new InvalidOperationException("Step already running.");
            }
            _callback = finished;
            if(_model.Forward != null)
            {
                _dispatcher.AddListener(OnEvent);
            }
            _dispatcher.Raise(_model.Name, _model.Arguments);
            if(_model.Forward == null)
            {
                Finish(Decision.Forward, null, null);
            }
        }
    }
    
    public class Script
    {
        readonly List<ScriptStep> _steps = new List<ScriptStep>();
        IScriptEventDispatcher _dispatcher;
        Action _finished;

        public int CurrentStepNum { get; private set; }

        public ScriptStep CurrentStep
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

        public Script(IScriptEventDispatcher dispatcher, ScriptStepModel[] stepModels) :
            this(dispatcher, new List<ScriptStepModel>(stepModels))
        {
        }
        
        public Script(IScriptEventDispatcher dispatcher, List<ScriptStepModel> stepModels)
        {
            if(dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }
            _dispatcher = dispatcher;
            foreach(var stepModel in stepModels)
            {
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

        public void Run(Action finished)
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
            step.Run(OnStepDecision);
        }

        void OnStepDecision(ScriptStep.Decision decision, string evName, Attr evArgs)
        {
            switch(decision)
            {
            case ScriptStep.Decision.Forward:
                CurrentStepNum++;
                break;
            case ScriptStep.Decision.Backward:
                CurrentStepNum--;
                break;
            default:
                break;
            }
            RunCurrentStep();
        }

        public override string ToString()
        {
            var str = new string[_steps.Count];
            var i = 0;
            foreach(var step in _steps)
            {
                str[i] = step.ToString();
            }
            return string.Format("[Script:{0}]", string.Join(", ", str));
        }
    }
}