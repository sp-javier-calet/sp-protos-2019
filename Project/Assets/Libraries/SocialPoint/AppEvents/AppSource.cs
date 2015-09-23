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
        public const string SchemeSeparator = "://";
        public const string QuerySeparator = "?";
        private const string SourceKeyLocalNotification = "sp_notification";
        private const string SourceKeyWidget = "widget";
        private const string SourceKeyFacebookLink = "applink_data";
        private static readonly List<string> CustomSchemes = new List<string>{ LocalNotificationScheme, PushNotificationScheme, WidgetScheme, FacebookScheme, OthersScheme };
        private static readonly Dictionary<string, string> SourceMapping = new Dictionary<string, string>
        {
            { SourceKeyLocalNotification, LocalNotificationScheme },
            { SourceKeyWidget, WidgetScheme },
            { SourceKeyFacebookLink, FacebookScheme }
        };
        private static readonly Dictionary<string, string[]> SourceFilters = new Dictionary<string, string[]>
        {
            { LocalNotificationScheme, new string[]{} },
            { WidgetScheme, new string[]{} },
            { FacebookScheme, new string[]{} },
            { OthersScheme, new string[] {"profile"} }
        };
        private Uri _uri;

        public string Uri
        { 
            get
            {
                return (_uri != null) ? _uri.ToString() : string.Empty;
            }
        }

        public string Scheme
        {
            get
            {
                return (_uri != null) ? _uri.Scheme : string.Empty;
            }
        }

        public string QueryString
        {
            get
            {
                return (_uri != null) ? _uri.Query : string.Empty;
            }
        }

        public Dictionary<string,string> Parameters
        {
            get
            {
                return StringUtils.QueryToDictionary(QueryString);
            }
        }

        public bool Empty
        {
            get
            {
                return Uri == null || Uri.Length == 0;
            }
        }

        private static bool IsCustomScheme(string scheme)
        {
            return CustomSchemes.Contains(scheme);
        }

        public bool IsCustomScheme()
        {
            return IsCustomScheme(Scheme);
        }

        public AppSource()
        {
        }

        public AppSource(string sourceString)
        {
            /* If sourceString is already a valid URL, 
             * use it as source. If not, try to parse parameters (in url format)
             * and infer the corresponding scheme */
            try
            {
                _uri = new Uri(sourceString);
            }
            catch(UriFormatException)
            {
                _uri = CreateUriFromSource(sourceString);
            }
        }

        public AppSource(IDictionary<string, string> sourceParameters)
        {
            _uri = CreateUriFromSource(sourceParameters);
        }

        private Uri CreateUriFromSource(string sourceString)
        {
            return CreateUriFromSource(StringUtils.QueryToDictionary(sourceString));
        }

        private Uri CreateUriFromSource(IDictionary<string, string> sourceParameters)
        {
            string scheme = OthersScheme;

            // Check source parameter for scheme mapping 
            foreach(KeyValuePair<string, string> pair in SourceMapping)
            {
                if(sourceParameters.ContainsKey(pair.Key))
                {
                    scheme = pair.Value;
                    break;
                }
            }

            // Apply filters
            string[] filters;
            if(SourceFilters.TryGetValue(scheme, out filters))
            {
                foreach(string filter in filters)
                {
                    sourceParameters.Remove(filter);
                }
            }

            /* Return null if there is no parameters after apply filters. 
             * Otherwise, add parameters and generate a valid URI
             */
            Uri uri = null;
            if(sourceParameters.Count > 0)
            {
                uri = new Uri(scheme + SchemeSeparator + QuerySeparator + StringUtils.DictionaryToQuery(sourceParameters));
            }

            return uri;
        }

        public override string ToString()
        {
            if(Uri == null)
            {
                return String.Empty;
            }
            else
            {
                return Uri.ToString();
            }
        }

    }
}