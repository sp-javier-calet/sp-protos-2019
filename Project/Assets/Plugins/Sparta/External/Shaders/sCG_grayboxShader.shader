Shader "socialPointCG/sCG_grayboxShader" 
{
	Properties
	{
		_Color("Global tint", Color) = (1,1,1,1)
		[BasicColorsButtons] _MaskColor("VC alpha masked color", Color) = (1,1,1,1)
		_Overbright("Overbright", Float) = 1.0

		[HideInInspector]__BASICCOLORSBUTTONDRAWER_PALETTEINDEX__("INTERNAL PARAMETER", Float) = 0.0
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

