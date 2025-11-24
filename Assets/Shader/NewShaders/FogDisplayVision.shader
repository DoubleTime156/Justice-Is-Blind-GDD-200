Shader "Custom/FogDisplayVision"
{
    Properties
    {
        _Darkness ("Darkness Color", Color) = (0,0,0,1)
        _FogTex ("Fog Memory", 2D) = "white" {}
        _LiveMaskTex ("Live Mask", 2D) = "black" {}
        _WorldMin ("World Min", Vector) = (0,0,0,0)
        _WorldSize ("World Size", Vector) = (1,1,0,0)

        _MemoryColor ("Memory Color (RGB)", Color) = (0.7,0.8,1.0,1)
        _MemoryAlpha ("Memory Alpha", Range(0,1)) = 0.35

        _BurstCount ("Burst Count", Int) = 0
        _BurstPos ("Burst Pos", Vector) = (0,0,0,0)
        _BurstRad ("Burst Rad", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define MAX_BURSTS 32

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 worldPos : TEXCOORD0; float2 uv:TEXCOORD1; };

            float4 _Darkness;
            sampler2D _FogTex;
            sampler2D _LiveMaskTex;
            float4 _WorldMin, _WorldSize;

            int     _BurstCount;
            float4  _BurstPos[MAX_BURSTS];
            float4  _BurstRad[MAX_BURSTS];

            float4 _MemoryColor;
            float  _MemoryAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                o.uv = v.uv;
                return o;
            }

            float circleSoft(float2 p, float2 c, float r, float f)
            {
                float d = distance(p, c);
                float e = max(1e-6f, f);
                return saturate(1.0 - smoothstep(r - e, r + e, d));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 fogUV = saturate((i.worldPos - _WorldMin.xy) / _WorldSize.xy);

                float liveMask = tex2D(_LiveMaskTex, fogUV).r;

                int bcount = clamp(_BurstCount, 0, MAX_BURSTS);
                float seenBurst = 0.0;
                [unroll]
                for (int k = 0; k < bcount; ++k)
                {
                    float2 cW = _BurstPos[k].xy;
                    float  rW = _BurstRad[k].x;
                    float  fW = max(1e-6f, _BurstRad[k].y);
                    seenBurst = max(seenBurst, circleSoft(i.worldPos, cW, rW, fW));
                }

                float seenNow = max(liveMask, seenBurst);
                if (seenNow > 0.001) return fixed4(0,0,0,0);

                float memory = tex2D(_FogTex, fogUV).r;
                if (memory > 0.001) return fixed4(_MemoryColor.rgb, _MemoryAlpha);

                return fixed4(_Darkness.rgb, 1.0);
            }
            ENDCG
        }
    }
}
