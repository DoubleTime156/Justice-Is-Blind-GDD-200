Shader "Custom/FogPainterSmooth"
{
    Properties
    {
        _MainTex        ("Base (Fog Memory)", 2D) = "black" {}
        _WorldMin       ("World Min", Vector) = (0,0,0,0)
        _WorldSize      ("World Size", Vector) = (1,1,0,0)
        _PositionWorld  ("Position (World)", Vector) = (0,0,0,0)
        _RadiusWorld    ("Radius (World)", Float) = 3
        _EdgeWorld      ("Edge (World)", Float) = 0.75
        _Intensity      ("Write Intensity", Range(0,1)) = 0.3
        _WriteMode      ("0=LERP, 1=MAX", Float) = 1
        _Position       ("Position (UV)", Vector) = (0,0,0,0)
        _Radius         ("Radius (UV)", Float) = 0
        _Edge           ("Edge (UV)", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend Off
        ZWrite Off
        ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float4 _WorldMin, _WorldSize;
            float4 _PositionWorld, _Position;
            float _RadiusWorld, _EdgeWorld, _Radius, _Edge, _Intensity, _WriteMode;

            v2f vert(appdata v){ v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            fixed4 frag(v2f i) : SV_Target
            {
                float current = tex2D(_MainTex, i.uv).r;

                float2 worldXY = _WorldMin.xy + i.uv * _WorldSize.xy;

                float useR = _RadiusWorld;
                float useE = _EdgeWorld;
                float2 cW  = _PositionWorld.xy;

                if (useR <= 0.0)
                {
                    cW = _WorldMin.xy + _Position.xy * _WorldSize.xy;
                    float s = min(_WorldSize.x, _WorldSize.y);
                    useR = _Radius * s;
                    useE = _Edge * s;
                }

                float d = distance(worldXY, cW);
                float e = max(1e-6f, useE);
                float inside = saturate(1.0 - smoothstep(useR - e, useR + e, d));

                float target = (_WriteMode < 0.5) ? _Intensity : max(current, _Intensity);
                float written = lerp(current, target, inside);

                return fixed4(written, written, written, 1);
            }
            ENDCG
        }
    }
}
