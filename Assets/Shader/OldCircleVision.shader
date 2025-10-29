Shader "Unlit/2DCircleVision"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Position ("Position", Vector) = (0, 0, 0, 0)
        _Radius ("Radius", Float) = 0.05
        _EdgeSoftness ("Edge Softness", Float) = 0.02
        _FogColor ("Fog Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            float _EdgeSoftness;
            fixed4 _FogColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
{
    float dist = distance(i.uv, _Position.xy);

    // 0 = fully clear near player, 1 = full fog far away
    float edge = smoothstep(_Radius - _EdgeSoftness, _Radius, dist);

    // Fog fades in from transparent to _FogColor
    fixed4 fogged = _FogColor;
    fogged.a = edge;

    return fogged;
}

            ENDCG
        }
    }
}
