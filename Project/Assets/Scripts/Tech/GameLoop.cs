﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
