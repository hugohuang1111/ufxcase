Shader "UFXCase/NoiseWithNormal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Light("Light", Vector) = (1, 0, 0, 0)
        _NoiseFactor("NoiseFactor", Vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM

            #include "./Lib/Noise.hlsl"

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 offset : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _Light;
            float3 _NoiseFactor;

            float3 getOffset(float2 uv)
            {
                float2 noiseUV = uv * float2(10, 10);
                // noiseUV += _Time.y;
                float noise = simpleNoise(noiseUV);

                return noise * _NoiseFactor;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float4 vertex = v.vertex;

                float UVStep = 0.01;
                float VertexStep = UVStep;

                float3 offset = getOffset(v.uv);
                float3 offsetX = getOffset(v.uv + float2(UVStep, 0));
                float3 offsetY = getOffset(v.uv + float2(0, UVStep));
                vertex.xyz += offset;

                float3 tagentX = offsetX - offset;
                tagentX.x = VertexStep;
                float3 tagentY = offsetY - offset;
                tagentY.z = VertexStep;

                float3 normal = cross(tagentX, tagentY);
                normal = normalize(normal);

                o.offset = offset;
                o.normal = normal;
                o.vertex = UnityObjectToClipPos(vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 normal = normalize(i.normal);
                float3 light = normalize(_Light);

                col.xyz = normal * 0.5 + 0.5;
                // col.y = 0;
                // col.z = 0;
                // col.x = dot(light, normal);
                // col.a = 1;

                return col;
            }
            ENDCG
        }
    }
}
