Shader "Custom/FogPainter"
{
    Properties
    {
        _Position ("Position", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 0.1
        _FadeSpeed ("Fade Speed", Range(0,1)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // --- Pass 0: fade old vision ---
        Pass
        {
            Name "FadeMemory"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _FadeSpeed;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // Gradually fade brightness toward 0 (unseen)
                col.rgb = max(col.rgb - _FadeSpeed.xxx, 0);
                return col;
            }
            ENDCG
        }

        // --- Pass 1: reveal circle ---
        Pass
        {
            Name "RevealCircle"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha // normal alpha blend
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Position;
            float _Radius;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // compute distance from circle center
                float dist = distance(i.uv, _Position.xy);
                float edge = smoothstep(_Radius, _Radius * 0.7, dist);
                float visibility = 1.0 - edge;

                // sample existing fog
                fixed4 col = tex2D(_MainTex, i.uv);

                // brighten revealed area (so seen=1 inside circle)
                col.rgb = max(col.rgb, visibility.xxx);

                return col;
            }
            ENDCG
        }
    }
}
