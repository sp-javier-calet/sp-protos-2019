using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	public class AnimationEditorContainer
	{
		GUIAnimationTool _animationTool;

		AnimationTimelinePanel _boxContainer = new AnimationTimelinePanel();
		AnimationPropertiesPanel _animItemDetail = new AnimationPropertiesPanel();

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
			_boxContainer.ResetState();

			_animItemDetail.ResetState();
			_animItemDetail.SetBoxEditor(_boxContainer);
		}

		void Init()
		{
			_animItemDetail.SetBoxEditor(_boxContainer);
		}
		
		void DoRender()
		{
			_boxContainer.Render(_animationTool);
			_animItemDetail.Render(_animationTool, new Vector2(0f, _boxContainer.PanelHeight));
		}
	}
}
