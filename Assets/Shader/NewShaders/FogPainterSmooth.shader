Shader "Custom/FogPainterSmooth"
{
    Properties
    {
        _MainTex ("Base (Fog Memory)", 2D) = "black" {}
        _Position ("Position (UV)", Vector) = (0,0,0,0)
        _Radius ("Radius (UV)", Float) = 0.05
        _Intensity ("Write Intensity", Range(0,1)) = 1.0   // 1=white, 0.3=gray memory
        _Edge ("Edge Softness", Range(0,0.1)) = 0.02
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
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float4 _Position;
            float _Radius;
            float _Intensity;
            float _Edge;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // current fog value in [0..1]
                float current = tex2D(_MainTex, i.uv).r;

                // soft circular mask: 1 at center, 0 outside
                float dist = distance(i.uv, _Position.xy);
                float mask = smoothstep(_Radius, _Radius - _Edge, dist);

                // write TOWARD target so we can brighten (to 1) or fade down (to 0.3)
                float written = lerp(current, _Intensity, mask);

                return fixed4(written, written, written, 1);
            }
            ENDCG
        }
    }
}
