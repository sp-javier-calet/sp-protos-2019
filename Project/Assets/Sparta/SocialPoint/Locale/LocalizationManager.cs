using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Network;

namespace SocialPoint.Locale
{
    public class LocalizationManager : ILocalizationManager
    {
        public sealed class LocationData
        {
            public const string DevEnvironmentId = "dev";
            public const string ProdEnvironmentId = "prod";

            public const string DefaultProjectId = "lod";

            // Android uses iOS json too
            public const string DefaultPlatform = "ios";

            public const string DefaultDevEnvironmentId = DevEnvironmentId;
            public const string DefaultDevSecretKey = "AE7ffBEUZXNzfTe0";
            public const string DefaultProdEnvironmentId = ProdEnvironmentId;
            public const string DefaultProdSecretKey = "GpfC4F40mwkX0jBX";

            const string UrlFormat = "http://sp-translations.socialpointgames.com/deploy/<PROJ>/<PLAT>/<ENV>/<PROJ>_<PLAT>_<LANG>_<ENV>_<KEY>.json";
            const string ProjectIdPlaceholder = "<PROJ>";
            const string PlatformPlaceholder = "<PLAT>";
            const string EnvionmentIdPlaceholder = "<ENV>";
            const string SecretKeyPlaceholder = "<KEY>";
            const string LanguagePlaceholder = "<LANG>";

            public string ProjectId = DefaultProjectId;
            public string EnvironmentId = DefaultDevEnvironmentId;
            public string SecretKey = DefaultDevSecretKey;
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

        public enum CsvMode
        {
            WriteCsv,
            WriteCsvWithAllSupportedLanguages,
            NoCsv
        }

        const string JsonExtension = ".json";
        const string EtagHeader = "Etag";
        const string IfNoneMatchHeader = "If-None-Match";
        const string FilePrefixFormat = "<PROJ>_localization_<PLAT>_<ENV>_<LANG>_";
        const string CsvSeparator = ",";

        public const string SimplifiedChineseServerIdentifier = "zh";
        public const string TraditionalChineseServerIdentifier = "tw";

        // CultureInfo identifiers
        // http://www.localeplanet.com/dotnet/
        public const string EnglishUSIdentifier = "en-US";
        public const string SpanishESIdentifier = "es-ES";
        public const string PortugueseBRIdentifier = "pt-BR";
        public const string PortuguesePTIdentifier = "pt-PT";
        public const string FrenchFRIdentifier = "fr-FR";
        public const string TurkishTRIdentifier = "tr-TR";
        public const string ItalianITIdentifier = "it-IT";
        public const string JapaneseJPIdentifier = "ja-JP";
        public const string KoreanKRIdentifier = "ko-KR";
        public const string RussianRUIdentifier = "ru-RU";
        public const string GermanDEIdentifier = "de-DE";
        public const string ChineseCNIdentifier = "zh-CN";
        //zn-CHS and zn-CHT are neutral culture info (crashes).

        string _cachePath;
        string _bundlePath;
        IHttpClient _httpClient;
        IAppInfo _appInfo;
        bool _running;
        IHttpConnection _httpConn;
        bool _writeCsv = true;
        bool _loadAllSupportedLanguagesCsv = true;

        public const float DefaultTimeout = 20.0f;
        public float Timeout = DefaultTimeout;

        public event Action<Dictionary<string, Localization>> Loaded = delegate{};

        public const string DefaultBundleDir = "localization";
        public string BundleDir = DefaultBundleDir;

        string _fallbackLanguage;

        public string FallbackLanguage
        {
            get
            {
                if(string.IsNullOrEmpty(_fallbackLanguage) && Location.EnvironmentId == LocationData.ProdEnvironmentId)
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
            Localization.RussianIdentifier,
            Localization.JapaneseIdentifier,
            Localization.KoreanIdentifier,
            Localization.BrasilianIdentifier,
            Localization.GalicianIdentifier,
            Localization.BasqueIdentifier,
            Localization.CatalanIdentifier,
            Localization.SimplifiedChineseIdentifier,
            Localization.TraditionalChineseIdentifier
        };

        HashSet<string> _supportedFixedLanguages = new HashSet<string>();

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

                _supportedFixedLanguages.Clear();
                for(int i = 0, _supportedLanguagesLength = _supportedLanguages.Length; i < _supportedLanguagesLength; i++)
                {
                    var lang = _supportedLanguages[i];
                    _supportedFixedLanguages.Add(FixLanguage(lang));
                }
            }
        }

