//-----------------------------------------------------------------------
// ScriptEventTrigger.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.Attributes;
using SocialPoint.Dependency;
using SocialPoint.Lifecycle;
using SocialPoint.ScriptEvents;
using SocialPoint.Tutorial;
using UnityEngine;

public class ScriptEventTrigger : MonoBehaviour, IScriptEventsBridge
{
    public struct TutorialTriggerEvent
    {
    }

    public sealed class TutorialTriggerEventSerializer : BaseScriptEventSerializer<TutorialTriggerEvent>
    {
        public TutorialTriggerEventSerializer() : base("tutorial_trigger")
        {
        }

        protected override Attr SerializeEvent(TutorialTriggerEvent ev)
        {
            return new AttrString(Name);
        }
    }

    IEventProcessor _processor;
    TutorialManager _tutorialManager;
    bool _initialised;
    bool _triggerTutorial;

    void Update()
    {
        if(_initialised == false)
        {
            try
            {
                Init();
                _initialised = true;
            }
            catch
            {
                /* Not exactly production code but works for a sample */
            }
        }

        if(_triggerTutorial)
        {
            _processor.Process(new TutorialTriggerEvent());
            _triggerTutorial = false;
        }
    }

    void Init()
    {
        _tutorialManager = Services.Instance.Resolve<TutorialManager>();
        _tutorialManager.TutorialCompleted += OnTutorialCompleted;

        var processor = Services.Instance.Resolve<IScriptEventProcessor>();
        processor.RegisterBridge(this);
    }

    public void Load(IScriptEventProcessor scriptProcessor, IEventProcessor processor)
    {
        _processor = processor;
        scriptProcessor.RegisterSerializer(new TutorialTriggerEventSerializer());
    }

    void OnTutorialCompleted(string tutorialName)
    {
        if(tutorialName.Equals("Example 1"))
        {
            // If we trigger the event that sets the new tutorial on the next frame
            // Maybe we should have the option to queue new tutorials?!?
            _triggerTutorial = true;
        }
    }

    public void Dispose()
    {
        _tutorialManager.TutorialCompleted -= OnTutorialCompleted;
    }
}
