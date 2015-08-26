using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.Attributes;
using SocialPoint.Utils;
using SocialPoint.IO;

namespace SocialPoint.Locale
{
    public class LocalizationManager
    {
        public class LocationData
        {
            public const string DefaultProjectId = "dc";
            
            // Android uses iOS json too
            public const string DefaultPlatform = "ios";
            
            //public const string DefaultEnvironmentId = "dev";
            //public const string DefaultSecretKey = "5TgemMFH4yj7RJ3d";
            public const string DefaultEnvironmentId = "prod";
            public const string DefaultSecretKey = "wetd46pWuR8J5CmS";
                        
            private const string UrlFormat = "http://sp-translations.socialpointgames.com/deploy/<PROJ>/<PLAT>/<ENV>/<PROJ>_<PLAT>_<LANG>_<ENV>_<KEY>.json";
            private const string ProjectIdPlaceholder = "<PROJ>";
            private const string PlatformPlaceholder = "<PLAT>";
            private const string EnvionmentIdPlaceholder = "<ENV>";
            private const string SecretKeyPlaceholder = "<KEY>";
            private const string LanguagePlaceholder = "<LANG>";
            
            public string ProjectId = DefaultProjectId;
            public string EnvironmentId = DefaultEnvironmentId;
            public string SecretKey = DefaultSecretKey;
            public string Platform = DefaultPlatform;
            
            public string Format(string pattern, string lang)
            {
                pattern = pattern.Replace(ProjectIdPlaceholder, ProjectId);
                pattern = pattern.Replace(PlatformPlaceholder, Platform);
                pattern = pattern.Replace(EnvionmentIdPlaceholder, EnvironmentId);
                pattern = pattern.Replace(SecretKeyPlaceholder, SecretKey);
                pattern = pattern.Replace(LanguagePlaceholder, lang);
                return pattern;
            }
            
            public string GetUrl(string lang)
            {
                return Format(UrlFormat, lang);
            }
        }

        private const string JsonExtension = ".json";
        private const string kEtagHeader = "Etag";
        private const string kIfNoneMatchHeader = "If-None-Match";
        private const string FilePrefixFormat = "<PROJ>_localization_<PLAT>_<ENV>_<LANG>_";
        private const string CsvSeparator = ",";

        public const string SimplifiedChineseServerIdentifier = "zh";
        public const string TraditionalChineseServerIdentifier = "tw";
        public const string SimplifiedChineseIdentifier = "zh-Hans";
        public const string TraditionalChineseIdentifier = "zh-Hant";

        private string _localizationUrl;
        private string _cachePath;
        private string _bundlePath;
        private IHttpClient _httpClient;
        private IAppInfo _appInfo;
        private MonoBehaviour _monobehavior;
        private IEnumerator _loadLanguagesCoroutine;
        private bool _running = false;

        public bool WriteCsv = true;
        public const float DefaultTimeout = 20.0f;
        public float Timeout = DefaultTimeout;
        public Action Loaded = null;

        public const string DefaultBundleDir = "localization";
        public string BundleDir = DefaultBundleDir;

        public static readonly string[] DefaultSupportedLanguages = {
            Localization.EnglishIdentifier,
            Localization.SpanishIdentifier,
            Localization.GermanIdentifier,
            Localization.FrenchIdentifier,
            Localization.TurkishIdentifier,
            Localization.ItalianIdentifier,
            Localization.PortugueseIdentifier,
            Localization.JapaneseIdentifier,
            Localization.KoreanIdentifier,
            Localization.ChineseIdentifier,
            Localization.RussianIdentifier,
            Localization.CatalanIdentifier,
            Localization.GalicianIdentifier,
            Localization.BasqueIdentifier
        };
        public string[] SupportedLanguages = DefaultSupportedLanguages;

        LocationData _location = new LocationData();
        public LocationData Location
        {
            get
            {
                return _location;
            }
        }

        Localization _localization;
        public Localization Localization
        {
            get
            {
                if(_localization == null)
                {
                    return Localization.Default;
                }
                return _localization;
            }

            set
            {
                _localization = value;
            }
        }
                
        private string _currentLanguage;
        public string CurrentLanguage
        {
            get
            {
                if(string.IsNullOrEmpty(_currentLanguage))
                {
                    _currentLanguage = GetSupportedLanguage();
                }
                return _currentLanguage;
            }
            
            set
            {
                var oldLang = _currentLanguage;
                _currentLanguage = GetSupportedLanguage(value);
                if(_running && oldLang != _currentLanguage)
                {
                    LoadCurrentLanguage();
                    DownloadCurrentLanguage();
                }
            }
        }

