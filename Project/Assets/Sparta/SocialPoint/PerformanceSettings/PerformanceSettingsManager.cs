using UnityEngine;
using System;
using SocialPoint.Attributes;
using SocialPoint.Login;

namespace SocialPoint.PerformanceSettings
{
    public class PerformanceSettingsManager : IDisposable
    {
        ILogin _login;

        PerformanceSettingsData _data;

        IAttrStorage _storage;

        const string kscreenRatio = "screen_ratio";
        const string kPerformancesettings = "performance_settings";

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
          
            if(dic.ContainsKey(kPerformancesettings))
            {
                Init(dic.Get(kPerformancesettings).AsDic);
            }
        }

        public virtual void Init(AttrDic config)
        {
            _data = new PerformanceSettingsData(config);

            ApplyPerformanceSettings();	
        }

        void ApplyPerformanceSettings()
        {
            if(Application.targetFrameRate != _data.FrameRate)
            {
                Application.targetFrameRate = _data.FrameRate;
            }
            
            if(Time.fixedDeltaTime != _data.FixedTimestep)
            {
                Time.fixedDeltaTime = _data.FixedTimestep;
            }

            if(_data.ScreenRatio != 1)
            {
                _storage.Save(kscreenRatio, new AttrFloat(_data.ScreenRatio));
            }
            else
            {
                if(_storage.Has(kscreenRatio))
                {
                    _storage.Remove(kscreenRatio);
                }
            }

            if(Shader.globalMaximumLOD != _data.MaxShaderLod)
            {
                Shader.globalMaximumLOD = _data.MaxShaderLod;
            }

            QualitySettings.antiAliasing = _data.AntiAliasing ? 1 : 0;

            if(QualitySettings.asyncUploadBufferSize != _data.AsyncUploadBufferSize)
            {
                QualitySettings.asyncUploadBufferSize = _data.AsyncUploadBufferSize;
            }

            if(QualitySettings.asyncUploadTimeSlice != _data.AsyncUploadTimeSlice)
            {
                QualitySettings.asyncUploadTimeSlice = _data.AsyncUploadTimeSlice;
            }

            if(QualitySettings.blendWeights != (BlendWeights)_data.BlendWeights)
            {
                QualitySettings.blendWeights = (BlendWeights)_data.BlendWeights;
            }

            if(QualitySettings.lodBias != _data.LodBias)
            {
                QualitySettings.lodBias = _data.LodBias;
            }

            if(QualitySettings.masterTextureLimit != _data.MasterTextureLimit)
            {
                QualitySettings.masterTextureLimit = _data.MasterTextureLimit;
            }

            if(QualitySettings.maximumLODLevel != _data.MaxLodLevel)
            {
                QualitySettings.maximumLODLevel = _data.MaxLodLevel;
            }

            QualitySettings.vSyncCount = _data.Vsync ? 1 : 0;
        }

        public void Dispose()
        {
            Reset();
        }

        void Reset()
        {
            _data = null;
            _login = null;
        }
    }

      
}
