Shader "Hidden/VignetteEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,1)
        _Intensity ("Intensity", Range(0,1)) = 0.5
        _InnerRadius ("Inner Radius", Range(0,0.8)) = 0.35
        _OuterRadius ("Outer Radius", Range(0.4,1)) = 0.75
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _Intensity;
            float _InnerRadius;
            float _OuterRadius;

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

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float2 centered = i.uv - 0.5;
                float dist = length(centered);

                // žádný efekt uvnitø vnitøního kruhu
                float ring = smoothstep(_InnerRadius, _OuterRadius, dist);

                float alpha = ring * _Intensity;

                return lerp(col, _Color, alpha);
            }
            ENDCG
        }
    }
    FallBack Off
}
