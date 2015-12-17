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

        public UpgradeData(UpgradeType type, IStreamReader reader)
        {
            Type = type;
            if (reader.Token != StreamToken.ObjectStart)
            {
                reader.SkipElement();
            }
            else
            {
                while(reader.Read() && reader.Token != StreamToken.ObjectEnd)
                {
                    var key = (string)reader.Value;
                    reader.Read();
                    switch(key)
                    {
                    case AttrKeyUpgradeMessage:
                        Message = (string)reader.Value;
                        break;
                    case AttrKeyUpgradeVersion:
                        Version = (string)reader.Value;
                        break;
                    }
                }
                reader.Read();
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

        public MaintenanceData(IStreamReader reader)
        {
            if (reader.Token != StreamToken.ObjectStart)
            {
                reader.SkipElement();
            }
            else
            {
                while(reader.Read() && reader.Token != StreamToken.ObjectEnd)
                {
                    var key = (string)reader.Value;
                    reader.Read();
                    switch(key)
                    {
                    case AttrKeyMaintenanceMessage:
                        Message = (string)reader.Value;
                        break;
                    case AttrKeyMaintenanceTitle:
                        Title = (string)reader.Value;
                        break;
                    }
                }
            }
        }

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
        public int UserImportance;
        public MaintenanceData Maintenance;

        private const string AttrKeyTimestamp = "ts";
        private const string AttrKeyStoreUrl = "store";
        private const string AttrKeyUpgradeSuggested = "suggested_upgrade";
        private const string AttrKeyUpgradeForced = "forced_upgrade";
        private const string AttrKeyMaintenanceData = "maintenance_data";
        private const string AttrKeyUserImportance = "user_importance";
        
        public void Load(IStreamReader reader)
        {
            if (reader.Token != StreamToken.ObjectStart)
            {
                reader.SkipElement();
            }
            else
            {
                while(reader.Read() && reader.Token != StreamToken.ObjectEnd)
                {
                    var key = (string)reader.Value;
                    reader.Read();
                    switch(key)
                    {
                    case AttrKeyTimestamp:
                        var serverTime = TimeUtils.GetDateTime(long.Parse(reader.Value.ToString()));
                        DeltaTime = serverTime - DateTime.UtcNow;
                        break;
                    case AttrKeyStoreUrl:
                        StoreUrl = (string)reader.Value;
                        break;
                    case AttrKeyUpgradeForced:
                        Upgrade = new UpgradeData(UpgradeType.Forced, reader);
                        break;
                    case AttrKeyUpgradeSuggested:
                        Upgrade = new UpgradeData(UpgradeType.Suggested, reader);
                        break;
                    case AttrKeyMaintenanceData:
                        Maintenance = new MaintenanceData(reader);
                        break;
                    case AttrKeyUserImportance:
                        UserImportance = reader.GetIntValue();
                        break;
                    }
                }
                if (Upgrade == null)
                    Upgrade = new UpgradeData(UpgradeType.None);
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

            if(datadic.ContainsKey(AttrKeyUserImportance))
            {
                UserImportance = datadic.Get(AttrKeyUserImportance).AsValue.ToInt();
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
