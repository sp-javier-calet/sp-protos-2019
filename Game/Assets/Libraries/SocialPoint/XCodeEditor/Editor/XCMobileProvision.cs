
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace SocialPoint.XCodeEditor
{
    public class XCMobileProvision : XCPlist
    {
        private const string ELEMENT_APP_ID_PREFIX = "ApplicationIdentifierPrefix";
        private const string ELEMENT_UUID = "UUID";
        private const string ELEMENT_CREATION_DATE = "CreationDate";
        private const string ELEMENT_EXPIRATION_DATE = "ExpirationDate";
        private const string ELEMENT_DEV_CERTIFICATES = "DeveloperCertificates";
        private const string ELEMENT_ENTITLEMENTS = "Entitlements";
        private const string ELEMENT_NAME = "Name";
        private const string ELEMENT_ALL_DEVICES = "ProvisionsAllDevices";
        private const string ELEMENT_TEAM_ID = "TeamIdentifier";
        private const string ELEMENT_TEAM_NAME = "TeamName";
        private const string ELEMENT_TTL = "TimeToLive";
        private const string ELEMENT_VERSION = "Version";
        private const string XML_START = "<?xml";
        private const string XML_END = "</plist>";
        private string name;

        public string Name
        {
            get
            {
                if(name == null && ContainsKey(ELEMENT_NAME))
                {
                    name = (string)this[ELEMENT_NAME];
                }
                return name;
            }
        }

        private string teamName;

        public string TeamName
        {
            get
            {
                if(teamName == null && ContainsKey(ELEMENT_TEAM_NAME))
                {
                    teamName = (string)this[ELEMENT_TEAM_NAME];
                }
                return teamName;
            }
        }

        private string appIdName;

        public string AppIdName
        {
            get
            {
                if(appIdName == null && ContainsKey(ELEMENT_TEAM_NAME))
                {
                    appIdName = (string)this[ELEMENT_TEAM_NAME];
                }
                return appIdName;
            }
        }

        private string uuid;

        public string UUID
        {
            get
            {
                if(uuid == null && ContainsKey(ELEMENT_UUID))
                {
                    uuid = (string)this[ELEMENT_UUID];
                }
                return uuid;
            }
        }

        private int timeToLive = -1;

        public int TimeToLive
        {
            get
            {
                if(timeToLive == -1 && ContainsKey(ELEMENT_TTL))
                {
                    timeToLive = Int32.Parse((string)this[ELEMENT_TTL]);
                }
                return timeToLive;
            }
        }

        private int version;

        public int Version
        {
            get
            {
                if(version == -1 && ContainsKey(ELEMENT_VERSION))
                {
                    version = Int32.Parse((string)this[ELEMENT_VERSION]);
                }
                return version;
            }
        }

        private string appIdPrefix;

        public string AppIdPrefix
        {
            get
            {
                if(AppIdPrefixes != null)
                {
                    foreach(object p in AppIdPrefixes)
                    {
                        if(p is string)
                        {
                            appIdPrefix = (string)p;
                        }
                    }
                }
                return appIdPrefix;
            }
        }

        private XCPlistArray appIdPrefixes;

        public XCPlistArray AppIdPrefixes
        {
            get
            {
                if(appIdPrefixes == null && ContainsKey(ELEMENT_APP_ID_PREFIX))
                {
                    appIdPrefixes = (XCPlistArray)this[ELEMENT_APP_ID_PREFIX];
                }
                return appIdPrefixes;
            }
        }

        private bool creationDateSet = false;
        private DateTime creationDate;

        public DateTime CreationDate
        {
            get
            {
                if(!creationDateSet && ContainsKey(ELEMENT_CREATION_DATE))
                {
                    creationDateSet = true;
                    creationDate = DateTime.Parse((string)this[ELEMENT_CREATION_DATE]);
                }
                return creationDate;
            }
        }

        private bool expirationDateSet = false;
        private DateTime expirationDate;

        public DateTime ExpirationDate
        {
            get
            {
                if(!expirationDateSet && ContainsKey(ELEMENT_EXPIRATION_DATE))
                {
                    expirationDateSet = true;
                    expirationDate = DateTime.Parse((string)this[ELEMENT_EXPIRATION_DATE]);
                }
                return expirationDate;
            }
        }
        
        private ArrayList developerCertificates;

        public ArrayList DeveloperCertificates
        {
            get
            {
                if(developerCertificates == null && ContainsKey(ELEMENT_DEV_CERTIFICATES))
                {
                    developerCertificates = (ArrayList)this[ELEMENT_DEV_CERTIFICATES];
                }
                return developerCertificates;
            }
        }
        
        private Hashtable entitlements;

        public Hashtable Entitlements
        {
            get
            {
                if(entitlements == null && ContainsKey(ELEMENT_ENTITLEMENTS))
                {
                    entitlements = (Hashtable)this[ELEMENT_ENTITLEMENTS];
                }
                return entitlements;
            }
        }

        private XCPlistArray teamIdentifiers;

        public XCPlistArray TeamIdentifiers
        {
            get
            {
                if(teamIdentifiers == null && ContainsKey(ELEMENT_TEAM_ID))
                {
                    teamIdentifiers = (XCPlistArray)this[ELEMENT_TEAM_ID];
                }
                return teamIdentifiers;
            }
        }

        public bool ProvisionsAllDevices
        {
            get
            {
                return (Boolean)this[ELEMENT_ALL_DEVICES];
            }
        }
        
        override public bool LoadString(string contents)
        {
            int start = contents.IndexOf(XML_START);
            int end = contents.IndexOf(XML_END) + XML_END.Length;
            contents = contents.Substring(start, end - start);
            return base.LoadString(contents);
        }

        public XCMobileProvision(string filename)
        {
            LoadFile(filename);
        }
    }
}