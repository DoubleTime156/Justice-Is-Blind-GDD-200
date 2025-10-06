Shader "Custom/2DCircleVision"
{
    Properties
    {
        // player pos
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
        //circle radius
        _Radius ("Radius", Float) = 3
        //transition between black and vision
        _Falloff ("Falloff", Float) = 0.5
        //color of no see
        _Darkness ("Darkness Color", Color) = (0,0,0,1)
        //What youve seen
        _FogTex ("Fog Memory", 2D) = "white" {}

        
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float4 _PlayerPos;
            float _Radius;
            float _Falloff;
            float4 _Darkness;
            sampler2D _FogTex;

            float4 _WorldMin;
            float4 _WorldSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy; 
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // getting where the player is
                float dist = distance(i.worldPos, _PlayerPos.xy);
                // calculates how big the circle will be
                float mask = smoothstep(_Radius - _Falloff, _Radius, dist);
                //Fog memory
                float2 fogUV = (i.worldPos - _WorldMin.xy) / _WorldSize.xy;

                float fog = tex2D(_FogTex, fogUV).r;
                //reduce darkness on see
                float explored = saturate(fog + (1 - mask));

                float seen = 1 - mask;
                //combine no see and have seen
                float visibility = max(seen, fog * 0.5);
                //edits _darkness material color to make it a gray fog
                fixed3 baseColor = lerp(_Darkness.rgb, float3(0.5, 0.5, 0.5), fog);

                return fixed4(baseColor, 1 - seen);
            }
            ENDCG
        }
    }
}
