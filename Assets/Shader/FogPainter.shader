Shader "Unlit/FogPainter"
{
    Properties
    {
        _MainTex ("Base", 2D) = "black" {}
        _Position ("Position", Vector) = (0, 0, 0, 0)
        _Radius ("Radius", Float) = 0.05
        _FogRevealIntensity ("Fog Reveal Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        Blend One OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Position;
            float _Radius;
            float _FogRevealIntensity;   // <— declared here properly

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Read current fog
                float currentFog = tex2D(_MainTex, i.uv).r;

                // Distance from this pixel to reveal center
                float2 pos = i.uv;
                float dist = distance(pos, _Position.xy);

                // Smooth circular reveal
                float circle = pow(saturate(1.0 - dist / _Radius), 2.0);

                // Blend circle with desired brightness intensity (1 = full white, 0.3 = gray)
                float intensity = clamp(_FogRevealIntensity, 0.0, 1.0);
                float newFog = max(currentFog, circle * intensity);

                return fixed4(newFog, newFog, newFog, 1);
            }
            ENDCG
        }
    }
}
