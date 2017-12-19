    /*
     * Copyright 2015, Helpshift, Inc.
     * All rights reserved
     */

    #if UNITY_ANDROID
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using HSMiniJSON;
    using System.Linq;
	using System.Collections;
	using System.Threading;

    namespace Helpshift
    {
        public class HelpshiftAndroid : IWorkerMethodDispacther, IDexLoaderListener{

            private AndroidJavaClass jc;
            private AndroidJavaObject currentActivity, application;
            private AndroidJavaObject hsHelpshiftClass;
            private AndroidJavaObject hsSupportClass;
            private AndroidJavaClass hsUnityAPIDelegate;

            void unityHSApiCall(string api, params object[] args) {
                addHSApiCallToQueue ("unityHSApiCallWithArgs", api, args);
            }

            void hsApiCall(string api, params object[] args) {
                addHSApiCallToQueue ("hsApiCallWithArgs", api, args);
            }

            void hsApiCall(string api) {
                addHSApiCallToQueue ("hsApiCall", api, null);
            }

            void hsSupportApiCall(string api, params object[] args) {
                addHSApiCallToQueue ("hsSupportApiCallWithArgs", api, args);
            }

            void hsSupportApiCall(string api) {
                addHSApiCallToQueue ("hsSupportApiCall", api, null);
            }

            void addHSApiCallToQueue(String methodIdentifier, String api, object[] args) {
                HelpshiftWorker.getInstance ().enqueueApiCall ("support", methodIdentifier, api, args);
            }

            public HelpshiftAndroid () {
                this.jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                this.currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                this.application = currentActivity.Call<AndroidJavaObject>("getApplication");
                this.hsUnityAPIDelegate = new AndroidJavaClass("com.helpshift.supportCampaigns.UnityAPIDelegate");
                HelpshiftWorker.getInstance ().registerClient ("support", this);
                HelpshiftDexLoader.getInstance().loadDex(this, application);
            }

            public void resolveAndCallApi(string methodIdentifier, string api, object[] args)
            {
                if (methodIdentifier.Equals ("hsApiCallWithArgs")) {
                    hsHelpshiftClass.CallStatic (api, args);
                }
                else if (methodIdentifier.Equals ("hsApiCall")) {
                    hsHelpshiftClass.CallStatic (api);
                }
                else if (methodIdentifier.Equals ("hsSupportApiCallWithArgs")) {
                    hsSupportClass.CallStatic (api, args);
                }
                else if (methodIdentifier.Equals ("hsSupportApiCall")) {
                    hsSupportClass.CallStatic (api);
                } else if (methodIdentifier.Equals ("unityHSApiCallWithArgs")) {
                    hsUnityAPIDelegate.CallStatic(api, args);
                }
            }

            public void onDexLoaded() {
			hsHelpshiftClass = HelpshiftDexLoader.getInstance().getHSDexLoaderJavaClass().CallStatic<AndroidJavaObject> ("getHelpshiftInstance");
			hsSupportClass = HelpshiftDexLoader.getInstance().getHSDexLoaderJavaClass().CallStatic<AndroidJavaObject> ("getHelpshiftSupportInstance");
            }

            public void install (string apiKey, string domain, string appId, Dictionary<string, object> configMap) {
                configMap.Add ("sdkType", "unity");
                configMap.Add ("pluginVersion", "2.6.1");
 		 configMap.Add ("runtimeVersion", Application.unityVersion);
		 hsApiCall("install", new object[] {this.application, apiKey, domain, appId, Json.Serialize(configMap)});
            }

            public void install () {
                hsApiCall("install", new object[] {this.application});
            }

            public int getNotificationCount (Boolean isAsync) {
                // Wait for queue since we need synchronous call here.
                HelpshiftWorker.getInstance ().synchronousWaitForApiCallQueue ();
                return this.hsHelpshiftClass.CallStatic<int> ("getNotificationCount", isAsync);
            }

            public void setNameAndEmail (string userName, string email) {
                hsApiCall("setNameAndEmail", new object[] {userName, email});
            }

            public void setUserIdentifier (string identifier) {
                hsApiCall("setUserIdentifier", identifier);
            }

            public void registerDeviceToken (string deviceToken) {
                hsApiCall("registerDeviceToken", new object [] {this.currentActivity, deviceToken});
            }

            public void leaveBreadCrumb (string breadCrumb) {
                hsApiCall("leaveBreadCrumb", breadCrumb);
            }

            public void clearBreadCrumbs () {
                hsApiCall("clearBreadCrumbs");
            }

            public void login (string identifier, string userName, string email) {
                hsApiCall("login", new object[] {identifier, userName, email});
            }

            public void logout() {
                hsApiCall("logout");
            }

            public void showConversation (Dictionary<string, object> configMap) {
                hsApiCall("showConversationUnity", new object [] {this.currentActivity, Json.Serialize(configMap)});
            }

            public void showFAQSection (string sectionPublishId, Dictionary<string, object> configMap) {
                hsApiCall("showFAQSectionUnity", new object[] {this.currentActivity, sectionPublishId, Json.Serialize(configMap)});
            }

            public void showSingleFAQ (string questionPublishId, Dictionary<string, object> configMap) {
                hsApiCall("showSingleFAQUnity", new object[] {this.currentActivity, questionPublishId, Json.Serialize(configMap)});
            }

            public void showFAQs (Dictionary<string, object> configMap) {
                hsApiCall("showFAQsUnity", new object [] { this.currentActivity, Json.Serialize(configMap)});
            }

            public void showConversation () {
                hsApiCall("showConversationUnity", new object[] {this.currentActivity, null});
            }

            public void showFAQSection (string sectionPublishId) {
                hsApiCall("showFAQSectionUnity", new object[] {this.currentActivity, sectionPublishId, null});
            }

            public void showSingleFAQ (string questionPublishId) {
                hsApiCall("showSingleFAQUnity", new object[] {this.currentActivity, questionPublishId, null});
            }

            public void showFAQs () {
                hsApiCall("showFAQsUnity", new object[] {this.currentActivity, null});
            }

            public void showConversationWithMeta (Dictionary<string, object> configMap) {
                hsApiCall("showConversationWithMetaUnity", new object[]{this.currentActivity, Json.Serialize(configMap)});
            }

            public void showFAQSectionWithMeta (string sectionPublishId, Dictionary<string, object> configMap) {
                hsApiCall("showFAQSectionWithMetaUnity", new object[] {this.currentActivity, sectionPublishId, Json.Serialize(configMap)});
            }

            public void showSingleFAQWithMeta (string questionPublishId, Dictionary<string, object> configMap) {
                hsApiCall("showSingleFAQWithMetaUnity", new object[] {this.currentActivity, questionPublishId, Json.Serialize(configMap)});
            }

            public void showFAQsWithMeta (Dictionary<string, object> configMap) {
                hsApiCall("showFAQsWithMetaUnity", new object[]{this.currentActivity, Json.Serialize(configMap)});
            }

            public void updateMetaData(Dictionary<string, object> metaData) {
                hsApiCall("setMetaData", Json.Serialize(metaData));
            }

            public void handlePushNotification(string issueId) {
                // Handle issueId via the new api for handling push using dictionary.
                Dictionary<string, object> pushNotificationData = new Dictionary<string, object>();
                pushNotificationData.Add("issue_id", issueId);
                handlePushNotification(pushNotificationData);
            }

            public void handlePushNotification(Dictionary<string, object> pushNotificationData) {
                unityHSApiCall("handlePush", new object[] {this.currentActivity, Json.Serialize(pushNotificationData)});
            }

            public void showAlertToRateAppWithURL (string url) {
                hsApiCall("showAlertToRateApp", url);
            }

            public void registerDelegates() {
                hsApiCall("registerDelegates");
            }

            public void registerForPushWithGcmId(string gcmId) {
                hsApiCall("registerGcmKey", new object[] {gcmId, this.currentActivity});
            }

            public void setSDKLanguage(string locale) {
                hsApiCall("setSDKLanguage", new object[] {locale});
            }

            public void showDynamicForm(string title, Dictionary<string, object>[] flows) {
                hsSupportApiCall("showDynamicFormFromDataJson", new object[] {this.currentActivity, Json.Serialize(flows)});
            }

            public void onApplicationQuit() {
                HelpshiftWorker.getInstance ().onApplicationQuit ();
            }
        }

        public class HelpshiftAndroidLog : IDexLoaderListener, IWorkerMethodDispacther {
            private static AndroidJavaObject logger = null;
            private static HelpshiftAndroidLog helpshiftAndroidLog = new HelpshiftAndroidLog();

            private HelpshiftAndroidLog () {
            }

            public void resolveAndCallApi(string methodIdentifier, string api, object[] args) {

            }

            public void onDexLoaded() {
                HelpshiftAndroidLog.logger = HelpshiftDexLoader.getInstance().getHSDexLoaderJavaClass().CallStatic<AndroidJavaObject> ("getHelpshiftLogInstance");
            }

            private static void initLogger () {
                if(HelpshiftAndroidLog.logger == null) {
                    HelpshiftWorker.getInstance ().registerClient ("helpshiftandroidlog", helpshiftAndroidLog);
                    HelpshiftDexLoader.getInstance().registerListener(helpshiftAndroidLog);
                }
            }

            public static int v (String tag, String log) {
                initLogger();
                HelpshiftWorker.getInstance ().synchronousWaitForApiCallQueue ();
                return HelpshiftAndroidLog.logger.CallStatic<int> ("v", new object[] {tag, log});
            }

            public static int d (String tag, String log) {
                initLogger();
                HelpshiftWorker.getInstance ().synchronousWaitForApiCallQueue ();
                return HelpshiftAndroidLog.logger.CallStatic<int> ("d", new object[] {tag, log});
            }

            public static int i (String tag, String log) {
                initLogger();
                HelpshiftWorker.getInstance ().synchronousWaitForApiCallQueue ();
                return HelpshiftAndroidLog.logger.CallStatic<int> ("i", new object[] {tag, log});
            }

            public static int w (String tag, String log) {
                initLogger();
                HelpshiftWorker.getInstance ().synchronousWaitForApiCallQueue ();
                return HelpshiftAndroidLog.logger.CallStatic<int> ("w", new object[] {tag, log});
            }

            public static int e (String tag, String log) {
                initLogger();
                HelpshiftWorker.getInstance ().synchronousWaitForApiCallQueue ();
                return HelpshiftAndroidLog.logger.CallStatic<int> ("e", new object[] {tag, log});
            }
        }
    }
    #endif
