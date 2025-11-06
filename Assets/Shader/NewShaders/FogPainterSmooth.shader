Shader "Custom/FogPainterSmooth"
{
    Properties
    {
        _MainTex   ("Base (Fog Memory)", 2D) = "black" {}
        _Position  ("Position (UV)", Vector) = (0,0,0,0)
        _Radius    ("Radius (UV)", Float) = 0.05
        _Intensity ("Write Intensity", Range(0,1)) = 1.0   // 1=white, 0.3=gray memory
        _Edge      ("Edge Softness (UV)", Range(0,0.1)) = 0.02
        _WriteMode ("0=LERP, 1=MAX", Float) = 0
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
            struct v2f      { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float4 _Position;
            float  _Radius;
            float  _Intensity;
            float  _Edge;
            float  _WriteMode; 

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float current = tex2D(_MainTex, i.uv).r;

                float dist   = distance(i.uv, _Position.xy);
                float edge   = max(0.0, _Edge);
                float inside = 1.0 - smoothstep(_Radius - edge, _Radius, dist);

                float written = current;

                if (_WriteMode < 0.5)
                {
                    float target = _Intensity;
                    written = lerp(current, target, inside);
                }
                else
                {
                    float target = _Intensity;
                    float insideValue = max(current, target);
                    written = lerp(current, insideValue, step(0.0001, inside)); 
                }

                return fixed4(written, written, written, 1);
            }
            ENDCG
        }
    }
}
