Shader "Custom/FogDisplayVision"
{
    Properties
    {
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
        _Radius ("Vision Radius", Float) = 3
        _Falloff ("Edge Falloff", Float) = 0.5
        _Darkness ("Darkness Color", Color) = (0,0,0,1)
        _FogTex ("Fog Memory", 2D) = "white" {}
        _WorldMin ("World Min", Vector) = (0,0,0,0)
        _WorldSize ("World Size", Vector) = (1,1,0,0)
        _MemoryStrength ("Memory Brightness", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 worldPos : TEXCOORD0; };

            float4 _PlayerPos;
            float _Radius;
            float _Falloff;
            float4 _Darkness;
            sampler2D _FogTex;
            float4 _WorldMin;
            float4 _WorldSize;
            float _MemoryStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Live circle mask: 1 inside, 0 outside (soft edge)
                float dist = distance(i.worldPos, _PlayerPos.xy);
                float mask = smoothstep(_Radius - _Falloff, _Radius, dist);
                float seenNow = 1.0 - mask;

                // If inside live circle at all -> CLEAR overlay (alpha = 0), regardless of memory
                if (seenNow > 0.001)
                {
                    return fixed4(0,0,0,0);
                }

                // Otherwise, sample fog memory
                float2 fogUV = (i.worldPos - _WorldMin.xy) / _WorldSize.xy;
                fogUV = saturate(fogUV);
                float memory = tex2D(_FogTex, fogUV).r;

                // memory region (gray)
                if (memory > 0.001)
                {
                    float gray = lerp(0.15, 0.8, _MemoryStrength);
                    return fixed4(gray, gray, gray, 0.5);
                }

                // unseen darkness
                return fixed4(_Darkness.rgb, 1.0);
            }
            ENDCG
        }
    }
}
