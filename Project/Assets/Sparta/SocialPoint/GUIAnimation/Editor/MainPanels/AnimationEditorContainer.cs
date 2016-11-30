using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public sealed class AnimationEditorContainer
    {
        GUIAnimationTool _animationTool;

        static readonly AnimationTimelinePanel _timelinePanel = new AnimationTimelinePanel();
        readonly AnimationPropertiesPanel _propertiesPanel = new AnimationPropertiesPanel();

        bool _isInit;

        public void Render(GUIAnimationTool animationTool)
        {
            _animationTool = animationTool;

            if(!_isInit)
            {
                Init();
                _isInit = true;
            }

            DoRender();
        }

        public static void ResetTimeLine()
        {
            _timelinePanel.SetTimeAt(0f);
        }

        public void ResetState()
        {
            _timelinePanel.ResetState();

            _propertiesPanel.ResetState();
            _propertiesPanel.SetBoxEditor(_timelinePanel);
        }

        void Init()
        {
            _propertiesPanel.SetBoxEditor(_timelinePanel);
        }

        void DoRender()
        {
            var timelineContainerRect = new Rect(0f, 0f, _animationTool.position.width, _animationTool.position.height * 0.6f);
            var propertiesContainerRect = new Rect(0f, _animationTool.position.height * 0.6f, _animationTool.position.width, (_animationTool.position.height - _animationTool.position.height * 0.6f));

            GUI.BeginGroup(timelineContainerRect);
            _timelinePanel.Render(_animationTool, timelineContainerRect);
            GUI.EndGroup();

            GUI.BeginGroup(propertiesContainerRect);
            _propertiesPanel.Render(_animationTool, propertiesContainerRect);
            GUI.EndGroup();
        }
    }
}
