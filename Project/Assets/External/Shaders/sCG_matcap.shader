
Shader "socialPointCG/sCG_matcap" 
{
	Properties
	{
		[Header(BASE SURFACE)]
		_MainTex ("DiffuseMap (ch1)", 2D) = "white" {}
		[Normal] [TextureToggle(USE_NORMALMAP)] [NoScaleOffset] _BumpMap("NormalMap (ch1)", 2D) = "bump" {}
		[NoScaleOffset] _BaseMatCap ("Base MatCap (RGBA)", 2D) = "white" {}
        _GlobalMatCapGlossiness("Smoothness (global MatCap only)", Range(0.0, 1.0)) = 0.5
		_Color("Tint color", Color) = (1,1,1,1)
		_Overbright("Overbright", Range(0, 2)) = 1.0

		[Space(30)]
		[Header(ADDITIVE LIGHTING)]
		_AddAmount ("Additive amount", Range(0, 2)) = 1.0
		[NoScaleOffset] _AdditiveMatCap ("Additive MatCap (RGB)", 2D) = "black" {}

		// These values have to be set via script (globally or locally) from somewhere else.
		// Also, remember toggling the keywords USE_GLOBAL_ADDITIVE_MATCAP, USE_STRETCH, USE_SQUASH, USE_ANGULAR_STRETCH
		//[NoScaleOffset] _GlobalAdditiveMatCapRough ("Rough Global Additive MatCap (RGB)", 2D) = "black" {}
		//[NoScaleOffset] _GlobalAdditiveMatCapSmooth ("Smooth Global Additive MatCap (RGB)", 2D) = "black" {}
		//_GlobalAdditiveMatCapColor("Global Additive MatCap Color", Color) = (1,1,1,1)
		//_GlobalAdditiveMatCapMultiplier ("Global Additive MatCap Multiplier", Range(0, 2)) = 1.0
		//_GlobalAdditiveEnvironmentMultiplier ("Global Additive Environment Multiplier", Range(0, 2)) = 1.0
		//_GlobalOverbrightMultiplier ("Global Overbright Multiplier", Range(0, 2)) = 1.0
		[HideInInspector] [PerRendererData] _StretchAmount ("Stretch Amount", Float) = 1.0		
		[HideInInspector] [PerRendererData] _StretchExponent ("Stretch Exponent", Range(1.01, 2.0) ) = 1.1
		[HideInInspector] [PerRendererData] _SquashAmount ("Squash Amount", Float) = 0.2
		[HideInInspector] [PerRendererData] _MaxSquash01 ("Maximum Squash Normalized", Range(0.0,1.0)) = 0.4
		[HideInInspector] [PerRendererData] _AngularStretch ("Angular Stretch", Float) = 1.0
		[HideInInspector] [PerRendererData] _AngularStretchExponent ("Angular Stretch Exponent", Range(1.0, 8.0)) = 2.0
		[HideInInspector] [PerRendererData] _VelocityDirObjectSpace ("Velocity Direction", Vector) = (0, 0, 1, 0)
		[HideInInspector] [PerRendererData] _VelocityAmountObjectSpace ("Velocity Amount", Float) = 0.0
		[HideInInspector] [PerRendererData] _AngularVelocityDirObjectSpace ("Angular Velocity Direction", Vector) = (0, 0, 1, 0)
		[HideInInspector] [PerRendererData] _AngularVelocityAmountObjectSpace ("Angular Velocity Amount", Float) = 0.0
		
		[Space(30)]
		[Header(OTHER SETTINGS)]
		[Toggle(USE_FOG)] _UseFog("Use Fog", Float) = 1
		[NoScaleOffset] _MK ("MaskMap (ch1) DOES NOTHING CURRENTLY", 2D) = "white" {}

		// -- Common Social Point material render state settings -- //		
		[Space(30)]
		[Header(Render State Settings)]	
		[Toggle(ADVANCED_RENDER_STATE)] _AdvancedRenderState("Show advanced options", Float) = 0.0

		// Blend mode values
		[KeywordEnum(Solid, Transparent, Additive, Screen, Multiply, Transparent Double Sided, Glass)] _RenderStateBasicTypes("Blend mode", Float) = 0
		[HideInInspector] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend source", Float) = 1.0
		[HideInInspector] [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend destination", Float) = 0.0
		[Toggle(ALPHA_MULTIPLIES_RGB)] _AlphaMultipliesRGB("Use alpha as RGB factor", Float) = 0
		[Toggle(ALPHA_PREMULTIPLY)] _AlphaPremultiply("Alpha premultiply", Float) = 0

		[Enum(Background,1000, Geometry,2000, AlphaTest,2450, Transparent,3000, Overlay,4000)] _RenderQueueEnum("Render queue", Int) = 2000
		_RenderQueueOffset("Render queue offset", Int) = 0
		
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull mode", Int) = 2

		[Enum(UnityEngine.Rendering.CompareFunction)] _DepthTest("Depth test (default LessEqual)", Int) = 4
		[Toggle] _DepthWrite("Depth write", Float) = 1
	}

	CustomEditor "SPCustomMaterialEditor"
	
	Subshader
	{
		// INI TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------
		Tags{
			"LightMode" = "ForwardBase"			// Used in Forward rendering, ambient, main directional light and vertex/SH lights are applied.
			"ShadowSupport" = "True"
			"IgnoreProjector" = "True"
		}
		
		Blend [_SrcBlend] [_DstBlend]
		
		Fog {Mode Global} // MODE: Off | Global | Linear | Exp | Exp
		
		Cull [_CullMode]
		ZTest [_DepthTest]
		ZWrite[_DepthWrite]

		// END TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------
		

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog

				#pragma multi_compile ___ USE_NORMALMAP
				#pragma multi_compile ___ USE_GLOBAL_ADDITIVE_MATCAP				

				#pragma fragmentoption ARB_precision_hint_fastest
				
				#pragma shader_feature USE_LM
				#pragma shader_feature USE_EMISSIVE
				#pragma shader_feature USE_FOG
				#pragma shader_feature USE_VC
				#pragma shader_feature ALPHA_MULTIPLIES_RGB
				#pragma shader_feature ALPHA_PREMULTIPLY				

				#pragma multi_compile ___ USE_STRETCH
				#pragma multi_compile ___ USE_SQUASH
				#pragma multi_compile ___ USE_ANGULAR_STRETCH

				#include "UnityCG.cginc"
				#include "sCG_inc_stretch.cginc"


				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 uv 	: TEXCOORD0;
					
					#if USE_NORMALMAP
						half3 viewXTangentSpace : TEXCOORD1;
						half3 viewYTangentSpace : TEXCOORD2;
					#else
						half2 uvMatcap : TEXCOORD1;
					#endif

					#if USE_FOG
						UNITY_FOG_COORDS(3)
					#endif
half3 worldNormal : NORMAL;
				};
				
				uniform float4 _MainTex_ST;
								
				v2f vert (appdata_tan v)
				{
					v2f o;

					float3 stretchedVertex = ApplyStretchObjectSpace(v.vertex);
					o.pos = UnityObjectToClipPos (stretchedVertex);

					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					
					// If this is not done, then there are issues with skinning.
					v.normal = normalize(v.normal);
					v.tangent = normalize(v.tangent);

					float3 viewXTangentSpace = normalize(UNITY_MATRIX_IT_MV[0].xyz);
					float3 viewYTangentSpace = normalize(UNITY_MATRIX_IT_MV[1].xyz);

					#if USE_NORMALMAP
						TANGENT_SPACE_ROTATION;
						o.viewXTangentSpace = mul(rotation, viewXTangentSpace);
						o.viewYTangentSpace = mul(rotation, viewYTangentSpace);
					#else
						half2 capCoord;
						capCoord.x = dot(viewXTangentSpace, v.normal);
						capCoord.y = dot(viewYTangentSpace, v.normal);
						o.uvMatcap = capCoord * 0.5 + 0.5;
					#endif

					#if USE_FOG
						UNITY_TRANSFER_FOG(o, o.pos);
					#endif

o.worldNormal = UnityObjectToWorldNormal(v.normal);

					return o;
				}
				
				uniform sampler2D _MainTex;
				#if USE_NORMALMAP
					uniform sampler2D _BumpMap;
				#endif
				uniform sampler2D _BaseMatCap;				
				uniform sampler2D _AdditiveMatCap;
				#if USE_GLOBAL_ADDITIVE_MATCAP
					uniform fixed _GlobalMatCapGlossiness;
					uniform sampler2D _GlobalAdditiveMatCapRough;
					uniform sampler2D _GlobalAdditiveMatCapSmooth;
					uniform fixed4 _GlobalAdditiveMatCapColor;
					uniform fixed _GlobalAdditiveMatCapMultiplier;
					uniform fixed _GlobalAdditiveEnvironmentMultiplier;
					uniform fixed _GlobalOverbrightMultiplier;					
				#endif
				uniform sampler2D _MK;
				uniform fixed _AddAmount;
				uniform fixed4 _Color;
				uniform fixed _Overbright;
				
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 MAIN_TEX = tex2D(_MainTex, i.uv);
					fixed4 MK = tex2D(_MK, i.uv);
					
					half2 uvMatcap;
					#if USE_NORMALMAP
						half3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
						half2 capCoord = half2(dot(i.viewXTangentSpace, normal), dot(i.viewYTangentSpace, normal));
						uvMatcap = capCoord * 0.5 + 0.5;
					#else
						uvMatcap = i.uvMatcap;
					#endif

					fixed4 BASE_MC = tex2D(_BaseMatCap, uvMatcap);
					fixed4 ADD_MC = tex2D(_AdditiveMatCap, uvMatcap);

					fixed4 GLOBAL_ADD_MC = fixed4(0,0,0,0);
					fixed3 sh = fixed3(0,0,0);
					fixed overbright = _Overbright;
					#if USE_GLOBAL_ADDITIVE_MATCAP
						overbright *= _GlobalOverbrightMultiplier;
sh = ShadeSH9(float4(i.worldNormal, 1));
sh *= _GlobalAdditiveEnvironmentMultiplier;
						GLOBAL_ADD_MC = tex2D(_GlobalAdditiveMatCapRough, uvMatcap);
						fixed4 GLOBAL_ADD_MC_SMOOTH = tex2D(_GlobalAdditiveMatCapSmooth, uvMatcap);
						GLOBAL_ADD_MC = lerp(GLOBAL_ADD_MC, GLOBAL_ADD_MC_SMOOTH, _GlobalMatCapGlossiness);
						GLOBAL_ADD_MC *= _GlobalAdditiveMatCapColor;
						GLOBAL_ADD_MC *= _GlobalAdditiveMatCapMultiplier;
					#endif

					fixed3 baseRGB = MAIN_TEX.rgb * BASE_MC.rgb * _Color.rgb * overbright;
					fixed baseAlpha = _Color.a * MAIN_TEX.a * BASE_MC.a;

					fixed reflectivity = (MK.r * _AddAmount);
					fixed3 addMCContrib = (reflectivity * ADD_MC.rgb) + GLOBAL_ADD_MC.rgb;
					
					fixed3 environmentContrib = sh * baseRGB;
//fixed3 environmentContrib = sh;

					fixed outputAlpha = baseAlpha;
					#if ALPHA_PREMULTIPLY
						fixed addMCLum = dot(addMCContrib, fixed3(0.3, 0.59, 0.11));
						fixed oneMinusReflectivity = 1 - addMCLum;
						outputAlpha += addMCLum;
						baseRGB *= baseAlpha;
					#endif

					fixed4 Complete = fixed4(baseRGB + addMCContrib + environmentContrib, baseAlpha);

					#if ALPHA_MULTIPLIES_RGB
						Complete.rgb *= Complete.a;
					#endif

					#if USE_FOG
						UNITY_APPLY_FOG(i.fogCoord, Complete);
					#endif

					return Complete;
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}