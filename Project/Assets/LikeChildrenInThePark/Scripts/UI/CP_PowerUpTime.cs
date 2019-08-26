
using SocialPoint.Rendering.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CP_PowerUpTime : MonoBehaviour
{
    public CanvasGroup Canvas = null;
    public BCSHModifier TimeBCSH = null;
    public TextMeshProUGUI Text = null;
    public Image TimeBar = null;

    Vector3 _vecTemp = Vector3.one;

    void Awake()
    {
        SetEnabled(false);
    }

    public void SetEnabled(bool enabled)
    {
        if(Canvas != null)
        {
            Canvas.alpha = enabled ? 1f : 0f;
        }

        if(TimeBar != null)
        {
            TimeBar.transform.localScale = Vector3.one;
        }
    }

    public void ShowPowerUpTime(CP_PlayerController.PowerUpType powerUpType)
    {
        if(TimeBCSH != null)
        {
            switch(powerUpType)
            {
                case CP_PlayerController.PowerUpType.E_ANGRY:
                {
                    TimeBCSH.ApplyBCSHState("angry");
                    Text.text = "A";
                    break;
                }
                case CP_PlayerController.PowerUpType.E_INVINCIBLE:
                {
                    TimeBCSH.ApplyBCSHState("invincible");
                    Text.text = "I";
                    break;
                }
                case CP_PlayerController.PowerUpType.E_SPEED_UP:
                {
                    TimeBCSH.ApplyBCSHState("speedup");
                    Text.text = "S";
                    break;
                }
                case CP_PlayerController.PowerUpType.E_DOUBLE_JUMP:
                {
                    TimeBCSH.ApplyBCSHState("doublejump");
                    Text.text = "D";
                    break;
                }
            }
        }

        SetEnabled(true);
    }

    public void SetDelta(float delta)
    {
        if(TimeBar != null)
        {
            _vecTemp.y = 1.0f - delta;

            TimeBar.transform.localScale = _vecTemp;
        }
    }
}
