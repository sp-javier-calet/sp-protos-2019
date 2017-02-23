/*
 * Copyright 2015, Helpshift, Inc.
 * All rights reserved
 */
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Helpshift.Campaigns
{
    /// <summary>
    /// The main class which exposes the Helpshift Campaigns API for Unity scripts
    /// </summary>
    public class HelpshiftCampaigns {

#if UNITY_IOS || UNITY_ANDROID
        private static HelpshiftCampaigns instance = null;
#endif

#if UNITY_IOS
        private static HelpshiftCampaignsiOS nativeSdk = null;
#elif UNITY_ANDROID
        private static HelpshiftCampaignsAndroid nativeSdk = null;
#endif
        private HelpshiftCampaigns() {
        }


        /// <summary>
        /// Main function which should be used to get the HelpshiftCampaigns instance.
        /// </summary>
        /// <returns>Singleton HelpshiftCampaigns instance</returns>
        public static HelpshiftCampaigns getInstance () {
#if UNITY_IOS || UNITY_ANDROID
            if(instance == null) {
                instance = new HelpshiftCampaigns();
#if UNITY_IOS
                nativeSdk = new HelpshiftCampaignsiOS();
#elif UNITY_ANDROID
                nativeSdk = new HelpshiftCampaignsAndroid();
#endif
            }
            return instance;
#else
            return null;
#endif

        }

        public bool AddProperty(string key,int value) {
#if UNITY_IOS || UNITY_ANDROID
            return nativeSdk.AddProperty(key,value);
#else
            return false;
#endif
        }

        public bool AddProperty(string key,string value) {
#if UNITY_IOS || UNITY_ANDROID
            return nativeSdk.AddProperty(key,value);
#else
            return false;
#endif
        }

        public bool AddProperty(string key,bool value) {
#if UNITY_IOS || UNITY_ANDROID
            return nativeSdk.AddProperty(key,value);
#else
            return false;
#endif
        }

        public bool AddProperty(string key,System.DateTime value) {
#if UNITY_IOS || UNITY_ANDROID
            return nativeSdk.AddProperty(key,value);
#else
            return false;
#endif
        }

        public string[] AddProperties(Dictionary<string,object> value) {
#if UNITY_IOS || UNITY_ANDROID
            return nativeSdk.AddProperties(value);
#else
            return null;
#endif
        }

        public void ShowInbox(Dictionary<string, object> configMap) {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.ShowInbox(configMap);
#endif
        }

        public int GetCountOfUnreadMessages() {
#if UNITY_IOS || UNITY_ANDROID
            return nativeSdk.GetCountOfUnreadMessages();
#else
            return 0;
#endif
        }

#if UNITY_IOS
        public void RefetchMessages() {
            nativeSdk.RefetchMessages();
        }
#endif

    }
}
