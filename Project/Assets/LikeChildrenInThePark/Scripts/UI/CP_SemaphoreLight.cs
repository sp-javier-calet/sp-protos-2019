using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class CP_SemaphoreLight : MonoBehaviour
{
    public enum LightState
    {
        E_DISABLED,
        E_OFF,
        E_RED,
        E_GREEN
    }

    public Image MaskImage;
    public GameObject OffGO;
    public GameObject RedGO;
    public GameObject GreenGO;

    public void SetLightState(LightState state)
    {
        if(MaskImage == null || OffGO == null || RedGO == null || GreenGO == null)
            return;

        MaskImage.enabled = false;
        OffGO.SetActive(false);
        RedGO.SetActive(false);
        GreenGO.SetActive(false);

        switch(state)
        {
            case LightState.E_OFF:
            {
                OffGO.SetActive(true);
                MaskImage.enabled = true;
                break;
            }
            case LightState.E_RED:
            {
                RedGO.SetActive(true);
                MaskImage.enabled = true;
                break;
            }
            case LightState.E_GREEN:
            {
                GreenGO.SetActive(true);
                MaskImage.enabled = true;
                break;
            }
        }
    }
}
