Shader "Custom/FogPainterOverwrite"
{
    Properties
    {
        _MainTex ("Base", 2D) = "black" {}
        _Position ("Position (UV)", Vector) = (0,0,0,0)
        _Radius ("Radius (UV)", Float) = 0.05
        _Intensity ("Write Intensity", Float) = 1.0     
        _Edge ("Edge Softness", Float) = 0.02          
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
            float4 _Position;     // UV center (x,y)
            float _Radius;        // UV radius
            float _Intensity;     // target write value (1→white, 0.3→gray)
            float _Edge;          // feather width

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Current fog already in texture
                float current = tex2D(_MainTex, i.uv).r;

                // Circular mask in UV
                float d = distance(i.uv, _Position.xy);

                // Hard center with soft edge using smoothstep
                float m = step(d, _Radius);         // 1 if inside the circle, 0 if outside
                float written = lerp(current, _Intensity, m);

                return fixed4(written, written, written, 1);
            }
            ENDCG
        }
    }
}
