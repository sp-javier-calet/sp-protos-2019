using UnityEngine;
using System;
using DS.Common.Services;
using SocialPoint.Network;
using SocialPoint.Attributes;

namespace SocialPoint.PerformanceSettings
{
    public class PerformanceSettingsManager : IDisposable
    {
        private ILoginService _login;

        private PerformanceSettingsData _data;

        private IAttrStorage _storage = null;

        const string kscreenRatio = "screen_ratio";

        static bool _screenRatioApplied = false;

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

		public PerformanceSettingsManager(ILoginService login, IAttrStorage storage = null)
        {
            _login = login;

			_storage = storage;

            InitLoginServices();

            ApplyInitialPerformanceSettings();
        }

        void ApplyInitialPerformanceSettings()
        {
			if(_storage == null)
            	_storage = new PlayerPrefsAttrStorage();

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

        public virtual void InitLoginServices()
        {
            _login.NewGenericData -= ParsePerformanceSettings;

            _login.NewGenericData += ParsePerformanceSettings;
        }

        void ParsePerformanceSettings(Attr data)
        {
            AttrDic dic = data.AsDic;
          
            if(dic.ContainsKey("performance_settings"))
            {
                Init(dic.Get("performance_settings").AsDic);
            }
        }

        public virtual void Init(AttrDic config)
        {
            _data = new PerformanceSettingsData(config);

            ApplyPerformanceSettings();	
        }

        private void ApplyPerformanceSettings()
        {
            if(Application.targetFrameRate != _data.FrameRate)
                Application.targetFrameRate = _data.FrameRate;
            
            if(Time.fixedDeltaTime != _data.FixedTimestep)
                Time.fixedDeltaTime = _data.FixedTimestep;

            if(_data.ScreenRatio != 1)
            {
                _storage.Save(kscreenRatio, new AttrFloat(_data.ScreenRatio));
            }
            else
            {
                if(_storage.Has(kscreenRatio))
                    _storage.Remove(kscreenRatio);
            }

            if(Shader.globalMaximumLOD != _data.MaxShaderLod)
                Shader.globalMaximumLOD = _data.MaxShaderLod;

            QualitySettings.antiAliasing = _data.AntiAliasing ? 1 : 0;

            if(QualitySettings.asyncUploadBufferSize != _data.AsyncUploadBufferSize)
                QualitySettings.asyncUploadBufferSize = _data.AsyncUploadBufferSize;

            if(QualitySettings.asyncUploadTimeSlice != _data.AsyncUploadTimeSlice)
                QualitySettings.asyncUploadTimeSlice = _data.AsyncUploadTimeSlice;

            if(QualitySettings.blendWeights != (BlendWeights)_data.BlendWeights)
                QualitySettings.blendWeights = (BlendWeights)_data.BlendWeights;

            if(QualitySettings.lodBias != _data.LodBias)
                QualitySettings.lodBias = _data.LodBias;

            if(QualitySettings.masterTextureLimit != _data.MasterTextureLimit)
                QualitySettings.masterTextureLimit = _data.MasterTextureLimit;

            if(QualitySettings.maximumLODLevel != _data.MaxLodLevel)
                QualitySettings.maximumLODLevel = _data.MaxLodLevel;

            QualitySettings.vSyncCount = _data.Vsync ? 1 : 0;
        }

        public virtual  void Dispose()
        {
            Reset();
        }

        private void Reset()
        {
            _data = null;
            _login = null;
        }
    }

      
}
