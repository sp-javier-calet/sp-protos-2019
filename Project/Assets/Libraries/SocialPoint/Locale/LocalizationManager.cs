﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.AppEvents;

namespace SocialPoint.Locale
{
    public class LocalizationManager : ILocalizationManager
    {
        public class LocationData
        {
            public const string DevEnvironmentId = "dev";
            public const string ProdEnvironmentId = "prod";

            public const string DefaultProjectId = "dc";

            // Android uses iOS json too
            public const string DefaultPlatform = "ios";

            //public const string DefaultEnvironmentId = DevEnvironmentId;
            //public const string DefaultSecretKey = "5TgemMFH4yj7RJ3d";
            public const string DefaultEnvironmentId = ProdEnvironmentId;
            public const string DefaultSecretKey = "wetd46pWuR8J5CmS";

            const string UrlFormat = "http://sp-translations.socialpointgames.com/deploy/<PROJ>/<PLAT>/<ENV>/<PROJ>_<PLAT>_<LANG>_<ENV>_<KEY>.json";
            const string ProjectIdPlaceholder = "<PROJ>";
            const string PlatformPlaceholder = "<PLAT>";
            const string EnvionmentIdPlaceholder = "<ENV>";
            const string SecretKeyPlaceholder = "<KEY>";
            const string LanguagePlaceholder = "<LANG>";

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

        const string JsonExtension = ".json";
        const string EtagHeader = "Etag";
        const string IfNoneMatchHeader = "If-None-Match";
        const string FilePrefixFormat = "<PROJ>_localization_<PLAT>_<ENV>_<LANG>_";
        const string CsvSeparator = ",";

        public const string SimplifiedChineseServerIdentifier = "zh";
        public const string TraditionalChineseServerIdentifier = "tw";
        public const string SimplifiedChineseIdentifier = "zh-Hans";
        public const string TraditionalChineseIdentifier = "zh-Hant";

        // CultureInfo identifiers
        // http://www.localeplanet.com/dotnet/
        public const string EnglishUSIdentifier = "en-US";
        public const string SpanishESIdentifier = "es-ES";
        public const string PortugueseBRIdentifier = "pt-BR";
        public const string FrenchFRIdentifier = "fr-FR";
        public const string TurkishTRIdentifier = "tr-TR";
        public const string ItalianITIdentifier = "it-IT";
        public const string JapaneseJPIdentifier = "ja-JP";
        public const string KoreanKRIdentifier = "ko-KR";
        public const string RussianRUIdentifier = "ru-RU";
        public const string GermanDEIdentifier = "de-DE";
        public const string ChineseCNIdentifier = "zh-CN";

        private string _cachePath;
        private string _bundlePath;
        private IHttpClient _httpClient;
        private IAppInfo _appInfo;
        private bool _running = false;
        private IHttpConnection _httpConn;

        public bool WriteCsv = true;
        public const float DefaultTimeout = 20.0f;
        public float Timeout = DefaultTimeout;
        public event Action Loaded = delegate{};

        public const string DefaultBundleDir = "localization";
        public string BundleDir = DefaultBundleDir;

        private string _fallbackLanguage;
        public string FallbackLanguage
        {
            get
            {
                if(_fallbackLanguage == null && Location.EnvironmentId == LocationData.ProdEnvironmentId)
                {
                    return Localization.EnglishIdentifier;
                }
                return _fallbackLanguage;
            }

            set
            {
                _fallbackLanguage = value;
            }
        }

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
        string[] _supportedLanguages = DefaultSupportedLanguages;
        public string[] SupportedLanguages
        {
            get
            {
                return _supportedLanguages;
            }

            set
            {
                _supportedLanguages = value;
            }
        }

        public static CultureInfo CurrentCultureInfo{ get; private set; }

        public delegate void CsvLoadedDelegate(byte[] bytes);

        public CsvLoadedDelegate CsvLoaded = null;

        LocationData _location = null;

        public LocationData Location
        {
            get
            {
                if(_location == null)
                {
                    _location = new LocationData();
                }
                return _location;
            }
        }

        Localization _localization;

        public Localization Localization
        {
            get
            {
                return _localization;
            }

            set
            {
                if(_localization != value)
                {
                    _localization = value;
                    UpdateCurrentLanguage();
                }
            }
        }

        private string _currentLanguage;

        public string CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }

