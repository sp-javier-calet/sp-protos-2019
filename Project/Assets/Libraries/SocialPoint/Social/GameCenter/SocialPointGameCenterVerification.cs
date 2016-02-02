//  Author:
//    Miguel-Janer 
//
//  Copyright (c) 2016, Miguel-Janer
//
//  All rights reserved.
//
using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace SocialPoint.Social
{

    public class SocialPointGameCenterVerification : MonoBehaviour
    {

        public GameCenterValidationDelegate Callback;

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport ("__Internal")]
        private static extern void SPUnityGameCenterUserVerification_Init(string name);

        #else
        private static void SPUnityGameCenterUserVerification_Init(string name)
        {

        }
        #endif

        void Awake()
        {
            gameObject.name = GetType().ToString();
            DontDestroyOnLoad(this);
            //initialize plugin with gameobject name
            SPUnityGameCenterUserVerification_Init(gameObject.name);
            //requests gameCenterVerification
        }

        //called by the plugin when the verification is ready
        void Notify(string verfication)
        {
            Debug.Log(verfication);
            Callback(new GameCenterUserVerification());
        }

    }
}
