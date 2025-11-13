Shader "Custom/FogAccumulateSmooth"
{
    Properties
    {
        _MainTex ("Base (Fog Memory)", 2D) = "black" {}
        _WorldMin ("World Min", Vector) = (0,0,0,0)
        _WorldSize("World Size", Vector) = (1,1,0,0)

        _PlayerPos ("Player Position (world)", Vector) = (0,0,0,0)
        _Radius    ("Vision Radius (world)", Float) = 3
        _Falloff   ("Live Falloff (world)", Float) = 0.75

        _BurstCount ("Burst Count", Int) = 0
        _BurstPos   ("Burst Pos", Vector) = (0,0,0,0)
        _BurstRad   ("Burst Rad", Vector) = (0,0,0,0)   // x=radius, y=falloff

        _QueuedCount ("Queued Count", Int) = 0
        _QueuedPos   ("Queued Pos", Vector) = (0,0,0,0)
        _QueuedRad   ("Queued Rad", Vector) = (0,0,0,0) // x=radius, z=intensity, w=mode(0=lerp,1=max)

        _MemWriteIntensity ("Memory Write Intensity", Range(0,1)) = 0.30
        _MemCoverageBiasWorld ("Coverage Bias (world)", Float) = 0.0
        _WriteLive   ("Write Live", Int) = 1
        _WriteBursts ("Write Bursts", Int) = 0
        _WriteQueued ("Write Queued", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define MAX_BURSTS 32
            #define MAX_QUEUED 64

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float2 uv:TEXCOORD0; float4 vertex:SV_POSITION; };

            sampler2D _MainTex;
            float4 _WorldMin, _WorldSize;

            float4 _PlayerPos;
            float  _Radius, _Falloff;

            int    _BurstCount;
            float4 _BurstPos[MAX_BURSTS];
            float4 _BurstRad[MAX_BURSTS];

            int    _QueuedCount;
            float4 _QueuedPos[MAX_QUEUED];
            float4 _QueuedRad[MAX_QUEUED];

            float  _MemWriteIntensity;
            float  _MemCoverageBiasWorld;
            int    _WriteLive, _WriteBursts, _WriteQueued;

            v2f vert(appdata v){ v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            float maskSoft(float2 p, float2 c, float r, float f)
            {
                float d = distance(p, c);
                float e = max(1e-6f, f);
                return saturate(1.0 - smoothstep(r - e, r + e, d));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float current = tex2D(_MainTex, i.uv).r;
                float2 pWorld = _WorldMin.xy + i.uv * _WorldSize.xy;

                float writeVal = current;

                if (_WriteLive > 0)
                {
                    float rW = max(0.0, _Radius + _MemCoverageBiasWorld);
                    float m  = maskSoft(pWorld, _PlayerPos.xy, rW, _Falloff);
                    writeVal = max(writeVal, m * _MemWriteIntensity);
                }

                if (_WriteBursts > 0)
                {
                    int n = clamp(_BurstCount, 0, MAX_BURSTS);
                    [unroll]
                    for (int k=0;k<n;++k)
                    {
                        float rB = max(0.0, _BurstRad[k].x + _MemCoverageBiasWorld);
                        float fB = max(1e-6f, _BurstRad[k].y);
                        float mB = maskSoft(pWorld, _BurstPos[k].xy, rB, fB);
                        writeVal = max(writeVal, mB * _MemWriteIntensity);
                    }
                }

                if (_WriteQueued > 0)
                {
                    int qn = clamp(_QueuedCount, 0, MAX_QUEUED);
                    [unroll]
                    for (int m=0;m<qn;++m)
                    {
                        float rQ = max(0.0, _QueuedRad[m].x + _MemCoverageBiasWorld);
                        float inten = saturate(_QueuedRad[m].z);
                        float mode  = _QueuedRad[m].w;
                        float mQ = maskSoft(pWorld, _QueuedPos[m].xy, rQ, _Falloff);
                        float t  = mQ * inten;
                        writeVal = (mode > 0.5) ? max(writeVal, t) : lerp(writeVal, t, mQ);
                    }
                }

                writeVal = saturate(writeVal);
                return fixed4(writeVal, writeVal, writeVal, 1);
            }
            ENDCG
        }
    }
}
