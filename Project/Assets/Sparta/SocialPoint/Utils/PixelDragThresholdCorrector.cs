﻿using UnityEngine;
using UnityEngine.EventSystems;

public class PixelDragThresholdCorrector : MonoBehaviour
{
    const int _baseTH = 6;
    const int _basePPI = 210;

    void Start()
    {
        EventSystem es = GetComponent<EventSystem>();

        if(es != null)
        {
            int dragTH = _baseTH * (int)Screen.dpi / _basePPI;
            es.pixelDragThreshold = dragTH;
        }
    }
}
