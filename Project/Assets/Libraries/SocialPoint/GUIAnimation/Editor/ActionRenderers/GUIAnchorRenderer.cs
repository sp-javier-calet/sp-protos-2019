using UnityEngine;
using UnityEditor;

namespace SocialPoint.GUIAnimation
{
	public class GUIAnchorRenderer : GUIDefaultActionRenderer
	{
		bool _showAnchors = false;

		public GUIAnchorRenderer()
		{

		}

		public GUIAnchorRenderer(bool showAnchors)
		{
			_showAnchors = showAnchors;
		}

		public override bool CanRender(Effect action)
		{
			return action is AnchorsEffect;
		}
		
		public override void Render(Effect effect, StepsSelection stepsSelection, OnActionChanged onChanged)
		{
			if(_showAnchors)
			{
				base.Render(effect, stepsSelection, onChanged);
			}

			GUILayout.Space(15f);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Set Current Position", GUILayout.MaxWidth(160f)))
			{
				for (int i = 0; i < stepsSelection.Steps.Count; ++i) 
				{
					Step step = stepsSelection.Steps[i];
					if(step is EffectsGroup)
					{
						EffectsGroup effectsGroup = (EffectsGroup) step;
						for (int animItems = 0; animItems < effectsGroup.AnimItems.Count; ++animItems) 
						{
							if(effectsGroup.AnimItems[animItems] is AnchorsEffect)
							{
								((AnchorsEffect) effectsGroup.AnimItems[animItems]).SetCurrentPosition();
							}
						}
					}
				}
			}
			GUILayout.EndHorizontal();
		}
	}
}