        public CultureInfo CurrentCultureInfo{ get; private set; }

        public CultureInfo SelectedCultureInfo{ get; private set; }

        public delegate void CsvLoadedDelegate(byte[] bytes);

        CsvLoadedDelegate CsvLoaded;

        LocationData _location;

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
                if(_localization == null)
                {
                    _localization = Localization.Default;
                }
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

        // language applied after selection (supported one).
        string _currentLanguage;

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

        // language selected by the user
        string _selectedLanguage;

        public string SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
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

        public bool SaveCSVFile{ get; set; }

        public bool CopyAllFilesToBundleFolder{ get; set; }

        public LocalizationManager(IHttpClient httpClient, IAppInfo appInfo, Localization locale = null, CsvMode csvMode = CsvMode.WriteCsvWithAllSupportedLanguages, CsvLoadedDelegate csvLoaded = null)
        {
            _httpClient = httpClient;
            _appInfo = appInfo;
            _writeCsv = csvMode == CsvMode.WriteCsv || csvMode == CsvMode.WriteCsvWithAllSupportedLanguages;
            _loadAllSupportedLanguagesCsv = csvMode == CsvMode.WriteCsvWithAllSupportedLanguages;
            CsvLoaded = csvLoaded;

            if(_httpClient == null)
            {
                throw new ArgumentNullException("httpClient", "httpClient cannot be null or empty!");
            }
            if(_appInfo == null)
            {
                throw new ArgumentNullException("appInfo", "appInfo cannot be null or empty!");
            }
            _localization = locale;
            if(_localization == null)
            {
                _localization = Localization.Default;
            }
            SupportedLanguages = DefaultSupportedLanguages; 

            _currentLanguage = GetSupportedLanguage(_currentLanguage);
            PathsManager.CallOnLoaded(Init);

            LoadCurrentLanguage();
        }

        void OnGameWasLoaded()
        {
            Load();
        }

        virtual public void Dispose()
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

        void Init()
        {
            _cachePath = Path.Combine(PathsManager.TemporaryDataPath, "localization");
            FileUtils.CreateDirectory(_cachePath);
            _bundlePath = Path.Combine(PathsManager.StreamingAssetsPath, BundleDir);
        }

