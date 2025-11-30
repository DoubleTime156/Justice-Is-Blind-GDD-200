Shader "Custom/FogDisplayVision"
{
    Properties
    {
        _PlayerPos ("Player Position (world)", Vector) = (0,0,0,0)
        _Radius    ("Vision Radius (world)",  Float)   = 3
        _Falloff   ("Edge Falloff (world)",   Float)   = 0.75
        _Darkness  ("Darkness Color", Color)           = (0,0,0,1)

        _FogTex    ("Fog Memory", 2D) = "white" {}
        _WorldMin  ("World Min",  Vector) = (0,0,0,0)
        _WorldSize ("World Size", Vector) = (1,1,0,0)

<<<<<<< Updated upstream
        _WhiteVisionCutoff ("White->Clear Threshold", Range(0.5,1)) = 0.95

        _DesatAmount ("Desaturation (0..1)", Range(0,1)) = 1
        _DimFactor   ("Memory Brightness",   Range(0,1)) = 0.95
        _MemoryAlpha ("Memory Alpha",        Range(0,1)) = 1.0
=======
        _MemoryAlpha ("Memory Alpha", Range(0,1)) = 0.35
        _DesatStrength ("Desaturation Strength", Range(0,1)) = 1.0
>>>>>>> Stashed changes

        _BurstCount ("Burst Count", Int) = 0
        _BurstPos   ("Burst Pos",   Vector) = (0,0,0,0)
        _BurstRad   ("Burst Rad",   Vector) = (0,0,0,0)

        _UseSortingLayerTex ("Use Camera Sorting Layer Texture", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define MAX_BURSTS 32

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
<<<<<<< Updated upstream
            struct v2f { float4 vertex : SV_POSITION; float2 worldPos : TEXCOORD0; float4 screenPos : TEXCOORD1; };

            float4 _PlayerPos; float _Radius; float _Falloff; float4 _Darkness;
            sampler2D _FogTex; float4 _WorldMin, _WorldSize;
=======
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            float4 _Darkness;
            sampler2D _FogTex;
            sampler2D _LiveMaskTex;
            sampler2D _CameraOpaqueTexture;
            float4 _WorldMin, _WorldSize;
>>>>>>> Stashed changes

            // Scene captures
            sampler2D _CameraOpaqueTexture;           // URP Forward Renderer (opaque)
            sampler2D _CameraSortingLayerTexture;     // URP 2D Renderer (sprites)
            float     _UseSortingLayerTex;

<<<<<<< Updated upstream
            float  _WhiteVisionCutoff;
            int    _BurstCount; float4 _BurstPos[MAX_BURSTS]; float4 _BurstRad[MAX_BURSTS];

            float  _DesatAmount; float _DimFactor; float _MemoryAlpha;

            v2f vert(appdata v)
            {
                v2f o; 
                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.worldPos  = mul(unity_ObjectToWorld, v.vertex).xy;
=======
            float  _MemoryAlpha;
            float  _DesatStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                o.uv = v.uv;
>>>>>>> Stashed changes
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float circleSoft(float2 p, float2 c, float r, float f)
            {
                float d = distance(p, c);
                float e = max(1e-6f, f);
                return saturate(1.0 - smoothstep(r - e, r + e, d));
            }

            float3 SampleScene(float4 sp)
            {
                // Choose sorting-layer texture for URP 2D, otherwise opaque texture.
                float3 srt = tex2Dproj(_CameraSortingLayerTexture, sp).rgb;
                float3 opq = tex2Dproj(_CameraOpaqueTexture,       sp).rgb;
                return lerp(opq, srt, saturate(_UseSortingLayerTex));
            }

            fixed4 frag(v2f i) : SV_Target
            {
<<<<<<< Updated upstream
                float seenPlayer = circleSoft(i.worldPos, _PlayerPos.xy, _Radius, _Falloff);
=======
                float2 fogUV = saturate((i.worldPos - _WorldMin.xy) / _WorldSize.xy);
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
>>>>>>> Stashed changes

                float seenBurst = 0.0;
                int bcount = clamp(_BurstCount, 0, MAX_BURSTS);
                [unroll]
                for (int k = 0; k < bcount; ++k)
                {
                    float2 cW = _BurstPos[k].xy;
                    float  rW = _BurstRad[k].x;
                    float  fW = _BurstRad[k].y;
                    seenBurst = max(seenBurst, circleSoft(i.worldPos, cW, rW, fW));
                }

                float seenNow = max(seenPlayer, seenBurst);
                if (seenNow > 0.001) return fixed4(0,0,0,0);

                float2 fogUV = saturate((i.worldPos - _WorldMin.xy) / _WorldSize.xy);
                float memory = tex2D(_FogTex, fogUV).r;
<<<<<<< Updated upstream
=======
                if (memory > 0.001)
                {
                    float4 sceneCol = tex2D(_CameraOpaqueTexture, screenUV);
                    float gray = dot(sceneCol.rgb, float3(0.299, 0.587, 0.114));
                    float3 desat = lerp(sceneCol.rgb, gray.xxx, _DesatStrength);
                    return float4(desat, _MemoryAlpha);
                }
>>>>>>> Stashed changes

                if (memory >= _WhiteVisionCutoff) return fixed4(0,0,0,0);
                if (memory <= 0.0)                return fixed4(_Darkness.rgb, 1.0);

                float3 scene = SampleScene(i.screenPos);
                float  gray  = dot(scene, float3(0.2126, 0.7152, 0.0722));
                float3 desat = lerp(scene, gray.xxx, saturate(_DesatAmount));
                desat *= _DimFactor;

                return fixed4(desat, _MemoryAlpha);
            }
            ENDCG
        }
    }
}
