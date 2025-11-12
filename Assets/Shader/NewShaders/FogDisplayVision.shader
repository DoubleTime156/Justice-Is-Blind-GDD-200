Shader "Custom/FogDisplayVision"
{
    Properties
    {
        _PlayerPos ("Player Position (world)", Vector) = (0,0,0,0)
        _Radius    ("Vision Radius (world)", Float) = 3
        _Falloff   ("Edge Falloff (world)", Float) = 0.75
        _Darkness  ("Darkness Color", Color) = (0,0,0,1)

        _FogTex    ("Fog Memory", 2D) = "white" {}
        _WorldMin  ("World Min", Vector)  = (0,0,0,0)
        _WorldSize ("World Size", Vector) = (1,1,0,0)

        _WhiteVisionCutoff ("White->Clear Threshold", Range(0.5,1)) = 0.95
        _MemoryColor  ("Memory Color (RGB)", Color) = (0.7,0.8,1.0,1)
        _MemoryAlpha  ("Memory Alpha", Range(0,1)) = 0.35

        _BurstCount ("Burst Count", Int) = 0
        _BurstPos ("Burst Pos", Vector) = (0,0,0,0)
        _BurstRad ("Burst Rad", Vector) = (0,0,0,0)

        _MemInsetWorld ("Memory Inset (world)", Float) = 0.0
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
            struct v2f { float4 vertex : SV_POSITION; float2 worldPos : TEXCOORD0; };

            float4 _PlayerPos;
            float  _Radius;
            float  _Falloff;
            float4 _Darkness;

            sampler2D _FogTex;
            float4 _FogTex_TexelSize;
            float4 _WorldMin, _WorldSize;
            float  _WhiteVisionCutoff;

            int     _BurstCount;
            float4  _BurstPos[MAX_BURSTS];
            float4  _BurstRad[MAX_BURSTS];

            float4 _MemoryColor;
            float  _MemoryAlpha;
            float  _MemInsetWorld;

            v2f vert(appdata v)
            {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            float circleSoft(float2 p, float2 c, float r, float f)
            {
                float d = distance(p, c);
                float e = max(1e-6f, f);
                return saturate(1.0 - smoothstep(r - e, r + e, d));
            }

            float4 weightsCR(float t)
            {
                float t2=t*t, t3=t2*t;
                return float4(-0.5*t + t2 - 0.5*t3,
                               1.0 - 2.5*t2 + 1.5*t3,
                               0.5*t + 2.0*t2 - 1.5*t3,
                              -0.5*t2 + 0.5*t3);
            }

            float sampleFogBicubic(sampler2D tex, float2 uv, float2 texSize)
            {
                float2 xy = uv * texSize - 0.5;
                float2 i  = floor(xy);
                float2 f  = xy - i;

                float4 wx = weightsCR(f.x);
                float4 wy = weightsCR(f.y);

                float2 baseUV = (i + 0.5) / texSize;

                float s00 = tex2D(tex, baseUV + float2(-1,-1)/texSize).r;
                float s10 = tex2D(tex, baseUV + float2( 0,-1)/texSize).r;
                float s20 = tex2D(tex, baseUV + float2( 1,-1)/texSize).r;
                float s30 = tex2D(tex, baseUV + float2( 2,-1)/texSize).r;

                float s01 = tex2D(tex, baseUV + float2(-1, 0)/texSize).r;
                float s11 = tex2D(tex, baseUV + float2( 0, 0)/texSize).r;
                float s21 = tex2D(tex, baseUV + float2( 1, 0)/texSize).r;
                float s31 = tex2D(tex, baseUV + float2( 2, 0)/texSize).r;

                float s02 = tex2D(tex, baseUV + float2(-1, 1)/texSize).r;
                float s12 = tex2D(tex, baseUV + float2( 0, 1)/texSize).r;
                float s22 = tex2D(tex, baseUV + float2( 1, 1)/texSize).r;
                float s32 = tex2D(tex, baseUV + float2( 2, 1)/texSize).r;

                float s03 = tex2D(tex, baseUV + float2(-1, 2)/texSize).r;
                float s13 = tex2D(tex, baseUV + float2( 0, 2)/texSize).r;
                float s23 = tex2D(tex, baseUV + float2( 1, 2)/texSize).r;
                float s33 = tex2D(tex, baseUV + float2( 2, 2)/texSize).r;

                float4 row0 = float4(s00,s10,s20,s30);
                float4 row1 = float4(s01,s11,s21,s31);
                float4 row2 = float4(s02,s12,s22,s32);
                float4 row3 = float4(s03,s13,s23,s33);

                float v0 = dot(row0, wx);
                float v1 = dot(row1, wx);
                float v2 = dot(row2, wx);
                float v3 = dot(row3, wx);

                float4 col = float4(v0,v1,v2,v3);
                return saturate(dot(col, wy));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 worldXY = i.worldPos;
                float2 fogUV   = saturate((worldXY - _WorldMin.xy) / _WorldSize.xy);

                float seenPlayer = circleSoft(worldXY, _PlayerPos.xy, _Radius, _Falloff);

                int bcount = clamp(_BurstCount, 0, MAX_BURSTS);
                float seenBurst = 0.0;
                [unroll]
                for (int k = 0; k < bcount; ++k)
                {
                    float2 cW = _BurstPos[k].xy;
                    float  rW = _BurstRad[k].x;
                    float  fW = _BurstRad[k].y;
                    seenBurst = max(seenBurst, circleSoft(worldXY, cW, rW, fW));
                }

                float seenNow = max(seenPlayer, seenBurst);
                if (seenNow > 0.001) return fixed4(0,0,0,0);

                float2 texSize = 1.0 / _FogTex_TexelSize.xy;
                float memory = sampleFogBicubic(_FogTex, fogUV, texSize);

                if (memory >= _WhiteVisionCutoff) return fixed4(0,0,0,0);

                float d = distance(worldXY, _PlayerPos.xy);
                float memoryAllowed = step(_Radius + max(0.0, _MemInsetWorld), d);

                if (memoryAllowed > 0.5 && memory > 0.001)
                    return fixed4(_MemoryColor.rgb, _MemoryAlpha);

                return fixed4(_Darkness.rgb, 1.0);
            }
            ENDCG
        }
    }
}
