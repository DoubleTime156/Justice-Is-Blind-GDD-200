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
        _MemoryStrength ("Memory Brightness", Range(0,1)) = 0.3

        _QuantizeCircle ("Quantize Live Circles To Fog Texels", Float) = 1
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
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Adjust this if you want more bursts (also update FogManager.maxBursts)
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
            float _MemoryStrength;
            float _QuantizeCircle;

            // Arrays of circles: slot 0 = player, 1..N-1 = bursts
            int _BurstCount;
            float4 _BurstPos[MAX_BURSTS]; // world positions
            float4 _BurstRad[MAX_BURSTS]; // x=radiusWorld, y=falloffWorld

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            float circleSeen(float2 fogUV, float2 centerWorld, float radiusW, float falloffW)
            {
                // Convert center from world to UV; quantize if requested
                float2 centerUV = (centerWorld - _WorldMin.xy) / _WorldSize.xy;
                centerUV = saturate(centerUV);

                if (_QuantizeCircle > 0.5)
                {
                    float2 stepUV = _FogTex_TexelSize.xy;
                    fogUV    = floor(fogUV    / stepUV + 0.5) * stepUV;
                    centerUV = floor(centerUV / stepUV + 0.5) * stepUV;
                }

                // Convert sizes from WORLD to UV
                float worldToUV = 1.0 / min(_WorldSize.x, _WorldSize.y);
                float rUV = radiusW  * worldToUV;
                float fUV = max(0.0001, falloffW) * worldToUV;

                float d = distance(fogUV, centerUV);
                // smoothstep(edgeStart, edgeEnd, x) with reversed slope for inside=1, outside=0
                float m = smoothstep(rUV - fUV, rUV, d);
                return 1.0 - m; // 1 inside, 0 outside
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Fog UV for sampling memory and for circle tests
                float2 fogUV = (i.worldPos - _WorldMin.xy) / _WorldSize.xy;
                fogUV = saturate(fogUV);

                // Are we inside ANY live clear circle?
                // Always include slot 0 (player circle)
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

                if (seenNow > 0.001)
                {
                    // Clear overlay (true vision) only where circle covers
                    return fixed4(0,0,0,0);
                }

                // Otherwise, read memory (unchanged)
                float memory = tex2D(_FogTex, fogUV).r;

                if (memory > 0.001)
                {
                    float gray = lerp(0.15, 0.8, _MemoryStrength);
                    return fixed4(gray, gray, gray, 0.5);
                }

                // Unseen darkness
                return fixed4(_Darkness.rgb, 1.0);
            }
            ENDCG
        }
    }
}
