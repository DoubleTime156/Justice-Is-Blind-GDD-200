Shader "Custom/FogPainterSmooth"
{
    Properties
    {
        _MainTex        ("Fog Memory (RT)", 2D) = "black" {}
        _WorldMin       ("World Min",       Vector) = (0,0,0,0)
        _WorldSize      ("World Size",      Vector) = (1,1,0,0)

        _PositionWorld  ("Center (world)",  Vector) = (0,0,0,0)
        _RadiusWorld    ("Radius (world)",  Float)  = 3.0
        _EdgeWorld      ("Edge (world)",    Float)  = 0.6

        _Intensity      ("Write Intensity", Range(0,1)) = 0.3   // 1 = white, 0.3 = memory
        _WriteMode      ("0=LERP 1=MAX",    Float)  = 1.0       // walking uses MAX
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite Off
        ZTest  Always
        Blend  Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f      { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float4 _WorldMin, _WorldSize;

            float4 _PositionWorld;
            float  _RadiusWorld, _EdgeWorld;
            float  _Intensity;
            float  _WriteMode;

            v2f vert(appdata v){ v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            // world->uv helper used with blit screen-space uv
            float2 WorldToUV(float2 w) { return saturate((w - _WorldMin.xy) / _WorldSize.xy); }

            fixed4 frag(v2f i) : SV_Target
            {
                // read current fog value at this pixel
                float current = tex2D(_MainTex, i.uv).r;

                // compute this pixel's world position by inverting the screen uv into world via _WorldMin/Size
                // (we're blitting a full-screen quad whose uv == fog uv already)
                float2 fogUV = i.uv;

                // distance in UV space between pixel and center (convert center/size)
                float2 centerUV = WorldToUV(_PositionWorld.xy);
                float worldToUV = 1.0 / min(_WorldSize.x, _WorldSize.y);
                float rUV = _RadiusWorld * worldToUV;
                float eUV = max(1e-6f, _EdgeWorld) * worldToUV;

                float d     = distance(fogUV, centerUV);
                float inside = 1.0 - smoothstep(rUV - eUV, rUV + eUV, d); // 1 inside, 0 outside

                float written = current;
                if (_WriteMode < 0.5)
                {
                    written = lerp(current, _Intensity, inside);
                }
                else
                {
                    float t = max(current, _Intensity);
                    written = lerp(current, t, step(0.0001, inside));
                }

                return fixed4(written, written, written, 1);
            }
            ENDCG
        }
    }
}
