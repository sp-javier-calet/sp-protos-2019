using SocialPoint.Attributes;

namespace SocialPoint.PerformanceSettings
{
    public class PerformanceSettingsData
    {
        const string kFramerate = "frame_rate";
        const string kFixedtimestep = "fixed_timestep";
        const string kAssetquality = "asset_quality";
        const string kScreenratio = "screen_ratio";
        const string kCulling = "culling";
        const string kMaxshaderlod = "max_shader_lod";
        const string kAntialiasing = "anti_aliasing";
        const string kAsyncuploadbuffersize = "async_upload_buffer_size";
        const string kBlendWeights = "blend_weights";
        const string kLodBias = "lod_bias";
        const string kMasterTextureLimit = "master_texture_limit";
        const string kMaxLodLevel = "max_lod_level";
        const string kVsync = "vsync";

        public int FrameRate { get; private set; }

        public float FixedTimestep { get; private set; }

        public string AssetQuality { get; private set; }

        public float ScreenRatio { get; private set; }

        public float Culling { get; private set; }

        public int MaxShaderLod { get; private set; }

        public bool AntiAliasing { get; private set; }

        public int AsyncUploadBufferSize { get; private set; }

        public int AsyncUploadTimeSlice { get; private set; }

        public int BlendWeights { get; private set; }

        public float LodBias { get; private set; }

        public int MasterTextureLimit { get; private set; }

        public int MaxLodLevel { get; private set; }

        public bool Vsync { get; private set; }

        public PerformanceSettingsData(AttrDic config)
        {
            FrameRate = config.GetValue(kFramerate).ToInt();
            FixedTimestep = config.GetValue(kFixedtimestep).ToFloat();
            AssetQuality = config.GetValue(kAssetquality).ToString();
            ScreenRatio = config.GetValue(kScreenratio).ToFloat();
            Culling = config.GetValue(kCulling).ToFloat();
            MaxShaderLod = config.GetValue(kMaxshaderlod).ToInt();
            AntiAliasing = config.GetValue(kAntialiasing).ToBool();
            AsyncUploadBufferSize = config.GetValue(kAsyncuploadbuffersize).ToInt();
            AsyncUploadTimeSlice = config.GetValue(kAsyncuploadbuffersize).ToInt();
            BlendWeights = config.GetValue(kBlendWeights).ToInt();
            LodBias = config.GetValue(kLodBias).ToFloat();
            MasterTextureLimit = config.GetValue(kMasterTextureLimit).ToInt();
            MaxLodLevel = config.GetValue(kMaxLodLevel).ToInt();
            Vsync = config.GetValue(kVsync).ToBool();
        }
    }
}