        public LocalizationManager(IHttpClient client, IAppInfo appInfo, MonoBehaviour monobehavior)
        {
            _httpClient = client;
            _appInfo = appInfo;
            _monobehavior = monobehavior;

            if(_httpClient == null)
            {
                throw new ArgumentNullException("httpClient", "httpClient cannot be null or empty!");
            }
            if(_appInfo == null)
            {
                throw new ArgumentNullException("appInfo", "appInfo cannot be null or empty!");
            }
            if(_monobehavior == null)
            {
                throw new ArgumentNullException("monobehavior", "monobehavior cannot be null or empty!");
            }
        }

        public void Start()
        {
            _running = true;
            _cachePath = Path.Combine(PathsManager.TemporaryCachePath, "localization");
            FileUtils.CreateDirectory(_cachePath);
            _bundlePath = Path.Combine(PathsManager.StreamingAssetsPath, BundleDir);

            LoadCurrentLanguage();
            
            #if UNITY_EDITOR
            DownloadSupportedLanguages(() => {
                LoadCurrentLanguage();
            });
            #else
            DownloadCurrentLanguage();
            #endif
        }

        public void Stop()
        {
            _running = false;
        }

        void DownloadSupportedLanguages(Action finish, IDictionary<string,Localization> locales =null)
        {
            if(locales == null)
            {
                locales = new Dictionary<string, Localization>();
            }
            if(_running == false || locales.Count >= SupportedLanguages.Length)
            {
                if(WriteCsv)
                {
                    var csv = LocalizationsToCsv(locales);
                    var resDir = Path.Combine(PathsManager.DataPath, "Resources");
                    var localFile = Path.Combine(resDir, "Localization.csv");
                    FileUtils.WriteAllText(localFile, csv);
                }
                if(finish != null)
                {
                    finish();
                }
                return;
            }
            var lang = SupportedLanguages[locales.Count];
            DownloadLocalization(lang, () => {
                var locale = new Localization();
                LoadLocalizationData(locale, lang);
                locales[lang] = locale;
                DownloadSupportedLanguages(finish, locales);
            });
        }

        bool LoadCurrentLanguage()
        {
            if(LoadLocalizationData(Localization, CurrentLanguage))
            {
                if(Loaded != null)
                {
                    Loaded();
                }
                return true;
            }
            return false;
        }
        
        void DownloadCurrentLanguage()
        {
            DownloadLocalization(CurrentLanguage, () => {
                LoadCurrentLanguage();
            });
        }

        string LocalizationsToCsv(IDictionary<string,Localization> locales)
        {
            List<string> keys = null;
            var builder = new StringBuilder();
            
            builder.Append("KEY");
            builder.Append(CsvSeparator);
            foreach(var pair in locales)
            {
                string lang = pair.Key;
                builder.Append(lang);
                builder.Append(CsvSeparator);
                if(keys == null)
                {
                    keys = new List<string>(pair.Value.Strings.Keys);
                }
            }
            builder.Remove(builder.Length - 1, 1);
            builder.AppendLine();
            
            foreach(var key in keys)
            {
                builder.Append(key);
                builder.Append(CsvSeparator);
                foreach(var pair in locales)
                {
                    var val = pair.Value.Get(key);
                    val = val.Replace("\"", "\\\"");
                    val = val.Replace("\n", "\\\n");
                    val = val.Replace("\t", "\\\t");
                    builder.Append("\"" + val + "\"");
                    builder.Append(CsvSeparator);
                }
                builder.Remove(builder.Length - 1, 1);
                builder.AppendLine();
            }
            builder.Remove(builder.Length - 1, 1);
            builder.AppendLine();
            
            return builder.ToString();
        }

        bool LoadLocalizationData(Localization locale, string lang)
        {
            var file = FindLocalizationFile(lang);
            if(string.IsNullOrEmpty(file))
            {
                file = Path.Combine(_bundlePath, lang + JsonExtension);
            }
            if(!FileUtils.Exists(file))
            {
                return false;
            }
            locale.Clear();
            var data = FileUtils.ReadAllBytes(file);
            AttrList attr = null;
            try
            {
                attr = new JsonAttrParser().Parse(data).AssertList;
            }
            catch(SerializationException)
            {
            }
            if(attr == null)
            {
                return false;
            }
            foreach(var elm in attr)
            {
                foreach(var entry in elm.AssertDic)
                {
                    locale.Set(entry.Key, entry.Value.AsValue.ToString());
                }
            }
            return true;
        }

