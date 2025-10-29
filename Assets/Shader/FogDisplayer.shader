Shader "Custom/FogDisplay"
{
    Properties
    {
        _MainTex ("Fog Texture", 2D) = "white" {}     // 👈 renamed for SpriteRenderer
        _Darkness ("Darkness Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;        // 👈 matches the property above
            fixed4 _Darkness;

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
                float2 uv = i.uv;
                uv.y = 1 - uv.y;
                fixed4 fogColor = tex2D(_MainTex, uv);
                return fogColor; // 👈 Show fog texture directly
            }
            ENDCG
        }
    }
}
