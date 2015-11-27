using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Utils;

namespace SocialPoint.Login
{
    public class UpgradeData
    {
        public UpgradeType Type;
        public string Message;
        public string Version;
        private const string AttrKeyUpgradeMessage = "message";
        private const string AttrKeyUpgradeVersion = "version";
        
        public UpgradeData(UpgradeType type, Attr data = null)
        {
            Type = type;
            
            if(data != null && data.AttrType == AttrType.DICTIONARY)
            {
                var datadic = data.AsDic;
                Message = datadic.Get(AttrKeyUpgradeMessage).AsValue.ToString();
                Version = datadic.GetValue(AttrKeyUpgradeVersion).AsValue.ToString();
                
            }
        }
        
        public override string ToString()
        {
            return string.Format("[UpgradeData: Type={0}, Message={1}, Version={2}]",
                                 Type, Message, Version);
        }
    }
    
    public class MaintenanceData
    {
        public string Title;
        public string Message;
        public string Button;
        private const string AttrKeyMaintenanceMessage = "message";
        private const string AttrKeyMaintenanceTitle = "title";
        private const string AttrKeyMaintenanceButton = "button";
        
        public MaintenanceData(Attr data = null)
        {
            if(data != null && data.AttrType == AttrType.DICTIONARY)
            {
                var datadic = data.AsDic;
                Message = datadic.Get(AttrKeyMaintenanceMessage).AsValue.ToString();
                Title = datadic.GetValue(AttrKeyMaintenanceTitle).AsValue.ToString();
                Button = datadic.GetValue(AttrKeyMaintenanceButton).AsValue.ToString();
            }
        }
        
        public override string ToString()
        {
            return string.Format("[MaintentanceData: Title={0}, Message={1} Button={2}]",
                Title, Message, Button);
        }
    }
    
    public class GenericData
    {
        public TimeSpan DeltaTime;
        public string StoreUrl;
        public UpgradeData Upgrade;
        public MaintenanceData Maintenance;
        public int UserImportance;

        private const string AttrKeyTimestamp = "ts";
        private const string AttrKeyStoreUrl = "store";
        private const string AttrKeyUpgradeSuggested = "suggested_upgrade";
        private const string AttrKeyUpgradeForced = "forced_upgrade";
        private const string AttrKeyMaintenanceData = "maintenance_data";
        private const string AttrKeyUserImportance = "user_importance";
        
        public GenericData(Attr data = null)
        {
            if(data != null)
            {
                Load(data);
            }
        }

        public void Load(Attr data)
        {
            var datadic = data.AsDic;
            if(datadic.ContainsKey(AttrKeyTimestamp))
            {
                var serverTime = TimeUtils.GetDateTime(datadic.Get(AttrKeyTimestamp).AsValue.ToLong());
                DeltaTime = serverTime - DateTime.UtcNow;
            }
            if(datadic.ContainsKey(AttrKeyStoreUrl))
            {
                StoreUrl = datadic.GetValue(AttrKeyStoreUrl).ToString();
            }
            if(datadic.ContainsKey(AttrKeyUserImportance))
            {
                UserImportance = datadic.GetValue(AttrKeyUserImportance).ToInt();
            } 
            if(datadic.ContainsKey(AttrKeyUpgradeForced))
            {
                Upgrade = new UpgradeData(UpgradeType.Forced, datadic.Get(AttrKeyUpgradeForced));
            }
            else if(datadic.ContainsKey(AttrKeyUpgradeSuggested))
            {
                Upgrade = new UpgradeData(UpgradeType.Suggested, datadic.Get(AttrKeyUpgradeSuggested));
            }
            else
            {
                Upgrade = new UpgradeData(UpgradeType.None);
            }
            if(datadic.ContainsKey(AttrKeyMaintenanceData))
            {
                Maintenance = new MaintenanceData(datadic.Get(AttrKeyMaintenanceData));
            }
        }

        public override string ToString()
        {
            return string.Format(
                "[GenericData: DeltaTime={0}, StoreUrl={1}, UserImportance={2} Upgrade={3}, Maintenance={4}]",
                DeltaTime, StoreUrl, UserImportance, Upgrade, Maintenance);
        }
        
        [Obsolete("Use Upgrade.Type")]
        public bool ForcedUpgradeRequired
        {
            get
            {
                return Upgrade.Type == UpgradeType.Forced;
            }
        }
    }
}
