// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

    Shader "Custom/SLaser" {
    Properties {
          _Color ("Main Color", Color) = (1, 1, 1, 1)
          _Dist ("Halving distance", Float) = 10.0
          _Intensity ("Intensity", Float) = 1.0
          _Origin ("Laser origin", Vector) = (0, 0, 0, 0)
          _Destination ("Laser destination", Vector) = (0, 0, 0, 0)
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
    uniform float4 _Destination;
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

    float dist_Point_to_Segment(float2 P, float2 P0, float2 P1)
	{
     	float2 v = P1 - P0;
     	float2 w = P - P0;
 
     	float c1 = dot(w,v);

     	if ( c1 <= 0 )
    	    return distance(P, P0);

    	float c2 = dot(v,v);
     	if ( c2 <= c1 )
     	    return distance(P.xy, P1.xy);

     	float b = c1 / c2;
     	float2 Pb = P0 + b * v;
     	return distance(P, Pb);
	}

    vertOutput vert(vertInput input) {
        vertOutput o;
        o.wpos = mul (unity_ObjectToWorld, input.pos).xyz;
        o.color = _Color;
        o.pos = UnityObjectToClipPos(input.pos.xyz);
        //o.color.a = 1.0;
        return o;
    }

    half4 frag(vertOutput output) : COLOR {
    	
    	float dist = dist_Point_to_Segment(output.wpos.xy, _Origin, _Destination);
    	float ndist = (dist*dist/(_Dist*_Dist));
    	if (dist < _Dist) return output.color * _Intensity * ((-0.8F)*ndist + 1.8F);
        else return output.color * _Intensity / (ndist); 
    }
    ENDCG
}
        }
        //FallBack "VertexLit"
    }
     
