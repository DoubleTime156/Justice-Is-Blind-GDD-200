Shader "Custom/EnemiesShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)
        _Cutoff  ("Mask Cutoff", Range(0,1)) = 0.01
        _Edge    ("Mask Edge Softness", Range(0.0001,0.02)) = 0.005
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };
            struct v2f {
                float4 pos       : SV_POSITION;
                float2 uv        : TEXCOORD0;
                fixed4 color     : COLOR;
                float2 worldPos  : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            sampler2D _LiveMaskTex;
            float4 _WorldMin;
            float4 _WorldSize;

            float _Cutoff;
            float _Edge;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                float2 w = mul(unity_ObjectToWorld, v.vertex).xy;
                o.worldPos = w;
                return o;
            }

            float sampleMask(float2 worldXY)
            {
                float2 uv = saturate((worldXY - _WorldMin.xy) / _WorldSize.xy);
                return tex2D(_LiveMaskTex, uv).r;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;

                float m = sampleMask(i.worldPos);
                float a = smoothstep(_Cutoff, _Cutoff + _Edge, m);

                c.a *= a;
                c.rgb *= a;

                if (c.a <= 0.001) discard;
                return c;
            }
            ENDCG
        }
    }
}
