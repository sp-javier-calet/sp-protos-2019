using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.IosKeychain
{
    public class KeychainItemException : Exception
    {
        public KeychainItemException(int status):
        base("Keychain returned error "+status)
        {
        }
    }

    public class KeychainItem {

        public string Id;
        public string AccessGroup;
        public string Service;

        public static readonly string kSeparator = ".";

        public KeychainItem(string id, string accessGroup=null, string service=null)
        {
            var seedId = SeedId;
            if(accessGroup == null)
            {
                accessGroup = DefaultAccessGroup;
            }
            else if(seedId != null)
            {
                accessGroup = seedId+kSeparator+accessGroup;
            }
            if(service == null)
            {
                service = accessGroup;
            }
            else if(seedId != null)
            {
                service = seedId+kSeparator+service;
            }
            Id = id;
            AccessGroup = accessGroup;
            Service = service;
        }

        private KeychainBridge.ItemStruct BridgeItem
        {
            get
            {
                return new KeychainBridge.ItemStruct{
                    Id = Id, Service = Service,
                    AccessGroup = AccessGroup };
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

        static public string DefaultAccessGroup
        {
            get
            {
                return KeychainBridge.SPUnityKeychainGetDefaultAccessGroup();
            }
        }

        static public string SeedId
        {
            get
            {
                var parts = DefaultAccessGroup.Split(new string[]{kSeparator}, StringSplitOptions.None);
                if(parts.Length > 0)
                {
                    return parts[0];
                }
                else
                {
                    return null;
                }
            }
        }

    }

}
