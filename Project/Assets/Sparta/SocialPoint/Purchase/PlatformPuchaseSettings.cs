using UnityEngine;
using System.Collections;
using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    public class PlatformPuchaseSettings
    {
        //IOS
        public const string IOSUseApplicationUsernameKey = "IOSUseApplicationUsernameKey";
        public const string IOSUseAppReceiptKey = "IOSUseAppReceiptKey";
        public const string IOSUseDetailedLogKey = "IOSUseDetailedLogKey";
        public const string IOSSendTransactionUpdateEventsKey = "IOSSendTransactionUpdateEventsKey";

        //ANDROID
        public const string AndroidUseDetailedLogKey = "AndroidUseDetailedLogKey";


        public delegate void SetBoolSettingDelegate(bool settingValue);

        //Use as a guide when building custom settings.
        //Setting all params is not mandatory, and those not included in a settings AttrDic should keep its current value.
        public static AttrDic GetDebugSettings()
        {
            AttrDic settings = new AttrDic();

            #if UNITY_IOS && !UNITY_EDITOR
            settings.SetValue(PlatformPuchaseSettings.IOSUseApplicationUsernameKey, false);
            settings.SetValue(PlatformPuchaseSettings.IOSUseAppReceiptKey, false);
            settings.SetValue(PlatformPuchaseSettings.IOSUseDetailedLogKey, true);
            settings.SetValue(PlatformPuchaseSettings.IOSSendTransactionUpdateEventsKey, true);
            #elif UNITY_ANDROID && !UNITY_EDITOR
            //Add Android-only keys if needed
            settings.SetValue(PlatformPuchaseSettings.AndroidUseDetailedLogKey, true);
            #elif UNITY_EDITOR
            //Add Editor-only keys if needed
            #endif

            return settings;
        }

        //Helper function to avoid duplicated code and handle safe AttrDic access when stores are implementing their Setup functions
        public static void SetBoolSetting(AttrDic settings, string key, SetBoolSettingDelegate Setter)
        {
            if(settings.ContainsKey(key))
            {
                Setter(settings.GetValue(key).ToBool());
            }
        }
    }
}
