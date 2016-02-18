
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace SocialPoint.XCodeEditor
{
    public class XCEntitlements : XCPlist
    {
        private const string ELEMENT_KEYCHAIN_ACCESS_GROUPS = "keychain-access-groups";

        public bool AddKeychainAccessGroups(ArrayList groups)
        {
            bool modified = false;
            if(!ContainsKey(ELEMENT_KEYCHAIN_ACCESS_GROUPS))
            {
                modified = true;
                Add(ELEMENT_KEYCHAIN_ACCESS_GROUPS, new XCPlistArray());
            }
            foreach(string group in groups)
            {
                if(!((XCPlistArray)this[ELEMENT_KEYCHAIN_ACCESS_GROUPS]).Contains(group))
                {
                    XCDebug.Log("Adding keychain access group '" + group + "'");
                    ((XCPlistArray)this[ELEMENT_KEYCHAIN_ACCESS_GROUPS]).Add(group);
                    modified = true;
                }
            }
            return modified;
        }
    }
}