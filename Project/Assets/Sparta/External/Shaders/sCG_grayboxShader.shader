Shader "socialPointCG/sCG_grayboxShader" 
{
	Properties
	{
		_Color("Global tint", Color) = (1,1,1,1)
		[BasicColorsButtons] _MaskColor("VC alpha masked color", Color) = (1,1,1,1)
		_Overbright("Overbright", Float) = 1.0
	}
	
	SubShader
	{
		Tags{ 
			"RenderType" = "Opaque"
		}
		Fog{ Mode Global }

		CGPROGRAM
		#pragma surface surf Lambert
		
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		uniform half _Overbright;
		uniform fixed4 _Color;
		uniform fixed4 _MaskColor;
		
		struct Input 
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		sampler2D _MainTex;

		void surf(Input IN, inout SurfaceOutput o) 
		{
			fixed3 baseRGB = lerp(_MaskColor.rgb, IN.color.rgb, IN.color.a);

			o.Albedo = fixed4(baseRGB * _Color.rgb * _Overbright, 1.0);
		}
		ENDCG
	}
	Fallback "VertexLit"
}



































/*	Properties {
		_Color("Global tint", Color) = (1,1,1,1)		
		_MaskColor("Color for Vertex Color alpha=0", Color) = (1,1,1,1)
		//[KeywordEnum(Solid, Transparent, Additive, Screen, Multiply)] _BlendMode("Blend mode", Float) = 0
		_Overbright("Overbright", Float) = 1.0
		[Toggle(USE_FOG)] _UseFog("Use Fog", Float) = 1
	}

	SubShader {
		
		// INI TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------
		Tags { 				
			"LightMode" = "ForwardBase"			// Used in Forward rendering, ambient, main directional light and vertex/SH lights are applied.
			
			"IgnoreProjector"="True" 
		}
				
		Fog {Mode Global} // MODE: Off | Global | Linear | Exp | Exp
		
		// END TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------
		
		
		Pass {
		
		CGPROGRAM
		#pragma vertex vert 
		#pragma fragment frag
		#pragma multi_compile_fog
		#pragma multi_compile_fwdadd_fullshadows		

		#pragma shader_feature USE_FOG
		
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		//user defined variables
		uniform half _Overbright;
		uniform fixed4 _Color;
		uniform fixed4 _MaskColor;

		//base input structs
		struct vertexInput {
			float4 vertex : POSITION;
			float4 normal : NORMAL;
			fixed4 color : COLOR;
		};

		struct vertexOutput {
			float4 pos : SV_POSITION;
			fixed4 VC : COLOR0;
			fixed3 DFL : COLOR1;
			#if USE_FOG
				UNITY_FOG_COORDS(0)
			#endif
			LIGHTING_COORDS(1, 2)
		};

		//vertex function
		vertexOutput vert(vertexInput v)
		{
			vertexOutput o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.VC = v.color;
			#if USE_FOG
				UNITY_TRANSFER_FOG(o, o.pos);
			#endif

			TRANSFER_VERTEX_TO_FRAGMENT(o);

			half3 L = ObjSpaceLightDir(v.vertex);
			half3 N = v.normal.xyz;
			float NdotL = max(0.0, dot(N, L));			
			o.DFL = lerp(UNITY_LIGHTMODEL_AMBIENT.rgb, _LightColor0.rgb, NdotL);
TIENES QUE HACERLO CON UN SHADER DE LOS QUE SOLO TIENES QUE PONER EL MODELO DE ILUMINACION 
			return o;
		}

		//fragment function
		fixed4 frag(vertexOutput i) : COLOR
		{
			//shadowmap...
			fixed atten = LIGHT_ATTENUATION(i);
			fixed3 shadow = lerp(UNITY_LIGHTMODEL_AMBIENT.rgb, fixed3(1.0, 1.0, 1.0), atten);
			
			fixed3 baseRGB = lerp(_MaskColor.rgb, i.VC.rgb, i.VC.a);

			fixed4 Complete = fixed4(baseRGB * DFL * _Color.rgb * shadow.rgb * _Overbright, 1.0);

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

*/