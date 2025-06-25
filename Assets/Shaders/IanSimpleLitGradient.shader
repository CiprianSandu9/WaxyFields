Shader "Custom/IanSimpleLitGradient"
{
    Properties
    {
        _FirstColour ("First Colour", Color) = (.34, .85, .92, 1)
        _SecondColour ("Second Colour", Color) = (.93, .93, .36, 1)    
        _MinimumHeight ("Minimum Height", Float) = 0.0
        _MaximumHeight ("Maximum Height", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Cull Off // Optional: remove if you only want front faces

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
            };

            float4 _FirstColour;
            float4 _SecondColour;
            float _MinimumHeight;
            float _MaximumHeight;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Compute t for gradient based on world-space height (Y)
                float minH = _MinimumHeight;
                float maxH = _MaximumHeight;
                float t = saturate((IN.positionWS.y - maxH) / (maxH - minH));

                float4 color = lerp(_FirstColour, _SecondColour, t);

                // Lambert diffuse lighting
                Light mainLight = GetMainLight();
                float3 normal = normalize(IN.normalWS);
                float3 lightDir = normalize(mainLight.direction);

                float lambert = max(0, dot(normal, lightDir));
                float3 diffuse = color.rgb * mainLight.color.rgb * lambert;

                // Add simple ambient lighting for visibility in shadow
                float3 ambient = color.rgb * 0.2;

                return float4(diffuse + ambient, color.a);
            }

            ENDHLSL
        }
    }
}
