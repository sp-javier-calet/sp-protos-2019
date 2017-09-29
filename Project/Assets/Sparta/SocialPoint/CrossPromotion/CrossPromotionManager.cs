using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.CrossPromotion
{
    public class CrossPromotionManager : IDisposable
    {
        public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

        public TrackEventDelegate TrackSystemEvent;
        public TrackEventDelegate TrackUrgentSystemEvent;

        public delegate void CreateIconDelegate();

        public CreateIconDelegate CreateIcon;

        public delegate void CreatePopupDelegate();

        public CreatePopupDelegate CreatePopup;
        const int kCrossFailByAssetFailedErrorCode = 1;
        const int kCrossFailByPopupTimeOutErrorCode = 2;
        const string kAssetBundleExtension = ".assetBundle";
        const string kAppsChecked = "xpromo_check_apps";
        const string kLastAutoShowPopup = "xpromo_last_auto_show_popup";
        #if (UNITY_IOS || UNITY_TVOS)
        const string kDefaultAppsToCheck = "dragoncity://,monsterlegends://,dragonland://,restaurantcity://,dragonstadium://";
        #elif UNITY_ANDROID
        const string kDefaultAppsToCheck = "es.socialpoint.DragonCity,es.socialpoint.MonsterLegends,es.parrotgames.restaurantcity,es.socialpoint.dragonland";
        
#else
        const string kDefaultAppsToCheck = "";
        #endif
        string _assetsPath;
        long _loginTime;
        long _startTime;
        bool _canOpenPopup;
        bool _isAutoOpened;
        bool _assetsReadyCalled;
        List<string> _assetsFailed = new List<string>();
        ICoroutineRunner _coroutineRunner;
        INativeUtils _nativeUtils;
        IAttrStorage _storage;
        IAppEvents _appEvents;
        CrossPromotionIconConfiguration _iconConfig;
        IEnumerator _trackBannerClickEventTimeoutCoroutine;
        int _remainingAssetsToDownload = -1;
        List<WWW> _currentDownloads = new List<WWW>();
        List<IEnumerator> _currentDownloadsCoroutines = new List<IEnumerator>();
        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        Texture2D _iconTexture;

        public CrossPromotionData Data
        {
            get;
            private set;
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

        public CrossPromotionManager(ICoroutineRunner coroutineRunner, INativeUtils nativeUtils)
        {
            DebugUtils.Assert(coroutineRunner != null);
            DebugUtils.Assert(nativeUtils != null);
            _coroutineRunner = coroutineRunner;
            _nativeUtils = nativeUtils;
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
            Data = null;
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

        public void Dispose()
        {
            Reset();
            if(_appEvents != null)
            {
                DisconnectAppEvents(_appEvents);
            }
        }

        public void Start()
        {
            if(Data == null)
            {
                return;
            }

            DebugUtils.Assert(TrackSystemEvent != null);
            DebugUtils.Assert(TrackUrgentSystemEvent != null);
            DebugUtils.Assert(AppEvents != null);
            DebugUtils.Assert(CreateIcon != null);
            DebugUtils.Assert(CreatePopup != null);

            _startTime = TimeUtils.Timestamp;
            _canOpenPopup = Data.ShowPopup && Data.BannerInfo.Count > 0 && CanOpenPopupByFreq();

            DownloadAllAssets();
        }

        public void Init(AttrDic config)
        {
            Data = new CrossPromotionData(config);
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
                canOpenPopup = _startTime - lastOpenedTs >= Data.PopupFrequency;
            }
            return canOpenPopup;
        }

        bool CanOpenPopupByTimeout()
        {
            return TimeUtils.Timestamp - _startTime < Data.PopupTimeout;
        }

        void DownloadAllAssets()
        {
            _assetsReadyCalled = false;
            var assetsToDownload = new HashSet<string>();
            assetsToDownload.Add(Data.IconImage);
            assetsToDownload.Add(Data.PopupTitleImage);
             
            var itr = Data.BannerInfo.GetEnumerator();
            while(itr.MoveNext())
            {
                var keyValue = itr.Current;
                CrossPromotionBannerData banner = keyValue.Value;
                assetsToDownload.Add(banner.ButtonTextImage);
                assetsToDownload.Add(banner.BgImage);
                assetsToDownload.Add(banner.IconImage);
            }
            itr.Dispose();

            _remainingAssetsToDownload = assetsToDownload.Count;

            var itrHashSet = assetsToDownload.GetEnumerator();
            while(itrHashSet.MoveNext())
            {
                var url = itrHashSet.Current;
                LoadAssetFromCacheOrDownload(url, OnAssetDownloaded);
            }
            itrHashSet.Dispose();
        }

        bool AreAssetsReady()
        {
            if(_remainingAssetsToDownload != 0)
            {
                return false;
            }

            bool allReady = true;
            allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(Data.IconImage)));
            allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(Data.PopupTitleImage)));

            var itr = Data.BannerInfo.GetEnumerator();
            while(itr.MoveNext())
            {
                var keyValue = itr.Current;
                CrossPromotionBannerData banner = keyValue.Value;
                allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(banner.ButtonTextImage)));
                allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(banner.BgImage)));
                allReady &= FileUtils.ExistsFile(FileUtils.Combine(_assetsPath, Path.GetFileName(banner.IconImage)));
            }
            itr.Dispose();

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
                var www = new WWW(url);
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
            for(int i = 0, DataAppsToCheckCount = Data.AppsToCheck.Count; i < DataAppsToCheckCount; i++)
            {
                var app = Data.AppsToCheck[i];
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
            var sb = new StringBuilder();
            string appsToCheck = CheckedApps();
            string[] appsToCheckArray = appsToCheck.Split(',');
            for(int i = 0, appsToCheckArrayLength = appsToCheckArray.Length; i < appsToCheckArrayLength; i++)
            {
                var app = appsToCheckArray[i];
                if(_nativeUtils.IsInstalled(app))
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
            if(Data.ShowIcon)
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
            var data = new AttrDic();
            data.SetValue("xpromoid", Data.Id);
            data.SetValue("init_ts", _loginTime);
            
            if(setAutoOpen)
            {
                data.SetValue("automatically", _isAutoOpened);
            }
            
            if(setIconId)
            {
                data.SetValue("id", Data.IconId);
            }
            
            return data;
        }

        void AddBannerListData(AttrDic data)
        {
            var bannerList = new AttrList();
            int position = 0;
            var itr = Data.BannerInfo.GetEnumerator();
            while(itr.MoveNext())
            {
                var keyValue = itr.Current;
                CrossPromotionBannerData banner = keyValue.Value;
                var bannerData = new AttrDic();
                bannerData.SetValue("id", banner.Uid);
                bannerData.SetValue("xpromo_game", banner.Game);
                bannerData.SetValue("installed", _nativeUtils.IsInstalled(banner.AppId));
                bannerData.SetValue("position", position);
                bannerList.Add(bannerData);
                ++position;
            }
            itr.Dispose();
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
            CrossPromotionBannerData banner = Data.BannerInfo[uid];
            data.SetValue("id", uid);
            data.SetValue("position", position);
            data.SetValue("xpromo_game", banner.Game);
            data.SetValue("installed", _nativeUtils.IsInstalled(banner.AppId));
            TrackSystemEvent("cross.banner_impressed", data);
        }

        public void SendBannerClickedEvent(int uid, int position, bool urgent, bool currentGame, Action endCallback)
        {
            var data = GetEventBasicInformation(false, true);
            CrossPromotionBannerData banner = Data.BannerInfo[uid];
            data.SetValue("id", uid);
            data.SetValue("position", position);
            data.SetValue("xpromo_game", banner.Game);
            data.SetValue("installed", _nativeUtils.IsInstalled(banner.AppId));
            data.SetValue("urgent", urgent);

            if(urgent)
            {
                _trackBannerClickEventTimeoutCoroutine = _coroutineRunner.StartCoroutine(TrackBannerClickEventTimeoutCoroutine(uid, position, endCallback));

                TrackUrgentSystemEvent("cross.banner_clicked", data, error => {
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
            yield return new WaitForSeconds(Data.TrackTimeout);
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
            var data = new AttrDic();
            data.SetValue("error", kCrossFailByAssetFailedErrorCode);
            var assetsFailed = new AttrList();
            for(int i = 0, _assetsFailedCount = _assetsFailed.Count; i < _assetsFailedCount; i++)
            {
                var asset = _assetsFailed[i];
                var assetData = new AttrDic();
                assetData.SetValue("src", asset);
                assetsFailed.Add(assetData);
            }
            data.Set("assets", assetsFailed);
            TrackSystemEvent("cross.failed", data);
        }

        void SendCrossFailPopupTimeoutEvent()
        {
            var data = new AttrDic();
            data.SetValue("error", kCrossFailByPopupTimeOutErrorCode);
            TrackSystemEvent("cross.failed", data);
        }

        void OpenApp(int uid)
        {
            CrossPromotionBannerData banner = Data.BannerInfo[uid];
            if(_nativeUtils.IsInstalled(banner.AppId))
            {
                _nativeUtils.OpenApp(banner.AppId);
            }
            else
            {
                _nativeUtils.OpenStore(banner.StoreId);
            }
        }

        Texture2D GetTexture2DForImage(string url)
        {
            string filePath = FileUtils.Combine(_assetsPath, Path.GetFileName(url));
            byte[] data = FileUtils.ReadAllBytes(filePath);
            if(data != null)
            {
                var texture = new Texture2D(0, 0);
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
                _iconTexture = GetTexture2DForImage(Data.IconImage);
            }
            return _iconTexture;
        }

        public CrossPromotionIconConfiguration GetIconConfiguration()
        {
            if(_iconConfig == null)
            {
                string filePath = FileUtils.Combine(_assetsPath, Path.GetFileName(Data.IconImage));
                AssetBundle assetBundle = AssetBundle.LoadFromFile(filePath);
                if(assetBundle != null)
                {
                    var obj = UnityEngine.Object.Instantiate(assetBundle.mainAsset) as GameObject;
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
            var itr = _textures.GetEnumerator();
            while(itr.MoveNext())
            {
                var keyValue = itr.Current;
                if(keyValue.Value != null)
                {
                    keyValue.Value.Destroy();
                }
            }
            itr.Dispose();
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
            return (Data != null && Data.ShowIcon && AreAssetsReady());
        }

        public bool IsIconAnImage()
        {
            return Path.GetExtension(Data.IconImage) != kAssetBundleExtension;
        }
    }
}
