using SocialPoint.Attributes;

namespace SocialPoint.PerformanceSettings
{
    public class PerformanceSettingsData
    {
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
            FrameRate = config.GetValue("frame_rate").ToInt();
            FixedTimestep = config.GetValue("fixed_timestep").ToFloat();
            AssetQuality = config.GetValue("asset_quality").ToString();
            ScreenRatio = config.GetValue("screen_ratio").ToFloat();
            Culling = config.GetValue("culling").ToFloat();
            MaxShaderLod = config.GetValue("max_shader_lod").ToInt();
            AntiAliasing = config.GetValue("anti_aliasing").ToBool();
            AsyncUploadBufferSize = config.GetValue("async_upload_buffer_size").ToInt();
            AsyncUploadTimeSlice = config.GetValue("async_upload_time_slice").ToInt();
            BlendWeights = config.GetValue("blend_weights").ToInt();
            LodBias = config.GetValue("lod_bias").ToFloat();
            MasterTextureLimit = config.GetValue("master_texture_limit").ToInt();
            MaxLodLevel = config.GetValue("max_lod_level").ToInt();
            Vsync = config.GetValue("vsync").ToBool();
        }
    }
}
