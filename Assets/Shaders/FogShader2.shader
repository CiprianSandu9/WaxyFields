Shader "Unlit/FogShader2"
{
    Properties
    {
        _FirstColour ("FirstColour", Color) = (.34, .85, .92, 1)
        _SecondColour ("SecondColour", Color) = (.34, .85, .92, 1)    
        _MinimumHeight("MinimumHeight", Float) = 0.0
        _MaximumHeight("MaximumHeight", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            float rand2(float2 n) {
                return frac(sin(dot(n, float2(12.9898, 78.233))) * 43758.5453);
            }

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

            fixed4 _FirstColour;
            fixed4 _SecondColour;

            float _MinimumHeight;
            float _MaximumHeight;


            v2f vert (appdata v)
            {
                v2f o;

                float wave = sin(_Time.y * 2 + v.vertex.y * 2) * .5;
                v.vertex.z += wave;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.height = v.vertex.z;

                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float minHeight = _MinimumHeight;
                float maxHeight = _MaximumHeight;
                float t = saturate((i.height - minHeight) / (maxHeight - minHeight)); // Clamp between 0 and 1

                fixed4 col = lerp(_FirstColour, _SecondColour, t);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

          
            ENDCG
        }
    }
}
