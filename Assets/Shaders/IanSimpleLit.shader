Shader "Custom/IanSimpleLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _LightIntensityMultiplier ("Light Intensity Multiplier", Range(0, 2)) = 1
        _AmbientStrength ("Fake Ambient Strength", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // Include URP's common functions (contains ComputeFogFactor)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv: TEXCOORD0;
                float4 normalOS : NORMAL; // Need normal for lighting calculations
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv: TEXCOORD0;
                half3 lightAmount : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float3 positionVS  : TEXCOORD4;
            };

            float4 _Color;
            float _LightIntensityMultiplier;
            float _AmbientStrength;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz); // Get world position
                OUT.positionVS = TransformWorldToView(OUT.positionWS);

                VertexNormalInputs positions = GetVertexNormalInputs(IN.positionOS, IN.normalOS); // Pass normalOS

                Light light = GetMainLight();

                OUT.lightAmount = LightingLambert(light.color, light.direction, positions.normalWS) * _LightIntensityMultiplier;

                return OUT;
            }

            void Applyfog(inout float4 color, float3 positionWS)
            {
                float4 inColor = color;
              
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                float viewZ = -TransformWorldToView(positionWS).z;
                float nearZ0ToFarZ = max(viewZ - _ProjectionParams.y, 0);
                float density = 1.0f - ComputeFogIntensity(ComputeFogFactorZ0ToFar(nearZ0ToFarZ));

                color = lerp(color, unity_FogColor,  density);

                #else
                color = color;
                #endif
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // Fake ambient: just a fraction of the color
                half3 fakeAmbient = _Color.rgb * _AmbientStrength;

                half3 totalLight = IN.lightAmount * _Color.rgb + fakeAmbient;

                // Calculate the fog factor based on the fragment's depth/distance.
                // ComputeFogFactor takes the clip-space Z (depth) for standard fog modes.
                // This factor will be between 0 (no fog) and 1 (full fog).
                // float fogFactor = ComputeFogFactor(-IN.positionVS.z);
                //
                // half3 finalColorRGB = MixFog(totalLight, fogFactor);
                half4 finalCol = half4(totalLight, 1);
                Applyfog(finalCol, IN.positionWS);

                return finalCol;
            }
            ENDHLSL
        }
    }
}
