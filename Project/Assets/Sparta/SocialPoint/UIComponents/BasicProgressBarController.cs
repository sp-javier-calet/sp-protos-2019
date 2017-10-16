using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.UIComponents
{
    public sealed class BasicProgressBarController : MonoBehaviour
    {
        public bool UseText = true;

        [SerializeField]
        Image _foreGround;

        [SerializeField]
        Text _message;

        public string Message
        {
            get
            {
                return _message != null && UseText ? _message.text : null;
            }

            set
            {
                if(_message != null && UseText)
                {
                    _message.text = value;
                }
            }
        }

        public float Percent
        {
            get
            {
                return _foreGround != null ? _foreGround.fillAmount : 0.0f;
            }

            set
            {
                if(_foreGround != null)
                {
                    _foreGround.fillAmount = value;
                }
            }
        }
    }
}