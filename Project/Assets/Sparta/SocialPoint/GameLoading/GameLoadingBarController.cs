using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.GameLoading
{
    public sealed class GameLoadingBarController : MonoBehaviour
    {
        [SerializeField]
        Slider _slider;

        [SerializeField]
        Text _log;

        public string Message
        {
            get
            {
                return _log != null ? _log.text : null;
            }

            set
            {
                if(_log != null)
                {
                    _log.text = value;
                }
            }
        }

        public float Percent
        {
            get
            {
                return _slider != null ? _slider.normalizedValue : 0.0f;
            }

            set
            {
                if(_slider != null)
                {
                    _slider.normalizedValue = value;
                }
            }
        }
    }
}