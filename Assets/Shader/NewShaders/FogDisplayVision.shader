Shader "Custom/FogDisplayVision"
{
    Properties
    {
        _PlayerPos ("Player Position (world)", Vector) = (0,0,0,0)
        _Radius ("Vision Radius (world)", Float) = 3
        _Falloff ("Edge Falloff (world)", Float) = 0.5
        _Darkness ("Darkness Color", Color) = (0,0,0,1)

        _FogTex ("Fog Memory", 2D) = "white" {}
        _WorldMin ("World Min", Vector) = (0,0,0,0)
        _WorldSize ("World Size", Vector) = (1,1,0,0)

        _QuantizeCircle ("Quantize Live Circles To Fog Texels", Float) = 1
        _WhiteVisionCutoff ("White->Clear Threshold", Range(0.5,1)) = 0.95

        _MemoryColor ("Memory Color (RGB)", Color) = (0.2,0.8,0.3,0.5)
        _MemoryAlpha ("Memory Alpha", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define MAX_BURSTS 8

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 worldPos : TEXCOORD0; };

            float4 _PlayerPos;
            float _Radius;
            float _Falloff;
            float4 _Darkness;

            sampler2D _FogTex;
            float4 _FogTex_TexelSize;

            float4 _WorldMin;
            float4 _WorldSize;

            float _QuantizeCircle;
            float _WhiteVisionCutoff;

            int _BurstCount;
            float4 _BurstPos[MAX_BURSTS];
            float4 _BurstRad[MAX_BURSTS];

            float4 _MemoryColor;
            float _MemoryAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            float circleSeen(float2 fogUV, float2 centerWorld, float radiusW, float falloffW)
            {
                float2 centerUV = (centerWorld - _WorldMin.xy) / _WorldSize.xy;
                centerUV = saturate(centerUV);

                if (_QuantizeCircle > 0.5)
                {
                    float2 stepUV = _FogTex_TexelSize.xy;
                    fogUV    = floor(fogUV    / stepUV + 0.5) * stepUV;
                    centerUV = floor(centerUV / stepUV + 0.5) * stepUV;
                }

                float worldToUV = 1.0 / min(_WorldSize.x, _WorldSize.y);
                float rUV = radiusW  * worldToUV;
                float fUV = max(0.0001, falloffW) * worldToUV;

                float d = distance(fogUV, centerUV);
                float m = smoothstep(rUV - fUV, rUV, d);
                return 1.0 - m; // 1 inside, 0 outside
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 fogUV = (i.worldPos - _WorldMin.xy) / _WorldSize.xy;
                fogUV = saturate(fogUV);

                int count = clamp(_BurstCount, 1, MAX_BURSTS);
                float seenNow = 0.0;
                [unroll]
                for (int k = 0; k < count; ++k)
                {
                    float2 cW = _BurstPos[k].xy;
                    float  rW = _BurstRad[k].x;
                    float  fW = _BurstRad[k].y;
                    seenNow = max(seenNow, circleSeen(fogUV, cW, rW, fW));
                }
                if (seenNow > 0.001) return fixed4(0,0,0,0);

                float memory = tex2D(_FogTex, fogUV).r;
                if (memory >= _WhiteVisionCutoff) return fixed4(0,0,0,0);

                if (memory > 0.001)
                    return fixed4(_MemoryColor.rgb, _MemoryAlpha);

                return fixed4(_Darkness.rgb, 1.0);
            }
            ENDCG
        }
    }
}
