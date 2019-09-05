Shader "socialPointCG/sCG_FxGrayboxShaderAdditive" 
{
	Properties
	{
		_MainTex("Diffuse Map",2D) = ""{}
		[Header(Gradient Color)]
		[BasicColorsButtons] _GradientA("Color 1", Color) = (1,1,1,1)
		[BasicColorsButtons] _GradientB("Color 2", Color) = (1,1,1,1)
		_Overbright("Overbright", Float) = 1.0
						
		[HideInInspector]__BASICCOLORSBUTTONDRAWER_PALETTEINDEX__("INTERNAL PARAMETER", Float) = 0.0
	}
		
		SubShader{

		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

		ZWrite Off Cull Off
		Blend One One

			




		Pass{
		CGPROGRAM
#pragma vertex vert 
#pragma fragment frag 
#include "UnityCG.cginc" 


		
	uniform sampler2D _MainTex;
	uniform fixed4 _MainTex_ST, _GradientA, _GradientB;
	uniform half _Overbright;


	struct appdata {

		fixed4 color : COLOR;
		float4 vertex : POSITION;
		half2 uv0 : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		half2 uv0 : TEXCOORD0;
		fixed4 color : COLOR;
	};


	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
		o.color = v.color;
		return o;
	}


	fixed4 frag(v2f i) : SV_Target
	{
		

		fixed4 texCol = tex2D(_MainTex, i.uv0);
		
		fixed invertVCAlpha = (1.0 - i.color.a);
		fixed3 RGBCol = fixed3(i.color.r - invertVCAlpha, i.color.g - invertVCAlpha, i.color.b - invertVCAlpha);
		fixed Grey = saturate(texCol);
		fixed4 gradient = lerp(_GradientA, _GradientB, Grey);
		gradient *= Grey;
		return fixed4(gradient.rgb * RGBCol * _Overbright *i.color.a, texCol.a );
		
		;
	



	}

		ENDCG
	}

	}
}