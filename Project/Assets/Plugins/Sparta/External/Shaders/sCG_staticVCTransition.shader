// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "socialPointCG/sCG_staticVC_Transition" 
{
	Properties {		
		_MainTex ("DiffuseMap (ch1)", 2D) = "white" {}
		_BlendTex ("DiffuseMap 2 (ch1)", 2D) = "white" {}
		_Color(" Tint Color", Color) = (1,1,1,1)
		_Blend("Blend", Range(0.0, 1.0)) = 0.0
		[NoScaleOffset] _MK ("MaskMap (ch1)", 2D) = "black" {}
		_Overbright("Overbright", Float) = 1.0
		[Toggle(USE_EMISSIVE)] _UseEmissive("Use Emissive", Float) = 0
			[DependsOnToggle(USE_EMISSIVE)] _Emissive("    Emiss. Amount", Float) = 1.0
			[DependsOnToggle(USE_EMISSIVE)] _EmissiveColor("    Emiss. Color (MK.green)", Color) = (1,1,1,1)
		[Toggle(USE_FOG)] _UseFog("Use Fog", Float) = 1


		// -- Common Social Point material render state settings -- //		
		[Space]
		[Header(Render State Settings)]		
		[Toggle(ADVANCED_RENDER_STATE)] _AdvancedRenderState("Show advanced options", Float) = 0.0

		// Blend mode values
		[KeywordEnum(Solid, Transparent, Additive, Screen, Multiply, Transparent Double Sided)] _RenderStateBasicTypes("Blend mode", Float) = 0
		[HideInInspector] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend source", Float) = 1.0
		[HideInInspector] [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend destination", Float) = 0.0
		[Toggle(ALPHA_MULTIPLIES_RGB)] _AlphaMultipliesRGB("Use alpha as RGB factor", Float) = 0

		[Enum(Background,1000, Geometry,2000, AlphaTest,2450, Transparent,3000, Overlay,4000)] _RenderQueueEnum("Render queue", Int) = 2000
		_RenderQueueOffset("Render queue offset", Int) = 0
		
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull mode", Int) = 2

		[Enum(UnityEngine.Rendering.CompareFunction)] _DepthTest("Depth test (default LessEqual)", Int) = 4
		[Toggle] _DepthWrite("Depth write", Float) = 1
	}

	CustomEditor "SPCustomMaterialEditor"

	SubShader {
		
		// INI TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------
		Tags { 				
			"LightMode" = "ForwardBase"			// Used in Forward rendering, ambient, main directional light and vertex/SH lights are applied.
			
			"IgnoreProjector"="True" 
		}
		
		Blend [_SrcBlend] [_DstBlend]
		
		Fog {Mode Global} // MODE: Off | Global | Linear | Exp | Exp
		
		Cull [_CullMode]
		ZTest [_DepthTest]
		ZWrite[_DepthWrite]
		
		// END TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------
		
		Pass {
		
		CGPROGRAM
		#pragma vertex vert 
		#pragma fragment frag
		#pragma multi_compile_fog
		#pragma multi_compile_fwdbase
		#pragma skip_variants SPOT POINT_COOKIE DIRECTIONAL_COOKIE

		#pragma shader_feature USE_EMISSIVE
		#pragma shader_feature USE_FOG
		#pragma shader_feature ALPHA_MULTIPLIES_RGB

		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		//user defined variables
		uniform sampler2D 	_MainTex;
		uniform sampler2D 	_BlendTex;
		uniform sampler2D	_MK;
		uniform half		_Overbright;
		#if USE_EMISSIVE
			uniform fixed		_Emissive;
			uniform fixed4		_EmissiveColor;
		#endif
		uniform float4 _MainTex_ST;
		uniform fixed4 _Color;
		uniform half _Blend;

		//base input structs
		struct vertexInput {
			float4 vertex : POSITION;
			float2 uv0 : TEXCOORD0;
			half2 uv1 : TEXCOORD1;
			fixed4 color : COLOR;
		};

		struct vertexOutput {
			float4 pos : SV_POSITION;
			#if USE_FOG
				UNITY_FOG_COORDS(0)
			#endif
			LIGHTING_COORDS(1, 2)
			float2 uv0 : TEXCOORD3;
			half2 uv1 : TEXCOORD4;
			fixed4 VC : COLOR;
		};

		//vertex function
		vertexOutput vert(vertexInput v)
		{
			vertexOutput o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv0 = (v.uv0.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
			o.uv1 = v.uv1.xy;
			o.VC = v.color;

			#if USE_FOG
				UNITY_TRANSFER_FOG(o, o.pos);
			#endif

			TRANSFER_VERTEX_TO_FRAGMENT(o);
			return o;
		}

		//fragment function
		fixed4 frag(vertexOutput i) : COLOR
		{
			fixed4 DF0 = tex2D(_MainTex, i.uv0);
			fixed4 DF1 = tex2D(_BlendTex, i.uv0);
			fixed4 DF = lerp(DF0, DF1, _Blend) * _Color;
			fixed4 MK = tex2D(_MK, i.uv0);

			// ---- Shadowmap ----
			fixed atten = SHADOW_ATTENUATION(i);
			fixed3 shadow = lerp(UNITY_LIGHTMODEL_AMBIENT.rgb, fixed3(1.0, 1.0, 1.0), atten);

			fixed3 light = min(i.VC.rgb, shadow);
			
			fixed3 emissiveContrib = fixed3(0, 0, 0);
			#if USE_EMISSIVE
				emissiveContrib = (MK.g * _Emissive) * _EmissiveColor;
			#endif

			fixed4 Complete = fixed4(DF.rgb * light * _Overbright + emissiveContrib, DF.a);

			#if USE_FOG
				UNITY_APPLY_FOG(i.fogCoord, Complete);
			#endif

			#if ALPHA_MULTIPLIES_RGB
				Complete.rgb *= Complete.a;
			#endif

			return Complete;
		}

		ENDCG
      }
	}
	

	Fallback "VertexLit"
}

