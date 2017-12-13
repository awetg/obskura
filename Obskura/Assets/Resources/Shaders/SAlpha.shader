// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

    Shader "Custom/SAlpha" {
        SubShader
        {
        Pass {
            
			Blend SrcAlpha OneMinusSrcAlpha 
    CGPROGRAM

    #pragma vertex vert             
    #pragma fragment frag

    struct vertInput {
        float4 pos : POSITION;
        float4 color : COLOR;
    };  

    struct vertOutput {
        float4 pos : SV_POSITION;
        float4 color : COLOR;
    };

    vertOutput vert(vertInput input) {
        vertOutput o;
        o.pos = UnityObjectToClipPos(input.pos);
        o.color = float4(0,0,0,0);
        return o;
    }

    half4 frag(vertOutput output) : COLOR {
        return float4(1,0,0,1); 
    }
    ENDCG
}
        }
        //FallBack "VertexLit"
    }
     
