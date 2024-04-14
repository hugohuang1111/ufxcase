Shader "UFXCase/WindPlane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Light("Light", Vector) = (1, 0, 0, 0)
        _NoiseFactor("voronoi noise", Vector) = (0, 1, 0, 0)
        _FBMNoise("fbm noise", Vector) = (0, 1, 0, 0)
        _Angle("Angle", Vector) = (0, 0, 0, 0)
        _Speed("Speed", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Blend srcAlpha oneMinusSrcAlpha
        ZWrite off
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
            float4 _NoiseFactor;
            float4 _Angle;
            float4 _Speed;

            float3 getOffset(float2 uv)
            {
                float2 noiseUV = float2(uv.x, pow(1-uv.y, 2)) * float2(3, 1.2);
                noiseUV.y += _Time.y * _Speed.y;
                noiseUV.y += _NoiseFactor.w;
                float noise = voronoiNoise(noiseUV);
                float3 n3 = noise * _NoiseFactor;
                // n3.y += FBMvalueNoise(uv*float2(4, 4)) * 0.1;

                n3.y *= uv.y + abs(uv.y - pow(uv.y, 4));

                return n3;
            }

            float inverseLerp(float a, float b, float v)
            {
                return saturate((v - a)/(b - a));
            }

            float3 getOffsetByAngle(float3 angle, float2 uv)
            {
                float offsetY = log(uv.y*5)*uv.y*5 * angle.y * 0.2;
                float offsetX = uv.y * angle.x;
                float offsetZ = -abs(sin(uv.y)*angle.y*0.5);

                return float3(offsetX, offsetY, offsetZ);
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
                vertex.xyz += getOffsetByAngle(_Angle, v.uv);

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

            float smoothEdge(float2 uv)
            {
                float x = abs((uv.x - 0.5) * 2);
                return -smoothstep(0.7f, 1, x);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 normal = normalize(i.normal);
                float3 light = normalize(_Light);

                // col.xyz = normal * 0.5 + 0.5;
                // col.x = 1;
                // col.y = 1;
                // col.z = 1;
                col.a = dot(light, normal)*0.8;
                col.a *= (1 - pow(i.uv.y, 2));
                col.a *= inverseLerp(-0.1f, 1, i.offset.y) * _Speed.w;
                col.a += smoothEdge(i.uv);
                col.a = saturate(col.a);

                return col;
            }
            ENDCG
        }
    }
}
