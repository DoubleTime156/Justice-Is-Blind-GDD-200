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

            //Declare Variables
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
                //pos of circle
                float dist = distance(i.worldPos, _PlayerPos.xy);
                // determines what number mask is
                float mask = smoothstep(_Radius - _Falloff, _Radius, dist);


                float2 fogUV = (i.worldPos - _WorldMin.xy) / _WorldSize.xy;
                float fog = tex2D(_FogTex, fogUV).r;


                // 1 = visible now, 0 = outside circle
                float seen = 1 - mask; 

                float3 baseColor;
                float alpha;

                // Visible right now no fog overlay
                if (seen > 0.01)
                {
                    baseColor = float3(0.0, 0.0, 0.0);
                    alpha = 0.0;
                }
                // Not visible but has been seen before gray fog memory
                else if (fog > 0.001)
                {
                    baseColor = float3(0.3, 0.3, 0.3);
                    alpha = 0.5;
                }
                // Completely unseen full darkness
                else
                {
                    baseColor = _Darkness.rgb;
                    alpha = 1.0;
                }

                return fixed4(baseColor, alpha);
            }

            ENDCG
        }
    }
}