        public void Load()
        {
            if(_running)
            {
                return;
            }
            _running = true;

            #if ADMIN_PANEL
            Location.EnvironmentId = LocationData.DefaultDevEnvironmentId;
            Location.SecretKey = LocationData.DefaultDevSecretKey;
            #else
            Location.EnvironmentId = LocationData.DefaultProdEnvironmentId;
            Location.SecretKey = LocationData.DefaultProdSecretKey;
            #endif

            LoadCurrentLanguage();

            #if !UNITY_EDITOR
            CopyAllFilesToBundleFolder = false;
            #endif

            if(CopyAllFilesToBundleFolder)
            {
                DownloadSupportedLanguages(() => LoadCurrentLanguage());
            }
            else
            {
                DownloadCurrentLanguage();
            }
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

        void DownloadSupportedLanguages(Action finish, Dictionary<string,Localization> locales = null, IEnumerator<string> langEnumerator = null)
        {
            if(locales == null)
            {
                locales = new Dictionary<string, Localization>();
            }
            if(!_running || (langEnumerator != null && !langEnumerator.MoveNext()))
            {
                langEnumerator.Dispose();
                OnLanguagesLoaded(locales);
                if(finish != null)
                {
                    finish();
                }
                return;
            }
            if(langEnumerator == null)
            {
                langEnumerator = _supportedFixedLanguages.GetEnumerator();
                langEnumerator.MoveNext();
            }
            var lang = langEnumerator.Current;

            DownloadLocalization(lang, () => OnDownloadLocalization(lang, finish, locales, langEnumerator));
        }

        void OnDownloadLocalization(string lang, Action finish, Dictionary<string, Localization> locales, IEnumerator<string> langEnumerator)
        {
            var locale = new Localization();
            LoadLocalizationData(locale, lang);
            locales[lang] = locale;
            DownloadSupportedLanguages(finish, locales, langEnumerator);
        }

        void OnLanguagesLoaded(Dictionary<string, Localization> locales)
        {
            if(_writeCsv)
            {
                if(_loadAllSupportedLanguagesCsv)
                {
                    var itr = _supportedFixedLanguages.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var slang = itr.Current;
                        if(!locales.ContainsKey(slang))
                        {
                            var slocale = new Localization();
                            if(LoadLocalizationData(slocale, slang))
                            {
                                locales[slang] = slocale;
                            }
                        }
                    }
                    itr.Dispose();
                }
                var csv = LocalizationsToCsv(locales);
                if(CsvLoaded != null)
                {
                    byte[] csvData = Encoding.UTF8.GetBytes(csv);
                    CsvLoaded(csvData);
                }

                #if UNITY_EDITOR
                if(SaveCSVFile)
                {
                    var resDir = Path.Combine(PathsManager.DataPath, "Resources");
                    var localFile = Path.Combine(resDir, "Localization.csv");
                    FileUtils.WriteAllText(localFile, csv);
                }
                #endif
            }

            Loaded(locales);
        }

        void DownloadCurrentLanguage()
        {
            if(_localization == null)
            {
                return;
            }

            DownloadLocalization(CurrentLanguage, OnDownloadLocalization);
        }

        void OnDownloadLocalization()
        {
            if(!LoadCurrentLanguage())
            {
                DownloadLocalization(FallbackLanguage, () => LoadCurrentLanguage());
            }
        }

        bool LoadCurrentLanguage()
        {
            if(_localization == null)
            {
                return false;
            }

            // load fallback localization
            var flang = FallbackLanguage;
            if(!string.IsNullOrEmpty(flang))
            {
                _localization.Fallback = new Localization();
                LoadLocalizationData(_localization.Fallback, flang);
            }

            if(LoadLocalizationData(_localization, CurrentLanguage))
            {
                var locales = new Dictionary<string, Localization>();
                locales[CurrentLanguage] = _localization;
                OnLanguagesLoaded(locales);
                return true;
            }
            return false;
        }

        static string LocalizationsToCsv(Dictionary<string,Localization> locales)
        {
            HashSet<string> keys = null;
            var builder = new StringBuilder();

            builder.Append("KEY");
            builder.Append(CsvSeparator);
            var itr = locales.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                string lang = pair.Key;
                builder.Append(lang);
                builder.Append(CsvSeparator);
                if(keys == null)
                {
                    keys = new HashSet<string>(pair.Value.Strings.Keys);
                    if(pair.Value.Fallback != null)
                    {
                        var itr2 = pair.Value.Fallback.Strings.Keys.GetEnumerator();
                        while(itr2.MoveNext())
                        {
                            var fkey = itr2.Current;
                            if(!keys.Contains(fkey))
                            {
                                keys.Add(fkey);
                            }
                        }
                        itr2.Dispose();
                    }
                }
            }
            itr.Dispose();

            builder.Remove(builder.Length - 1, 1);
            builder.AppendLine();

            var itrHashSet = keys.GetEnumerator();
            while(itrHashSet.MoveNext())
            {
                var key = itrHashSet.Current;
                builder.Append(key);
                builder.Append(CsvSeparator);
                itr = locales.GetEnumerator();
                while(itr.MoveNext())
                {
                    var pair = itr.Current;
                    var val = pair.Value.Get(key, string.Empty);
                    val = val.Replace("\n", @"\n");
                    val = val.Replace("\t", @"\t");
                    builder.Append("\"" + val + "\"");
                    builder.Append(CsvSeparator);
                }
                itr.Dispose();
                builder.Remove(builder.Length - 1, 1);
                builder.AppendLine();
            }
            itrHashSet.Dispose();

