Shader "Custom/SLightTexture" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_LightTex ("Base (RGB)", 2D) = "white" {}
		_Intensity ("Global intensity", Range (0, 10)) = 10
		_Overlay ("Overlay color", Color) = (1,1,1,1)
	}
	SubShader {
		Pass {
			Blend One Zero
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _LightTex;
			uniform float _Intensity;
			uniform float4 _Overlay;

			float4 frag(v2f_img i) : COLOR {
				float4 c = tex2D(_MainTex, i.uv);
				float4 d = tex2D(_LightTex, i.uv);

				float4 result = _Overlay + ((d)*_Intensity)*c;
				return result;
			}
			ENDCG
		}
	}
}