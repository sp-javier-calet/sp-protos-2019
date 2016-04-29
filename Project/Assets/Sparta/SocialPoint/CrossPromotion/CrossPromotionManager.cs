using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.AppEvents;

namespace SocialPoint.CrossPromotion
{
    public class CrossPromotionManager : IDisposable
    {
        public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

        public TrackEventDelegate TrackSystemEvent = null;
        public TrackEventDelegate TrackUrgentSystemEvent = null;

        public delegate void CreateIconDelegate();

        public CreateIconDelegate CreateIcon = null;

        public delegate void CreatePopupDelegate();

        public CreatePopupDelegate CreatePopup = null;
        const int kCrossFailByAssetFailedErrorCode = 1;
        const int kCrossFailByPopupTimeOutErrorCode = 2;
        const string kAssetBundleExtension = ".assetBundle";
        const string kAppsChecked = "xpromo_check_apps";
        const string kLastAutoShowPopup = "xpromo_last_auto_show_popup";
        #if UNITY_IPHONE
        const string kDefaultAppsToCheck = "dragoncity://,monsterlegends://,dragonland://,restaurantcity://,dragonstadium://";
        #elif UNITY_ANDROID
        const string kDefaultAppsToCheck = "es.socialpoint.DragonCity,es.socialpoint.MonsterLegends,es.parrotgames.restaurantcity,es.socialpoint.dragonland";
        #else
        const string kDefaultAppsToCheck = "";
        #endif
        CrossPromotionData _data = null;
        string _assetsPath;
        long _loginTime = 0;
        long _startTime = 0;
        bool _canOpenPopup = false;
        bool _isAutoOpened = false;
        bool _assetsReadyCalled = false;
        List<string> _assetsFailed = new List<string>();
        ICoroutineRunner _coroutineRunner = null;
        IAttrStorage _storage = null;
        IAppEvents _appEvents = null;
        CrossPromotionIconConfiguration _iconConfig = null;
        IEnumerator _trackBannerClickEventTimeoutCoroutine = null;
        int _remainingAssetsToDownload = -1;
        List<WWW> _currentDownloads = new List<WWW>();
        List<IEnumerator> _currentDownloadsCoroutines = new List<IEnumerator>();
        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        Texture2D _iconTexture = null;