        string GetLocalizationPathPrefix(string lang)
        {
            return Path.Combine(_cachePath, _location.Format(FilePrefixFormat, lang));
        }

        string FindLocalizationFile(string lang)
        {
            var files = FileUtils.GetFilesInDirectory(_cachePath);
            var prefix = GetLocalizationPathPrefix(lang);
            foreach(var file in files)
            {
                var fileExtension = Path.GetExtension(file).ToLower();

                if(JsonExtension != fileExtension)
                {
                    continue;
                }
                    
                if(!file.Contains(prefix))
                {
                    continue;
                }

                return file;
                
            }
            return string.Empty;
        }

        void DownloadLocalization(string lang, Action finish)
        {
            var url = _location.GetUrl(lang);
            var request = new HttpRequest(url, HttpRequest.MethodType.GET);

            var etag = FindLanguageEtag(lang);
            if(!string.IsNullOrEmpty(etag))
            {
                request.AddHeader(kIfNoneMatchHeader, "\"" + etag + "\"");
            }
            request.AcceptCompressed = true;
            request.Timeout = Timeout;

            _httpClient.Send(request, (HttpResponse resp) => OnLocalizationDownload(resp, lang, etag, finish));
        }

        void OnLocalizationDownload(HttpResponse resp, string lang, string oldEtag, Action finish)
        {
            if(resp.StatusCode == (int)HttpResponse.StatusCodeType.NotModified || resp.HasError)
            {
                if(finish != null)
                {
                    finish();
                }
                return;
            }
            string newEtag = null;
            if(resp.Headers.TryGetValue(kEtagHeader, out newEtag))
            {
                newEtag = newEtag.Replace("\"", "");
            }
            var json = resp.Body;
            if(json == null || json.Length == 0 || string.IsNullOrEmpty(newEtag))
            {
                if(finish != null)
                {
                    finish();
                }
                return;
            }
            var prefix = GetLocalizationPathPrefix(lang);
            string newLocalPath = prefix + newEtag + JsonExtension;
            string oldLocalPath = prefix + oldEtag + JsonExtension;
            FileUtils.WriteAllBytes(newLocalPath, json);

            if(!string.IsNullOrEmpty(oldEtag) && oldEtag != newLocalPath && FileUtils.Exists(oldLocalPath))
            {
                FileUtils.Delete(oldLocalPath);
            }

            if(finish != null)
            {
                finish();
            }
        }

        static string FixLanguage(string lang)
        {
            if(Localization.CatalanIdentifier == lang || Localization.BasqueIdentifier == lang || Localization.GalicianIdentifier == lang)
            {
                return Localization.SpanishIdentifier;
            }
            else if(Localization.PortugueseIdentifier == lang)
            {
                return Localization.BrasilianIdentifier;
            }
            else if(TraditionalChineseIdentifier == lang)  
            {
                return TraditionalChineseServerIdentifier; // It's prepared to check it, but it has to be added to the supportedLanguages vector above in order to work
            }
            else if(SimplifiedChineseIdentifier == lang)
            {
                return SimplifiedChineseServerIdentifier; // Conversion needed cause SP server did not support "zh-Hans" as Chinese identifier
            }
            else
            {
                return lang;
            }
        }

        string GetSupportedLanguage(string lang = null)
        {
            string slang = null;
            if(string.IsNullOrEmpty(lang))
            {
                lang = _appInfo.Language;
            }
            var supported = new List<string>(SupportedLanguages);
            var fixlang = FixLanguage(lang);
            if(supported.Contains(lang) || supported.Contains(fixlang))
            {
                slang = fixlang;
            }
            if(slang == null)
            {
                var i = lang.IndexOf('-');
                if(i >= 0)
                {
                    var sublang = lang.Substring(0, i);
                    fixlang = FixLanguage(sublang);
                    if(supported.Contains(lang) || supported.Contains(fixlang))
                    {
                        slang = fixlang;
                    }
                }
            }
            if(slang == null)
            {
                slang = Localization.EnglishIdentifier;
            }
            return slang;
        }

        string FindLanguageEtag(string lang)
        {
            var file = FindLocalizationFile(lang);
            if(!string.IsNullOrEmpty(file))
            {            
                int start = file.LastIndexOf("_");
                int end = file.LastIndexOf(".");
                if(start != -1 && end != -1)
                {
                    return file.Substring(start + 1, end - start - 1);
                }
            }
            return null;
        }
    }
}
