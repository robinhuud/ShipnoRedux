﻿Shader "Hidden/ColorCycleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		colorMap("ColorMap", 2D) = "white" {}
		_CycleSpeed("Cycle Speed", Float) = 1.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex, colorMap;
			float _CycleSpeed;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 source = tex2D(_MainTex, i.uv);
                // just invert the colors
				fixed index = source.r + _Time[0] * _CycleSpeed;
				fixed4 col = tex2D(colorMap, fixed2(index, 0));
                return col;
            }
            ENDCG
        }
    }
}