            set
            {
                var oldLang = _currentLanguage;
                _currentLanguage = GetSupportedLanguage(value);
                if(oldLang != _currentLanguage)
                {
                    UpdateCurrentLanguage();
                }
            }
        }

        IAppEvents _appEvents;
        public IAppEvents AppEvents
        {
            get
            {
                return _appEvents;
            }

            set
            {
                if(_appEvents != null)
                {
                    _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    _appEvents.GameWasLoaded.Add(0, OnGameWasLoaded);
                }
            }
        }

        public LocalizationManager(IHttpClient httpClient, IAppInfo appInfo, Localization locale=null)
        {
            _httpClient = httpClient;
            _appInfo = appInfo;

            if(_httpClient == null)
            {
                throw new ArgumentNullException("httpClient", "httpClient cannot be null or empty!");
            }
            if(_appInfo == null)
            {
                throw new ArgumentNullException("appInfo", "appInfo cannot be null or empty!");
            }
            _localization = locale;
            _currentLanguage = GetSupportedLanguage(_currentLanguage);
            PathsManager.CallOnLoaded(Init);
        }

        private void OnGameWasLoaded()
        {
            Load();
        }

        public void Dispose()
        {
            if(_httpConn != null)
            {
                _httpConn.Release();
                _httpConn = null;
            }
            _running = false;
            if(_appEvents != null)
            {
                _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            }
        }

        private void Init()
        {
            _cachePath = Path.Combine(PathsManager.TemporaryCachePath, "localization");
            FileUtils.CreateDirectory(_cachePath);
            _bundlePath = Path.Combine(PathsManager.StreamingAssetsPath, BundleDir);
            if(_localization == null)
            {
                _localization = Localization.Default;
            }
            UpdateCurrentLanguage();
        }

        public void Load()
        {
            _running = true;
            #if UNITY_EDITOR
            DownloadSupportedLanguages(() => LoadCurrentLanguage());
            #else
            DownloadCurrentLanguage();
            #endif
        }

        [Obsolete("Use Load()")]
        public void Start()
        {
            Load();
        }

        [Obsolete("Use Dispose() and only once")]
        public void Stop()
        {
            Dispose();
        }

        void UpdateCurrentLanguage()
        {
            if(_running)
            {
                DownloadCurrentLanguage();
            }
            else
            {
                LoadCurrentLanguage();
            }

        }

        void DownloadSupportedLanguages(Action finish, IDictionary<string,Localization> locales = null)
        {
            if(locales == null)
            {
                locales = new Dictionary<string, Localization>();
            }
            if(_running == false || locales.Count >= _supportedLanguages.Length)
            {
                OnLanguagesLoaded(locales);
                if(finish != null)
                {
                    finish();
                }
                return;
            }
            var lang = _supportedLanguages[locales.Count];
            DownloadLocalization(lang, () => {
                var locale = new Localization();
                LoadLocalizationData(locale, lang);
                locales[lang] = locale;
                DownloadSupportedLanguages(finish, locales);
            });
        }

        void OnLanguagesLoaded(IDictionary<string, Localization> locales)
        {
            if(WriteCsv)
            {
                foreach(var slang in SupportedLanguages)
                {
                    if(!locales.ContainsKey(slang))
                    {
                        var slocale = new Localization();
                        if(LoadLocalizationData(slocale, slang))
                        {
                            locales[slang] = slocale;
                        }
                    }
                }
                var csv = LocalizationsToCsv(locales);
                if(CsvLoaded != null)
                {
                    byte[] csvData = Encoding.UTF8.GetBytes(csv);
                    CsvLoaded(csvData);
                }

                #if UNITY_EDITOR
                var resDir = Path.Combine(PathsManager.DataPath, "Resources");
                var localFile = Path.Combine(resDir, "Localization.csv");
                FileUtils.WriteAllText(localFile, csv);
                #endif
            }
        }

        void DownloadCurrentLanguage()
        {
            DownloadLocalization(FallbackLanguage, () => {
                DownloadLocalization(CurrentLanguage, () => {
                    LoadCurrentLanguage();
                });
            });
        }

        void LoadCurrentLanguage()
        {
            if(_localization == null)
            {
                return;
            }
            // load fallback localization
            var flang = FallbackLanguage;
            if(flang != null)
            {
                _localization.Fallback = new Localization();
                LoadLocalizationData(_localization.Fallback, flang);
            }
            if(LoadLocalizationData(_localization, CurrentLanguage))
            {
                var locales = new Dictionary<string, Localization>();
                locales[CurrentLanguage] = _localization;
                OnLanguagesLoaded(locales);
                if(Loaded != null)
                {
                    Loaded();
                }
            }
        }

