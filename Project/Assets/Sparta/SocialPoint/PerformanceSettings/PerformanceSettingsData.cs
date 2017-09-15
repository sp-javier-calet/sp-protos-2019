using SocialPoint.Attributes;

namespace SocialPoint.PerformanceSettings
{
    public sealed class PerformanceSettingsData
    {
        const string kFramerate = "frame_rate";
        const string kFixedtimestep = "fixed_timestep";
        const string kAssetquality = "asset_quality";
        const string kScreenratio = "screen_ratio";
        const string kCulling = "culling";
        const string kMaxshaderlod = "max_shader_lod";
        const string kAntialiasing = "anti_aliasing";
        const string kAsyncuploadbuffersize = "async_upload_buffer_size";
        const string kAsyncuploadtimeslice = "async_upload_time_slice";
        const string kBlendWeights = "blend_weights";
        const string kLodBias = "lod_bias";
        const string kMasterTextureLimit = "master_texture_limit";
        const string kMaxLodLevel = "max_lod_level";
        const string kVsync = "vsync";

        public AttrDic Settings { get; private set; }

        public int FrameRate { get { return Settings.GetValue(kFramerate).ToInt(); } }

        public float FixedTimestep { get { return Settings.GetValue(kFixedtimestep).ToFloat(); } }

        public string AssetQuality { get { return Settings.GetValue(kAssetquality).ToString(); } }

        public float ScreenRatio { get { return Settings.GetValue(kScreenratio).ToFloat(); } }

        public float Culling { get { return Settings.GetValue(kCulling).ToFloat(); } }

        public int MaxShaderLod { get { return Settings.GetValue(kMaxshaderlod).ToInt(); } }

        public bool AntiAliasing { get { return Settings.GetValue(kAntialiasing).ToBool(); } }

        public int AsyncUploadBufferSize { get { return Settings.GetValue(kAsyncuploadbuffersize).ToInt(); } }

        public int AsyncUploadTimeSlice { get { return Settings.GetValue(kAsyncuploadtimeslice).ToInt(); } }

        public int BlendWeights { get { return Settings.GetValue(kBlendWeights).ToInt(); } }

        public float LodBias { get { return Settings.GetValue(kLodBias).ToFloat(); } }

        public int MasterTextureLimit { get { return Settings.GetValue(kMasterTextureLimit).ToInt(); } }

        public int MaxLodLevel { get { return Settings.GetValue(kMaxLodLevel).ToInt(); } }

        public int Vsync { get { return Settings.GetValue(kVsync).ToInt(); } }


        public PerformanceSettingsData(AttrDic config)
        {
            Settings = new AttrDic(config);
        }
    }
}
