using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	public class AnimationEditorContainer
	{
		GUIAnimationTool _animationTool;

		AnimationTimelinePanel _timelinePanel = new AnimationTimelinePanel();
		AnimationPropertiesPanel _propertiesPanel = new AnimationPropertiesPanel();

		bool _isInit = false;

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
			Rect timelineContainerRect = 	new Rect(0f, 0f, _animationTool.position.width, _animationTool.position.height * 0.6f);
			Rect propertiesContainerRect = 	new Rect(0f, _animationTool.position.height * 0.6f, _animationTool.position.width, (_animationTool.position.height-_animationTool.position.height * 0.6f));

			GUI.BeginGroup(timelineContainerRect);
			_timelinePanel.Render(_animationTool, timelineContainerRect);
			GUI.EndGroup();

			GUI.BeginGroup(propertiesContainerRect);
			_propertiesPanel.Render(_animationTool, propertiesContainerRect);
			GUI.EndGroup();
		}
	}
}
