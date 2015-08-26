using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.GameLoading
{
    public class LoadingBarController : MonoBehaviour
    {
        public Slider Slider;
        public Text log;

        public bool displayFunnyLogs;
        public string[] funnyLogs;

        public void UpdateProgress(float percent, string message)
        {
            if(message != string.Empty)
            {
                if(log != null)
                    log.text = message;
            }
            if(displayFunnyLogs && log != null)
            {
                log.text = funnyLogs[Mathf.Min((int)(funnyLogs.Length * percent), funnyLogs.Length - 1)];   
            }
            Slider.value = percent;
        }
    }
}