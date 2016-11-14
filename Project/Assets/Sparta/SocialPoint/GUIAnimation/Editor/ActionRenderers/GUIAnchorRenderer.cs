using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public class GUIAnchorRenderer : GUIDefaultActionRenderer
    {
        readonly bool _showAnchors;

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
                for(int i = 0; i < stepsSelection.Steps.Count; ++i)
                {
                    Step step = stepsSelection.Steps[i];
                    var effectsGroup = step as EffectsGroup;
                    if(effectsGroup != null)
                    {
                        for(int animItems = 0; animItems < effectsGroup.AnimItems.Count; ++animItems)
                        {
                            var anchorsEffect = effectsGroup.AnimItems[animItems] as AnchorsEffect;
                            if(anchorsEffect != null)
                            {
                                anchorsEffect.SetCurrentPosition();
                            }
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
