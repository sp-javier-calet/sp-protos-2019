using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Alert
{
    public sealed class UnityAlertViewButton : MonoBehaviour
    {
        public Text Label;
        public int Position;

        public event UnityAlertViewButtonClicked Clicked;

        public Button button;

        public string Text
        {
            set
            {
                Label.text = value;
            }
        }

        public void Start()
        {
            button.onClick.AddListener(OnClicked);
        }

        public void OnClicked()
        {
            if(Clicked != null)
            {
                Clicked(Position);
            }
        }

    }
}
