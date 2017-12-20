
namespace SocialPoint.GUIAnimation
{
    public delegate void OnActionChanged(Effect action);

    // Base class to draw the effect atrributes in the editor
    public abstract class BaseGUIActionRenderer
    {
        public virtual bool CanRender(Effect action)
        {
            return false;
        }

        public virtual void Render(Effect action, StepsSelection stepsSelection, OnActionChanged onActionChanged)
        {
        }

        protected virtual void CopyEffectToSelection(Effect effect, StepsSelection stepsSelection)
        {
            for(int i = 0; i < stepsSelection.Count; ++i)
            {
                Step step = stepsSelection.Steps[i];
                if(step == effect)
                {
                    continue;
                }
                if(step.GetType() == effect.GetType())
                {
                    step.Copy(effect);
                }
            }
        }
    }
}