            builder.Remove(builder.Length - 1, 1);
            builder.AppendLine();

            return builder.ToString();
        }

        bool LoadLocalizationData(Localization locale, string lang)
        {
            locale.Clear();
            locale.Language = lang;
            bool fileFromCache = true;

            var file = FindLocalizationFile(lang);
            if(string.IsNullOrEmpty(file))
            {
                file = Path.Combine(_bundlePath, lang + JsonExtension);
                fileFromCache = false;
            }
            if(!FileUtils.ExistsFile(file))
            {
                return false;
            }
            var data = FileUtils.ReadAllBytes(file);
            AttrList attr;
            try
            {
                attr = new JsonAttrParser().Parse(data).AssertList;
            }
            catch(SerializationException)
            {
                if(fileFromCache)
                {
                    FileUtils.DeleteFile(file);
                }
                return false;
            }
            if(attr == null)
            {
                return false;
            }

            var itr = attr.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                var itr2 = elm.AssertDic.GetEnumerator();
                while(itr2.MoveNext())
                {
                    var entry = itr2.Current;
                    var val = entry.Value.AsValue.ToString();
                    val = val.Replace(@"\n", "\n");
                    val = val.Replace(@"\t", "\t");
                    locale.Set(entry.Key, val);
                }
                itr2.Dispose();
            }
            itr.Dispose();
                
            if(CopyAllFilesToBundleFolder)
            {
                var localFile = Path.Combine(_bundlePath, lang + JsonExtension);
                FileUtils.WriteAllBytes(localFile, data);
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

            if(files == null)
            {
                return string.Empty;
            }

            var prefix = GetLocalizationPathPrefix(lang);
            for(int i = 0, filesLength = files.Length; i < filesLength; i++)
            {
                var file = files[i];
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
            var request = new HttpRequest(url);

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
            string newEtag;
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

            if(!string.IsNullOrEmpty(oldEtag) && oldEtag != newLocalPath && FileUtils.ExistsFile(oldLocalPath))
            {
                FileUtils.DeleteFile(oldLocalPath);
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
            case Localization.TraditionalChineseIdentifier:
            case Localization.TraditionalChineseIdentifierCountry:
            case Localization.TraditionalHongKongChineseIdentifier:
            case Localization.TraditionalHongKongChineseIdentifierCountry:
                return TraditionalChineseServerIdentifier;
            case Localization.SimplifiedChineseIdentifier:
            case Localization.SimplifiedChineseIdentifierCountry:
                return SimplifiedChineseServerIdentifier;
            default:
                return lang;
            }
        }

        string GetSupportedLanguage(string lang = null)
        {
            string slang = null;
            string country;
            var supported = new List<string>(_supportedFixedLanguages);

            if(string.IsNullOrEmpty(lang))
            {
                lang = _appInfo.Language;
                country = _appInfo.Country;
                if(!string.IsNullOrEmpty(country))
                {
                    lang = lang + "-" + country;
                }
            }

            _selectedLanguage = lang;
            SelectedCultureInfo = GetCultureInfo(_selectedLanguage);

            var fixlang = FixLanguage(lang);

            if(supported.Contains(lang) || supported.Contains(fixlang))
            {
                slang = fixlang;
            }

            if(string.IsNullOrEmpty(slang))
            {
                var i = lang.LastIndexOf('-');
                if(i >= 0)
                {
                    var sublang = lang.Substring(0, i);
                    fixlang = FixLanguage(sublang);
                    if(supported.Contains(sublang) || supported.Contains(fixlang))
                    {
                        slang = fixlang;
                    }
                }
            }
            if(string.IsNullOrEmpty(slang))
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
                return new CultureInfo(PortuguesePTIdentifier);
            case Localization.BrasilianIdentifier:
                return new CultureInfo(PortugueseBRIdentifier);
            case Localization.SimplifiedChineseIdentifier:
            case SimplifiedChineseServerIdentifier:
            case Localization.TraditionalChineseIdentifier:
            case TraditionalChineseServerIdentifier:
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
            return string.Empty;
        }
    }
}
