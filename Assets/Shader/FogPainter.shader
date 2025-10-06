Shader "Unlit/FogPainter"
{
    Properties
    {
        _MainTex ("Base (Fog Texture)", 2D) = "black" {}
        _Position ("Position (UV)", Vector) = (0, 0, 0, 0)
        _Radius ("Radius (UV)", Float) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend One OneMinusSrcAlpha

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float currentFog = tex2D(_MainTex, i.uv).r;
                float2 pos = i.uv;
                float dist = distance(pos, _Position.xy);

                float reveal = 1.0 - smoothstep(0.0, _Radius, dist);

                
                float newFog = max(currentFog, reveal);

                return fixed4(newFog, newFog, newFog, 1.0);
            }
            ENDCG
        }
    }
}
