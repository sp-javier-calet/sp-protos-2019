using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MATSDK
{
    public class MATDelegate : MonoBehaviour
    {
        public Action<string> OnTrackerDidSucced;

        public void trackerDidSucceed (string data)
        {
            #if UNITY_IPHONE
            print ("MATDelegate trackerDidSucceed: " + DecodeFrom64 (data));
            #endif
            #if (UNITY_ANDROID || UNITY_WP8 || UNITY_METRO)
            print ("MATDelegate trackerDidSucceed: " + data);
            #endif
            var handler = OnTrackerDidSucced;
            if(handler != null)
            {
                OnTrackerDidSucced(data);
            }
        }

        public void trackerDidFail (string error)
        {
            print ("MATDelegate trackerDidFail: " + error);
        }
        
        public void trackerDidEnqueueRequest (string refId)
        {
            print ("MATDelegate trackerDidEnqueueRequest: " + refId);
        }

        public void trackerDidReceiveDeeplink (string url)
        {
            print ("MATDelegate trackerDidReceiveDeeplink: " + url);

            // TODO: add your custom code to handle the deferred deeplink url
        }

        public void trackerDidFailDeeplink (string error)
        {
            print ("MATDelegate trackerDidFailDeeplink: " + error);
        }

        public void onAdLoad(String placement)
        {
            print ("MATDelegate onAdLoad: placement = " + placement);
        }

        public void onAdLoadFailed(String error)
        {
            print ("MATDelegate onAdLoadFailed: " + error);
        }

        public void onAdClick(String empty)
        {
            print ("MATDelegate onAdClick");
        }
        
        public void onAdShown(String empty)
        {
            print ("MATDelegate onAdShown");
        }

        public void onAdActionStart(String willLeaveApplication)
        {
            print ("MATDelegate onAdActionStart: willLeaveApplication = " + willLeaveApplication);
        }
        
        public void onAdActionEnd(String empty)
        {
            print ("MATDelegate onAdActionEnd");
        }

        public void onAdRequestFired(String request)
        {
            print ("MATDelegate onAdRequestFired: request = " + request);
        }

        public void onAdClosed(String empty)
        {
            print ("MATDelegate onAdClosed");
        }

        /// <summary>
        /// The method to decode base64 strings.
        /// </summary>
        /// <param name="encodedData">A base64 encoded string.</param>
        /// <returns>A decoded string.</returns>
        public static string DecodeFrom64 (string encodedString)
        {
            string decodedString = null;

            #if !(UNITY_WP8) && !(UNITY_METRO)
            print ("MATDelegate.DecodeFrom64(string)");

            //this line causes the following error when building for Windows 8 phones:
            //Error building Player: Exception: Error: method `System.String System.Text.Encoding::GetString(System.Byte[])` doesn't exist in target framework. It is referenced from Assembly-CSharp.dll at System.String MATDelegateScript::DecodeFrom64(System.String).
            //Because of this, I'm currently choosing to disable it when Windows 8 phones are used. I'll see if I can find 
            //something better later. Until then, I'll probably use an else branch to take care of the UNITY_WP8 case.
            decodedString = System.Text.Encoding.UTF8.GetString (System.Convert.FromBase64String (encodedString));
            #endif

            return decodedString;
        }
    }
}
