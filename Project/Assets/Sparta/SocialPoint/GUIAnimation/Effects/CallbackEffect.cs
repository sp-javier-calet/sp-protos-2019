using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class CallbackEffect : TriggerEffect
    {
        [SerializeField]
        [ShowInEditor]
        string _method = "OnEvent";

        public string Method { get { return _method; } set { _method = value; } }

        [SerializeField]
        [ShowInEditor]
        string _parameters = "OnEvent";

        public string Parameters { get { return _parameters; } set { _parameters = value; } }


        public override void Copy(Step other)
        {
            base.Copy(other);
            CopyActionValues((GameObjectEnablerEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            _method = ((CallbackEffect)other).Method;
            _parameters = ((CallbackEffect)other).Parameters;
        }

        public override void OnRemoved()
        {
        }

        public override void SetOrCreateDefaultValues()
        {
        }

        public override void Invert(bool invertTime)
        {
            base.Invert(invertTime);
        }

        public override void DoAction()
        {
            if(Target == null)
            {
                Log.w(GetType() + " OnBlend " + StepName + " Target is null");
                return;
            }

            if(string.IsNullOrEmpty(_parameters))
            {
                Target.SendMessage(_method, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Target.SendMessage(_method, _parameters, SendMessageOptions.DontRequireReceiver);
            }
        }

        public override void OnReset()
        {
            base.OnReset();
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            Log.w(GetType() + " -> SaveValues. Nothing to save :(");
        }
    }
}
