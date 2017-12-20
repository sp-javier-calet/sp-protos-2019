//  ---------------------------------------------------------------------------
//  FyberAdColonyFix.cs
//
//  The purpose of this class is to ensure proper session handling 
//  for AdColony SDK when using Fyber mediation.
//
//  Copyright © 2015 AdColony, Inc.  All rights reserved.
//
//  ---------------------------------------------------------------------------
using UnityEngine;
using System;

public class FyberAdColonyFix
{
#if UNITY_ANDROID && !UNITY_EDITOR
    static bool adr_initialized = false;
    static AndroidJavaClass class_UnityPlayer;
    static IntPtr class_UnityADC = IntPtr.Zero;

    public static IntPtr method_pause = IntPtr.Zero;
    public static IntPtr method_resume = IntPtr.Zero;

    public static void AndroidInitializePlugin()
    {
        if (adr_initialized) return;
        Debug.Log("Initializing Fyber AdColony fix-plugin");
        bool success = true;
        IntPtr local_class_UnityADC = AndroidJNI.FindClass("com/jirbo/unityadc/UnityADC");
        if (local_class_UnityADC != IntPtr.Zero)
        {
            class_UnityADC = AndroidJNI.NewGlobalRef(local_class_UnityADC);
            AndroidJNI.DeleteLocalRef(local_class_UnityADC);
        }
        else
        {
            success = false;
        }

        if (success)
        {

            class_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            // Get additional method IDs for later use.
            method_pause = AndroidJNI.GetStaticMethodID(class_UnityADC, "pause", "(Landroid/app/Activity;)V");
            method_resume = AndroidJNI.GetStaticMethodID(class_UnityADC, "resume", "(Landroid/app/Activity;)V");
            adr_initialized = true;
            Debug.Log("Initialization Fyber AdColony fix-plugin succeeded");
        }
        else
        {
            // adcolony.jar and unityadc.jar most both be in Assets/Plugins/Android/ !
            Debug.LogError("AdColony configuration error - make sure adcolony.jar and "
                + "unityadc.jar libraries are in your Unity project's Assets/Plugins/Android folder.");
        }
    }

    public static void AndroidResume()
    {
        var j_activity = class_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        jvalue[] args = new jvalue[1];
        args[0].l = j_activity.GetRawObject();

        AndroidJNI.CallStaticVoidMethod(class_UnityADC, method_resume, args);
    }

    public static void AndroidPause()
    {
        var j_activity = class_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        jvalue[] args = new jvalue[1];
        args[0].l = j_activity.GetRawObject();

        AndroidJNI.CallStaticVoidMethod(class_UnityADC, method_pause, args);
    }

#endif

}
