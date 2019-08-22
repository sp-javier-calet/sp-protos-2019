// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Unlit/Texture Shadows" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.01
    _Color ("Tint Color", Color) = (1,1,1,1)

    [HideInInspector] _BCSHTintColor ("BCSH Tint Color", Color) = (1,1,1,1)
}

SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" }
    Blend SrcAlpha OneMinusSrcAlpha
    LOD 100

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            #pragma shader_feature SHADER_MODULE_BCSH

            struct appdata_t {
                float4 vertex : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _Cutoff;
            float4 _Color;

            #ifdef SHADER_MODULE_BCSH
            #include "./../../../Plugins/Sparta/Assets/Rendering/CGIncludes/ShaderModuleBCSH.cginc"
		    #endif

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                #ifdef SHADER_MODULE_BCSH
                o.color.a *= _BCSHTintColor.a;
                #endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                col *= i.color;
                clip(col.a - _Cutoff);

                UNITY_APPLY_FOG(i.fogCoord, col);

                #ifdef SHADER_MODULE_BCSH
                col = ApplyModuleBCSH (col);
                #endif

                return col;
            }
        ENDCG
    }
}
Fallback "Mobile/VertexLit"
}
