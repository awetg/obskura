// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

    Shader "Custom/SLight" {
    Properties {
          _Color ("Main Color", Color) = (1, 1, 1, 1)
          _Dist ("Dimming distance", Float) = 10.0
          _Intensity ("Intensity", Float) = 0.5
          _Origin ("Light origin", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
    	Pass {
            Blend One One
    CGPROGRAM

    #pragma vertex vert             
    #pragma fragment frag

    uniform float4 _Color;
    uniform float4 _Origin;
    uniform float _Dist;
    uniform float _Intensity;

    struct vertInput {
        float4 pos : POSITION;
        float4 color : COLOR;
    };  

    struct vertOutput {
        float4 pos : SV_POSITION;
        float4 color : COLOR;
        float3 wpos : TEXCOORD1;
    };

    vertOutput vert(vertInput input) {
        vertOutput o;
        o.wpos = mul (unity_ObjectToWorld, input.pos).xyz;
        o.color = _Color;
        o.pos = UnityObjectToClipPos(input.pos.xyz);
        //o.color.a = 1.0;
        return o;
    }

    half4 frag(vertOutput output) : COLOR {
    	float dist = distance(_Origin.xy, output.wpos.xy);
    	float ndist = (dist*dist*dist*dist*dist/(_Dist*_Dist*_Dist*_Dist*_Dist));
    	if (dist < _Dist) return output.color * _Intensity * ((-0.8F)*ndist + 1.8F);
    	//if (dist > _Dist * 2) return output.color * exp2(-5-dist);
        else return output.color * _Intensity / exp2(ndist-1); 
    }
    ENDCG
}
        }
        //FallBack "VertexLit"
    }
     
