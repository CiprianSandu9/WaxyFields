Shader "Unlit/IanUnlit"
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
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float height : TEXCOORD1;
            };


            v2f vert (appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.height = v.vertex.z;


                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 _FirstColour;
            fixed4 _SecondColour;

            float _MinimumHeight;
            float _MaximumHeight;

            fixed4 frag (v2f i) : SV_Target
            {
                float minH = _MinimumHeight;
                float maxH = _MaximumHeight;
                float t = saturate((i.height - minH) / (maxH - minH)); // Clamp between 0 and 1

                float4 col = lerp(_FirstColour, _SecondColour, t);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // ---- Shadow Caster Pass ----
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }
}
