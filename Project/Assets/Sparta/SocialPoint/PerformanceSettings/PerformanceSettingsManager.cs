using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Login;

namespace SocialPoint.PerformanceSettings
{
    public class PerformanceSettingsManager : IDisposable
    {
        public interface IExtraPerformanceSettingsApplier
        {
            void Apply(AttrDic extraSettings);
        }

        ILogin _login;

        Dictionary<string, PerformanceSettingsData> _data;

        IAttrStorage _storage;

        public IExtraPerformanceSettingsApplier ExtraApplier { private get; set; }

        const string kscreenRatio = "screen_ratio";
        const string kPerformancesettings = "performance_settings";
        const string kPerformancesettingsMulti = "performance_settings_multi";
        const string kDefaultSettings = "default";

        static bool _screenRatioApplied;

        public static bool ScreenRatioApplied
        {
            get
            {
                return _screenRatioApplied;
            }
            set
            {
                _screenRatioApplied = value;
            }
        }

        public PerformanceSettingsManager(ILogin login, IAttrStorage storage = null)
        {
            _login = login;

            _storage = storage;

            _data = new Dictionary<string, PerformanceSettingsData>();

            InitLoginServices();

            ApplyInitialPerformanceSettings();
        }

        void ApplyInitialPerformanceSettings()
        {
            if(_storage == null)
            {
                _storage = new PlayerPrefsAttrStorage();
            }

            if(_storage.Has(kscreenRatio))
            {
                if(!PerformanceSettingsManager.ScreenRatioApplied)
                {
                    float screemRatio = _storage.Load(kscreenRatio).AsValue.ToFloat();
                    Screen.SetResolution((int)(screemRatio * Screen.width), (int)(screemRatio * Screen.height), true);
                    PerformanceSettingsManager.ScreenRatioApplied = true;
                }
            }
        }

        public void InitLoginServices()
        {
            _login.NewGenericDataEvent -= ParsePerformanceSettings;

            _login.NewGenericDataEvent += ParsePerformanceSettings;
        }

        void ParsePerformanceSettings(Attr data)
        {
            AttrDic dic = data.AsDic;
          
            if(dic.ContainsKey(kPerformancesettingsMulti))
            {
                InitMulti(dic.Get(kPerformancesettingsMulti).AsDic);
            }
            else if(dic.ContainsKey(kPerformancesettings))
            {
                Init(dic.Get(kPerformancesettings).AsDic);
            }
        }

        void Init(AttrDic config)
        {
            _data.Clear();
            _data.Add(kDefaultSettings, new PerformanceSettingsData(config));

            ApplyPerformanceSettings(kDefaultSettings);	
        }

        void InitMulti(AttrDic config)
        {
            _data.Clear();
            var itr = config.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                _data[pair.Key] = new PerformanceSettingsData(pair.Value.AsDic);
            }
            itr.Dispose();

            if(_data.ContainsKey(kDefaultSettings))
            {
                ApplyPerformanceSettings(kDefaultSettings);
            }
        }

        public void ApplyPerformanceSettings(string settingsId)
        {
            if(_data.ContainsKey(settingsId) == false)
            {
                return;
            }

            var settings = _data[settingsId];

            if(Application.targetFrameRate != settings.FrameRate)
            {
                Application.targetFrameRate = settings.FrameRate;
            }
            
            if(Time.fixedDeltaTime != settings.FixedTimestep)
            {
                Time.fixedDeltaTime = settings.FixedTimestep;
            }

            if(settings.ScreenRatio != 1)
            {
                _storage.Save(kscreenRatio, new AttrFloat(settings.ScreenRatio));
            }
            else
            {
                if(_storage.Has(kscreenRatio))
                {
                    _storage.Remove(kscreenRatio);
                }
            }

            if(Shader.globalMaximumLOD != settings.MaxShaderLod)
            {
                Shader.globalMaximumLOD = settings.MaxShaderLod;
            }

            QualitySettings.antiAliasing = settings.AntiAliasing ? 1 : 0;

            if(QualitySettings.asyncUploadBufferSize != settings.AsyncUploadBufferSize)
            {
                QualitySettings.asyncUploadBufferSize = settings.AsyncUploadBufferSize;
            }

            if(QualitySettings.asyncUploadTimeSlice != settings.AsyncUploadTimeSlice)
            {
                QualitySettings.asyncUploadTimeSlice = settings.AsyncUploadTimeSlice;
            }

            if(QualitySettings.blendWeights != (BlendWeights)settings.BlendWeights)
            {
                QualitySettings.blendWeights = (BlendWeights)settings.BlendWeights;
            }

            if(QualitySettings.lodBias != settings.LodBias)
            {
                QualitySettings.lodBias = settings.LodBias;
            }

            if(QualitySettings.masterTextureLimit != settings.MasterTextureLimit)
            {
                QualitySettings.masterTextureLimit = settings.MasterTextureLimit;
            }

            if(QualitySettings.maximumLODLevel != settings.MaxLodLevel)
            {
                QualitySettings.maximumLODLevel = settings.MaxLodLevel;
            }

            if(QualitySettings.vSyncCount != settings.Vsync)
            {
                QualitySettings.vSyncCount = settings.Vsync;
            }

            if(ExtraApplier != null)
            {
                ExtraApplier.Apply(settings.Settings);
            }
        }

        public void Dispose()
        {
            Reset();
        }

        void Reset()
        {
            _data.Clear();
            _login.NewGenericDataEvent -= ParsePerformanceSettings;
            _login = null;
        }
    }
}
