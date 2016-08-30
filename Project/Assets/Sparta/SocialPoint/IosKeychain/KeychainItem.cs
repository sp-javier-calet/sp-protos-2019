using System;

namespace SocialPoint.IosKeychain
{
    public sealed class KeychainItemException : Exception
    {
        public KeychainItemException(int status) :
            base("Keychain returned error " + status)
        {
        }
    }

    public sealed class KeychainItem
    {

        public string Id;
        public string AccessGroup;
        public string Service;

        public static readonly string kSeparator = ".";

        public KeychainItem(string id, string accessGroup = null, string service = null)
        {
            var seedId = SeedId;
            if(accessGroup == null)
            {
                accessGroup = DefaultAccessGroup;
            }
            else if(seedId != null)
            {
                accessGroup = seedId + kSeparator + accessGroup;
            }
            if(service == null)
            {
                service = accessGroup;
            }
            else if(seedId != null)
            {
                service = seedId + kSeparator + service;
            }
            Id = id;
            AccessGroup = accessGroup;
            Service = service;
        }

        KeychainBridge.ItemStruct BridgeItem
        {
            get
            {
                return new KeychainBridge.ItemStruct {
                    Id = Id ?? string.Empty, Service = Service ?? string.Empty,
                    AccessGroup = AccessGroup ?? string.Empty
                };
            }
        }

        public string Value
        {
            get
            {
                return KeychainBridge.SPUnityKeychainGet(BridgeItem);
            }
            
            set
            {
                var status = KeychainBridge.SPUnityKeychainSet(BridgeItem, value);
                if(status != 0)
                {
                    throw new KeychainItemException(status);
                }
            }
        }

        public void Clear()
        {
            var status = KeychainBridge.SPUnityKeychainClear(BridgeItem);
            if(status != 0)
            {
                throw new KeychainItemException(status);
            }
        }

        static string _defaultAccessGroup;

        static public string DefaultAccessGroup
        {
            get
            {
                if(_defaultAccessGroup == null)
                {
                    _defaultAccessGroup = KeychainBridge.SPUnityKeychainGetDefaultAccessGroup();
                }
                return _defaultAccessGroup;
            }
        }

        static string _seedId;

        static public string SeedId
        {
            get
            {
                if(_seedId == null)
                {
                    var parts = DefaultAccessGroup.Split(new []{ kSeparator }, StringSplitOptions.None);
                    _seedId = parts.Length > 0 ? parts[0] : null;
                }
                return _seedId;
            }
        }

    }

}