        static string LocalizationsToCsv(IDictionary<string,Localization> locales)
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
                    if(pair.Value.Fallback != null)
                    {
                        foreach(var fkey in pair.Value.Fallback.Strings.Keys)
                        {
                            if(!keys.Contains(fkey))
                            {
                                keys.Add(fkey);
                            }
                        }
                    }
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
                    val = val.Replace("\n", @"\n");
                    val = val.Replace("\t", @"\t");
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
            locale.Clear();
            locale.Language = lang;

            var file = FindLocalizationFile(lang);
            if(string.IsNullOrEmpty(file))
            {
                file = Path.Combine(_bundlePath, lang + JsonExtension);
            }
            if(!FileUtils.Exists(file))
            {
                return false;
            }
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
                    var val = entry.Value.AsValue.ToString();
                    val = val.Replace(@"\n", "\n");
                    val = val.Replace(@"\t", "\t");
                    locale.Set(entry.Key, val);
                }
            }

            #if UNITY_EDITOR
            var localFile = Path.Combine(_bundlePath, lang + JsonExtension);
            FileUtils.WriteAllBytes(localFile, data);
            #endif

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
            if(string.IsNullOrEmpty(lang))
            {
                if(finish != null)
                {
                    finish();
                }
                return;
            }
            var url = _location.GetUrl(lang);
            var request = new HttpRequest(url, HttpRequest.MethodType.GET);

            var etag = FindLanguageEtag(lang);
            if(!string.IsNullOrEmpty(etag))
            {
                request.AddHeader(IfNoneMatchHeader, "\"" + etag + "\"");
            }
            request.AcceptCompressed = true;
            request.Timeout = Timeout;

            _httpConn = _httpClient.Send(request, resp => OnLocalizationDownload(resp, lang, etag, finish));
        }

        void OnLocalizationDownload(HttpResponse resp, string lang, string oldEtag, Action finish)
        {
            _httpConn = null;
            if(resp.StatusCode == (int)HttpResponse.StatusCodeType.NotModified || resp.HasError)
            {
                if(finish != null)
                {
                    finish();
                }
                return;
            }
            string newEtag = null;
            if(resp.Headers.TryGetValue(EtagHeader, out newEtag))
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
            switch(lang)
            {
            case Localization.CatalanIdentifier:
            case Localization.BasqueIdentifier:
            case Localization.GalicianIdentifier:
                return Localization.SpanishIdentifier;
            case Localization.PortugueseIdentifier:
                return Localization.BrasilianIdentifier;
            case TraditionalChineseIdentifier:
                return TraditionalChineseServerIdentifier;
            case SimplifiedChineseIdentifier:
                return SimplifiedChineseServerIdentifier;
            default:
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
            var supported = new List<string>(_supportedLanguages);
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

            CurrentCultureInfo = GetCultureInfo(slang);

            return slang;
        }

        static CultureInfo GetCultureInfo(string lang)
        {
            switch(lang)
            {
            case Localization.EnglishIdentifier:
                return new CultureInfo(EnglishUSIdentifier);
            case Localization.FrenchIdentifier:
                return new CultureInfo(FrenchFRIdentifier);
            case Localization.TurkishIdentifier:
                return new CultureInfo(TurkishTRIdentifier);
            case Localization.ItalianIdentifier:
                return new CultureInfo(ItalianITIdentifier);
            case Localization.JapaneseIdentifier:
                return new CultureInfo(JapaneseJPIdentifier);
            case Localization.KoreanIdentifier:
                return new CultureInfo(KoreanKRIdentifier);
            case Localization.RussianIdentifier:
                return new CultureInfo(RussianRUIdentifier);
            case Localization.SpanishIdentifier:
            case Localization.CatalanIdentifier:
            case Localization.GalicianIdentifier:
            case Localization.BasqueIdentifier:
                return new CultureInfo(SpanishESIdentifier);
            case Localization.GermanIdentifier:
                return new CultureInfo(GermanDEIdentifier);
            case Localization.PortugueseIdentifier:
                return new CultureInfo(PortugueseBRIdentifier);
            case Localization.ChineseIdentifier:
                return new CultureInfo(ChineseCNIdentifier);
            default:
                return CultureInfo.CurrentCulture;
            }
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