        public CrossPromotionData Data
        {
            get { return _data; }
            private set { _data = value; }
        }

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
                    DisconnectAppEvents(_appEvents);
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    ConnectAppEvents(_appEvents);
                }
            }
        }

        protected bool IsAutoOpened
        {
            get
            {
                return _isAutoOpened;
            }
        }

        public CrossPromotionManager(ICoroutineRunner coroutineRunner)
        {
            DebugUtils.Assert(coroutineRunner != null);
            _coroutineRunner = coroutineRunner;
            _storage = new PlayerPrefsAttrStorage();
            _assetsPath = PathsManager.TemporaryDataPath;

            Reset();
        }

        void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.GameWillRestart.Add(-25, OnGameWillRestart);
        }

        void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.GameWillRestart.Remove(OnGameWillRestart);
        }

        void Reset()
        {
            DisposePopupTextures();
            DisposeIconTexture();
            ResetCurrentDownloads();
            _data = null;
            _loginTime = 0;
            _startTime = 0;
            _canOpenPopup = false;
            _isAutoOpened = false;
            _assetsReadyCalled = false;
            _assetsFailed.Clear();
            _iconConfig = null;
            _trackBannerClickEventTimeoutCoroutine = null;
            _remainingAssetsToDownload = -1;
        }

        void ResetCurrentDownloads()
        {
            for(int i = 0; i < _currentDownloadsCoroutines.Count; i++)
            {
                if(_currentDownloadsCoroutines[i] != null)
                {
                    _coroutineRunner.StopCoroutine(_currentDownloadsCoroutines[i]);
                }
            }
            _currentDownloadsCoroutines.Clear();

            for(int i = 0; i < _currentDownloads.Count; i++)
            {
                if(_currentDownloads[i] != null)
                {
                    _currentDownloads[i].Dispose();
                }
            }
            _currentDownloads.Clear();
        }

        void OnGameWillRestart()
        {
            Reset();
        }

        virtual public void Dispose()
        {
            Reset();
            if(_appEvents != null)
            {
                DisconnectAppEvents(_appEvents);
            }
        }

        public void Start()
        {
            if(_data == null)
            {
                return;
            }

            DebugUtils.Assert(TrackSystemEvent != null);
            DebugUtils.Assert(TrackUrgentSystemEvent != null);
            DebugUtils.Assert(AppEvents != null);
            DebugUtils.Assert(CreateIcon != null);
            DebugUtils.Assert(CreatePopup != null);

            _startTime = TimeUtils.Timestamp;
            _canOpenPopup = _data.ShowPopup && _data.BannerInfo.Count > 0 && CanOpenPopupByFreq();

            DownloadAllAssets();
        }

        public void Init(AttrDic config)
        {
            _data = new CrossPromotionData(config);
            _loginTime = TimeUtils.Timestamp;

            SendInitializedEvent();
            SaveGamesToCheck();
        }

        bool CanOpenPopupByFreq()
        {
            bool canOpenPopup = true;
            if(_storage.Has(kLastAutoShowPopup))
            {
                long lastOpenedTs = _storage.Load(kLastAutoShowPopup).AsValue.ToLong();
                canOpenPopup = _startTime - lastOpenedTs >= _data.PopupFrequency;
            }
            return canOpenPopup;
        }

        bool CanOpenPopupByTimeout()
        {
            return TimeUtils.Timestamp - _startTime < _data.PopupTimeout;
        }

        void DownloadAllAssets()
        {
            _assetsReadyCalled = false;
            HashSet<string> assetsToDownload = new HashSet<string>();
            assetsToDownload.Add(_data.IconImage);
            assetsToDownload.Add(_data.PopupTitleImage);
             
            foreach(var keyValue in _data.BannerInfo)
            {
                CrossPromotionBannerData banner = keyValue.Value;
                assetsToDownload.Add(banner.ButtonTextImage);
                assetsToDownload.Add(banner.BgImage);
                assetsToDownload.Add(banner.IconImage);
            }

            _remainingAssetsToDownload = assetsToDownload.Count;
             
            foreach(var url in assetsToDownload)
            {
                LoadAssetFromCacheOrDownload(url, OnAssetDownloaded);
            }
        }

        bool AreAssetsReady()
        {
            if(_remainingAssetsToDownload != 0)
            {
                return false;
            }

            bool allReady = true;
            allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(_data.IconImage)));
            allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(_data.PopupTitleImage)));

            foreach(var keyValue in _data.BannerInfo)
            {
                CrossPromotionBannerData banner = keyValue.Value;
                allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(banner.ButtonTextImage)));
                allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(banner.BgImage)));
                allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(banner.IconImage)));
            }

            return allReady;
        }

        void OnAssetsReady()
        {
            DebugUtils.Assert(AreAssetsReady());

            if(_assetsReadyCalled)
            {
                return;
            }
            _assetsReadyCalled = true;
            ResetCurrentDownloads();
            TryCreateIcon();
            TryAutoOpenPopup();
        }

        void OnAssetDownloaded(string url, bool success)
        {
            --_remainingAssetsToDownload;

            if(success)
            {
                if(AreAssetsReady())
                {
                    OnAssetsReady();
                }
            }
            else
            {
                if(_assetsFailed.Count == 0)
                {
                    _assetsFailed.Add(url);
                    SendCrossFailAssetFailedEvent();
                }
            }
        }

        void LoadAssetFromCacheOrDownload(string url, Action<string,bool> callback)
        {
            if(!string.IsNullOrEmpty(url))
            {
                _currentDownloadsCoroutines.Add(_coroutineRunner.StartCoroutine(LoadAssetFromCacheOrDownloadCoroutine(url, callback)));
            }
            else
            {
                callback(url, false);
            }
        }

        IEnumerator LoadAssetFromCacheOrDownloadCoroutine(string url, Action<string,bool> callback)
        {
            // Check if the file is in the cache
            string filePath = FileUtils.Combine(_assetsPath, Path.GetFileName(url));
            bool fileExist = FileUtils.ExistsFile(filePath);

            // If the file doesn't exist try to download it
            if(!fileExist)
            {
                WWW www = new WWW(url);
                _currentDownloads.Add(www);  
                yield return www;
                _currentDownloads.Remove(www);
                if(!string.IsNullOrEmpty(www.error) || www.bytes == null)
                {
                    www.Dispose();
                    callback(url, false);
                    yield break;
                }
                // Save the file on disk
                FileUtils.WriteAllBytes(filePath, www.bytes);
                www.Dispose();
            }

            callback(url, true);
        }

        void SaveGamesToCheck()
        {
            var sb = StringUtils.StartBuilder();
            foreach(var app in _data.AppsToCheck)
            {
                sb.Append(app);
                sb.Append(",");
            }
            if(sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            _storage.Save(kAppsChecked, new AttrString(StringUtils.FinishBuilder(sb)));
        }

        public string CheckedApps()
        {
            string appsToCheck = null;
            if(_storage.Has(kAppsChecked))
            {
                appsToCheck = _storage.Load(kAppsChecked).AsValue.ToString();
            }
            if(string.IsNullOrEmpty(appsToCheck))
            {
                appsToCheck = kDefaultAppsToCheck;
            }
            return appsToCheck;
        }

        public string InstalledApps()
        {
            StringBuilder sb = new StringBuilder();
            string appsToCheck = CheckedApps();
            string[] appsToCheckArray = appsToCheck.Split(',');
            foreach(var app in appsToCheckArray)
            {
                if(NativeUtils.IsInstalled(app))
                {
                    sb.Append(app);
                    sb.Append(",");
                }
            }
            if(sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        void TryCreateIcon()
        {
            if(_data.ShowIcon)
            {
                CreateIcon();
            }
        }

        void TryAutoOpenPopup()
        {
            bool canOpenPopupByTimeout = CanOpenPopupByTimeout();
            if(_canOpenPopup && !canOpenPopupByTimeout)
            {
                SendCrossFailPopupTimeoutEvent();
            }
           
            bool showPopup = _canOpenPopup && canOpenPopupByTimeout;
            if(showPopup)
            {
                _storage.Save(kLastAutoShowPopup, new AttrLong(TimeUtils.Timestamp));
                _isAutoOpened = true;
                CreatePopup();
            }
        }

        public void TryOpenPopup()
        {
            if(AreAssetsReady())
            {
                _isAutoOpened = false;
                CreatePopup();
            }
        }

        AttrDic GetEventBasicInformation(bool setIconId, bool setAutoOpen)
        {
            AttrDic data = new AttrDic();
            data.SetValue("xpromoid", _data.Id);
            data.SetValue("init_ts", _loginTime);
            
            if(setAutoOpen)
            {
                data.SetValue("automatically", _isAutoOpened);
            }
            
            if(setIconId)
            {
                data.SetValue("id", _data.IconId);
            }
            
            return data;
        }

        void AddBannerListData(AttrDic data)
        {
            AttrList bannerList = new AttrList();
            int position = 0;
            foreach(var keyValue in _data.BannerInfo)
            {
                CrossPromotionBannerData banner = keyValue.Value;
                AttrDic bannerData = new AttrDic();
                bannerData.SetValue("id", banner.Uid);
                bannerData.SetValue("xpromo_game", banner.Game);
                bannerData.SetValue("installed", NativeUtils.IsInstalled(banner.AppId));
                bannerData.SetValue("position", position);
                bannerList.Add(bannerData);
                ++position;
            }
            data.Set("banners", bannerList);
        }

        void SendInitializedEvent()
        {
            AttrDic data = GetEventBasicInformation(false, false);
            AddBannerListData(data);
            TrackSystemEvent("cross.initialized", data);
        }

        public void SendIconImpressedEvent()
        {
            TrackSystemEvent("cross.icon_impressed", GetEventBasicInformation(true, false));
        }

        public void SendIconClickedEvent()
        {
            TrackSystemEvent("cross.icon_clicked", GetEventBasicInformation(true, false));
        }

        public void SendPopupImpressedEvent()
        {
            AttrDic data = GetEventBasicInformation(false, true);
            AddBannerListData(data);
            TrackSystemEvent("cross.popup_impressed", data);
        }

        public void SendBannerImpressedEvent(int uid, int position)
        {
            AttrDic data = GetEventBasicInformation(false, true);
            CrossPromotionBannerData banner = _data.BannerInfo[uid];
            data.SetValue("id", uid);
            data.SetValue("position", position);
            data.SetValue("xpromo_game", banner.Game);
            data.SetValue("installed", NativeUtils.IsInstalled(banner.AppId));
            TrackSystemEvent("cross.banner_impressed", data);
        }

        public void SendBannerClickedEvent(int uid, int position, bool urgent, bool currentGame, Action endCallback)
        {
            AttrDic data = GetEventBasicInformation(false, true);
            CrossPromotionBannerData banner = _data.BannerInfo[uid];
            data.SetValue("id", uid);
            data.SetValue("position", position);
            data.SetValue("xpromo_game", banner.Game);
            data.SetValue("installed", NativeUtils.IsInstalled(banner.AppId));
            data.SetValue("urgent", urgent);

            if(urgent)
            {
                _trackBannerClickEventTimeoutCoroutine = _coroutineRunner.StartCoroutine(TrackBannerClickEventTimeoutCoroutine(uid, position, endCallback));

                TrackUrgentSystemEvent("cross.banner_clicked", data, (Error error) => {
                    if(_trackBannerClickEventTimeoutCoroutine != null)
                    {
                        _coroutineRunner.StopCoroutine(_trackBannerClickEventTimeoutCoroutine);
                        _trackBannerClickEventTimeoutCoroutine = null;
                        endCallback();
                        OpenApp(uid);
                    }
                });
            }
            else
            {
                TrackSystemEvent("cross.banner_clicked", data);
                endCallback();

                if(!currentGame)
                {
                    OpenApp(uid);
                }
            }
        }

        IEnumerator TrackBannerClickEventTimeoutCoroutine(int uid, int position, Action endCallback)
        {
            yield return new WaitForSeconds(_data.TrackTimeout);
            _trackBannerClickEventTimeoutCoroutine = null;
            SendBannerClickedEvent(uid, position, false, false, endCallback);
        }

        public void SendPopupClosedEvent(long timeDisplayed)
        {
            AttrDic data = GetEventBasicInformation(false, true);
            data.SetValue("time_displayed", timeDisplayed);
            AddBannerListData(data);
            TrackSystemEvent("cross.popup_closed", data);
        }

        void SendCrossFailAssetFailedEvent()
        {
            AttrDic data = new AttrDic();
            data.SetValue("error", kCrossFailByAssetFailedErrorCode);
            AttrList assetsFailed = new AttrList();
            foreach(var asset in _assetsFailed)
            {
                AttrDic assetData = new AttrDic();
                assetData.SetValue("src", asset);
                assetsFailed.Add(assetData);
            }
            data.Set("assets", assetsFailed);
            TrackSystemEvent("cross.failed", data);
        }

        void SendCrossFailPopupTimeoutEvent()
        {
            AttrDic data = new AttrDic();
            data.SetValue("error", kCrossFailByPopupTimeOutErrorCode);
            TrackSystemEvent("cross.failed", data);
        }

        void OpenApp(int uid)
        {
            CrossPromotionBannerData banner = _data.BannerInfo[uid];
            if(NativeUtils.IsInstalled(banner.AppId))
            {
                NativeUtils.OpenApp(banner.AppId);
            }
            else
            {
                NativeUtils.OpenStore(banner.StoreId);
            }
        }

        Texture2D GetTexture2DForImage(string url)
        {
            string filePath = FileUtils.Combine(_assetsPath, Path.GetFileName(url));
            byte[] data = FileUtils.ReadAllBytes(filePath);
            if(data != null)
            {
                Texture2D texture = new Texture2D(0, 0);
                bool ok = texture.LoadImage(data);
                if(ok)
                {
                    return texture;
                }
            }
            return null;
        }

        public Texture2D GetTexture2DForPopupImage(string url)
        {
            Texture2D texture;
            _textures.TryGetValue(url, out texture);
            if(texture == null)
            {
                texture = GetTexture2DForImage(url);
                if(texture != null)
                {
                    _textures.Add(url, texture);
                }
            }           
            return texture;
        }

        public Texture2D GetIconImage()
        {
            if(_iconTexture == null && IsIconAnImage())
            {
                _iconTexture = GetTexture2DForImage(_data.IconImage);
            }
            return _iconTexture;
        }

        public CrossPromotionIconConfiguration GetIconConfiguration()
        {
            if(_iconConfig == null)
            {
                string filePath = FileUtils.Combine(_assetsPath, Path.GetFileName(_data.IconImage));
                AssetBundle assetBundle = AssetBundle.LoadFromFile(filePath);
                if(assetBundle != null)
                {
                    GameObject obj = UnityEngine.Object.Instantiate(assetBundle.mainAsset) as GameObject;
                    if(obj != null)
                    {
                        _iconConfig = obj.GetComponent<CrossPromotionIconConfiguration>();
                    }
                    assetBundle.Unload(false);
                }
            }
            return _iconConfig;
        }

        public void DisposePopupTextures()
        {
            foreach(var keyValue in _textures)
            {
                if(keyValue.Value != null)
                {
                    keyValue.Value.Destroy();
                }
            }
            _textures.Clear();
        }

        void DisposeIconTexture()
        {
            if(_iconTexture != null)
            {
                _iconTexture.Destroy();
                _iconTexture = null;
            }
        }

        public bool CanShowIcon()
        {
            return (_data != null && _data.ShowIcon && AreAssetsReady());
        }

        public bool IsIconAnImage()
        {
            return Path.GetExtension(_data.IconImage) != kAssetBundleExtension;
        }
    }
}
