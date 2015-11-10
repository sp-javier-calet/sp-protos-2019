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
            { LocalNotificationScheme, new string[]{ SourceKeyOrigin, SourceTitleKey, SourceTextKey, "android.intent.extra.ALARM_COUNT" } },
            { PushNotificationScheme, new string[]{ SourceKeyOrigin, SourceTitleKey, SourceTextKey } },
            { WidgetScheme, new string[]{SourceKeyOrigin} },
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

            // Check parameters for scheme mapping 
            string spOrigin;
            if(sourceParameters.TryGetValue(SourceKeyOrigin, out spOrigin))
            {
                // Map sp_origin parameter, if exists
                scheme = OriginMapping[spOrigin];
            }
            else if(sourceParameters.ContainsKey(SourceKeyFacebookLink))
            {
                scheme = FacebookScheme;
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

            // If there is no parameters for an Others sources, returns a null uri.
            Uri uri = null;
            if(scheme != OthersScheme || sourceParameters.Count > 0)
            {
                string parametersString = string.Empty;
                if(sourceParameters.Count > 0)
                {
                    parametersString = QuerySeparator + StringUtils.DictionaryToQuery(sourceParameters);
                }

                uri = new Uri(scheme + SchemeSeparator + parametersString);
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