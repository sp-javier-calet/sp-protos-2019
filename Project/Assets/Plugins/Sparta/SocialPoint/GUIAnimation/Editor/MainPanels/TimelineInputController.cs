using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public class TimelineInputController
    {
        AnimationTimelinePanel _boxEditorContainer;

        protected AnimationTimelinePanel BoxEditorContainer { get { return _boxEditorContainer; } }

        public void Init(AnimationTimelinePanel boxEditorContainer)
        {
            _boxEditorContainer = boxEditorContainer;
        }

        public void Update()
        {
            // Check Exit
            if(BoxEditorContainer.SelectedStep == null)
            {
                return;
            }
			
            if(TryDeleteSelectedAnimationItems())
            {
                return;
            }
			
            if(TryDuplicateSelectedAnimationItems())
            {
                return;
            }
			
            if(TryGroupAnimationItems())
            {
                return;
            }
			
            if(TryUngroupAnimationItems())
            {
                return;
            }
        }

        bool TryDeleteSelectedAnimationItems()
        {
            if(Event.current.type == EventType.KeyDown)
            {
                bool isCommandPressed = (GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftCommand) || GUIAnimationTool.KeyController.IsPressed(KeyCode.RightCommand));

                if((GUIAnimationTool.KeyController.IsPressed(KeyCode.Delete) || (isCommandPressed && Event.current.keyCode == KeyCode.Backspace))
                   && !GUIAnimationTool.Blackboard.CompareValue(AnimationBlackboardKey.FocusPanelKey, AnimationBlackboardValue.EasingGridPanel)
                   && BoxEditorContainer.SelectedStep != null)
                {
                    BoxEditorContainer.RemoveSelectedAnimationItems();
                    return true;
                }
            }

            return false;
        }

        bool TryDuplicateSelectedAnimationItems()
        {
            if(Event.current.type == EventType.KeyDown
               && (GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftControl) || GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftCommand))
               && GUIAnimationTool.KeyController.IsPressed(KeyCode.D)
               && BoxEditorContainer.SelectedStep != null)
            {
                BoxEditorContainer.DuplicateSelectedAnimationItems();
                return true;
            }
			
            return false;
        }

        bool TryGroupAnimationItems()
        {
            if(Event.current.type == EventType.KeyDown
               && (GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftControl) || GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftCommand))
               && GUIAnimationTool.KeyController.IsPressed(KeyCode.G)
               && BoxEditorContainer.SelectedStep != null)
            {
                BoxEditorContainer.GroupSelection();
                return true;
            }
			
            return false;
        }

        bool TryUngroupAnimationItems()
        {
            if(Event.current.type == EventType.KeyDown
               && (GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftControl) || GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftCommand))
               && GUIAnimationTool.KeyController.IsPressed(KeyCode.U)
               && BoxEditorContainer.SelectedStep != null)
            {
                BoxEditorContainer.UngroupSelection();
                return true;
            }
			
            return false;
        }
    }
}
