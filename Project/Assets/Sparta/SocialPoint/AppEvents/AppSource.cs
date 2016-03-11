using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.AppEvents
{
    public class AppSource
    {

        public const string LocalNotificationScheme = "local";
        public const string PushNotificationScheme = "push";
        public const string WidgetScheme = "widget";
        public const string FacebookScheme = "facebook";
        public const string OthersScheme = "others";
        public const char QuerySeparator = '?';

        private const string SourceTitleKey = "title";
        private const string SourceTextKey = "text";
        private const string SourceKeyOrigin = "sp_origin";
        private const string SourceKeyFacebookLink = "applink_data";
        private const string SourceValueLocalNotification = "local_notification";
        private const string SourceValuePushNotification = "push_notification";
        private const string SourceValueWidget = "widget";

        // Custom schemes to identify custom URLs
        private static readonly List<string> CustomSchemes = new List<string>
        {
            LocalNotificationScheme, 
            PushNotificationScheme, 
            WidgetScheme, 
            FacebookScheme, 
            OthersScheme 
        };

        // sp_origin->scheme mapping
        private static readonly Dictionary<string, string> OriginMapping = new Dictionary<string, string>
        {
            { SourceValueLocalNotification, LocalNotificationScheme },
            { SourceValuePushNotification, PushNotificationScheme },
            { SourceValueWidget, WidgetScheme }
        };

        // Parameter filter per scheme
        private static readonly Dictionary<string, string[]> SourceFilters = new Dictionary<string, string[]>
        {
            { LocalNotificationScheme, new string[]{ SourceKeyOrigin, SourceTitleKey, SourceTextKey, "alarmId", "android.intent.extra.ALARM_COUNT" } },
            { PushNotificationScheme, new string[]{ SourceKeyOrigin, SourceTitleKey, SourceTextKey } },
            { WidgetScheme, new string[]{SourceKeyOrigin} },
            { FacebookScheme, new string[]{} },
            { OthersScheme, new string[] {"profile"} }
        };

        private Uri _uri;

        public string Scheme
        {
            get
            {
                return (_uri != null) ? _uri.Scheme : string.Empty;
            }
        }

        public string Host
        {
            get
            {
                return (_uri != null) ? _uri.Host : string.Empty;
            }
        }

        public string Path
        {
            get
            {
                return (_uri != null) ? _uri.LocalPath : string.Empty;
            }
        }

        public string Query
        {
            get
            {
                return (_uri != null) ? _uri.Query.TrimStart(QuerySeparator) : string.Empty;
            }
        }

        public Dictionary<string,string> Parameters
        {
            get
            {
                return StringUtils.QueryToDictionary(Query);
            }
        }

        public bool Empty
        {
            get
            {
                return ToString().Length == 0;
            }
        }

        public bool IsCustomScheme
        {
            get
            {
                return CustomSchemes.Contains(Scheme);
            }
        }

        public AppSource()
        {
        }

        public AppSource(string sourceString)
        {
            /* If sourceString is already a valid URL, 
             * use it as source. If not, try to parse parameters (in url format)
             * and infer the corresponding scheme */
            if(!System.Uri.TryCreate(sourceString, UriKind.Absolute, out _uri))
            {
                _uri = CreateUriFromSource(sourceString);
            }
        }

        public AppSource(IDictionary<string, string> parms)
        {
            _uri = CreateUriFromSource(parms);
        }

        private Uri CreateUriFromSource(string src)
        {
            return CreateUriFromSource(StringUtils.QueryToDictionary(src));
        }

        private Uri CreateUriFromSource(IDictionary<string, string> parms)
        {
            string scheme = OthersScheme;

            // Check parameters for scheme mapping 
            string spOrigin;
            if(parms.TryGetValue(SourceKeyOrigin, out spOrigin))
            {
                // Map sp_origin parameter, if exists
                scheme = OriginMapping[spOrigin];
            }
            else if(parms.ContainsKey(SourceKeyFacebookLink))
            {
                scheme = FacebookScheme;
            }

            // Apply filters
            string[] filters;
            if(SourceFilters.TryGetValue(scheme, out filters))
            {
                foreach(string filter in filters)
                {
                    parms.Remove(filter);
                }
            }

            //we assume that empty params is an "icon" source
            if(scheme == OthersScheme && parms.Count == 0)
            {
                return null;
            }

            var build = new UriBuilder();
            build.Scheme = scheme;
            if(parms.Count > 0)
            {
                build.Query = QuerySeparator.ToString()+StringUtils.DictionaryToQuery(parms);
            }
            return build.Uri;
        }

        public override string ToString()
        {
            return (_uri != null) ? _uri.ToString() : string.Empty;
        }

        public bool IsOpenFromIcon
        {
            get
            {
                return _uri == null;
            }
        }
    }
}
