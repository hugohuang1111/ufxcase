Shader "UFXCase/BRDF"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalTex ("Normal (RGB)", 2D) = "white" {}
        _MetallicTex ("Metallic (R)", 2D) = "white" {}
        _RoughnessTex ("Roughness (R)", 2D) = "white" {}
        _AOTex ("AO", 2D) = "white" {}
        _EmissiveTex ("Emissive", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass {
            Tags { "LightMode"="ForwardAdd" }

			Cull Back

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct v2f {
                float4 vertex: SV_POSITION;
                float2 uv: TEXCOORD0;
                float3 worldPos: TEXCOORD1;
                fixed3 worldNormal: TEXCOORD2;
            };

            float4 _Color;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NormalTex;
            float4 _NormalTex_ST;
            sampler2D _MetallicTex;
            float4 _MetallicTex_ST;
            sampler2D _RoughnessTex;
            float4 _RoughnessTex_ST;
            sampler2D _AOTex;
            float4 _AOTex_ST;
            sampler2D _EmissiveTex;
            float4 _EmissiveTex_ST;


            v2f vert (appdata_base v) {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            #define PI 3.14159265359f

            fixed3 getNormalFromMap(v2f i) {
                // https://www.bilibili.com/read/cv4583341/
                fixed3 tangentNormal = tex2D(_NormalTex, i.uv).xyz * 2.0 - 1.0;

                fixed3 Q1  = ddx(i.worldPos);
                fixed3 Q2  = ddy(i.worldPos);
                fixed2 st1 = ddx(i.uv);
                fixed2 st2 = ddy(i.uv);

                fixed3 N  = i.worldNormal;
                fixed3 T  = normalize(Q1*st2.y - Q2*st1.y);
                fixed3 B  = cross(N, T);
                fixed3x3 TBN = fixed3x3(T, B, N);

                return normalize(mul(TBN, tangentNormal));
            }

            float DistributionGGX(fixed3 N, fixed3 H, float roughness) {
                float a = roughness * roughness;
                float a2 = a*a;
                float NdotH = max(dot(N, H), 0.0);
                float NdotH2 = NdotH*NdotH;

                float nom   = a2;
                float denom = NdotH2 * (a2 - 1.0) + 1.0;
                denom = PI * denom * denom;

                return nom/denom;
            }

            float GeometrySchlickGGX(float NdotV, float roughness) {
                float r = (roughness + 1.0);
                float k = (r*r) / 8.0;

                float nom   = NdotV;
                float denom = NdotV * (1.0 - k) + k;

                return nom / denom;
            }

            float GeometrySmith(float3 N, float3 V, float3 L, float roughness) {
                float NdotV = max(dot(N, V), 0.0);
                float NdotL = max(dot(N, L), 0.0);
                float ggx2 = GeometrySchlickGGX(NdotV, roughness);
                float ggx1 = GeometrySchlickGGX(NdotL, roughness);

                return ggx1 * ggx2 * 1000;
            }
            float3 fresnelSchlick(float cosTheta, float3 F0) {
                return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
            }
            float3 SRGBToLinear(float3 rgb) {
                return pow(rgb, float3(2.2, 2.2, 2.2));
            }
            float3 LinearToSRGB(float3 rgb) {
                float f = 1/2.2;
                return pow(rgb, float3(f, f, f));
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 color = tex2D (_MainTex, i.uv);
                fixed3 albedo = SRGBToLinear(color.rgb) * _Color.rgb;
                color.a *= _Color.a;

                float metallic  = tex2D(_MetallicTex, i.uv).r;
                fixed roughness = tex2D(_RoughnessTex, i.uv).r;
                fixed ao        = tex2D(_AOTex, i.uv).r;
                fixed3 emissive = tex2D(_EmissiveTex, i.uv).rgb;

                fixed3 N = normalize(i.worldNormal); // getNormalFromMap(i);
                fixed3 V = normalize(_WorldSpaceCameraPos - i.worldPos);

                fixed3 F0 = fixed3(0.04, 0.04, 0.04);
                F0 = lerp(F0, albedo, 0.5);

                fixed3 Lo = fixed3(0, 0, 0);

                fixed3 L = _WorldSpaceLightPos0.xyz - i.worldPos;
                fixed Ldistance = length(L);
                fixed attenuation = 1.0 / (Ldistance * Ldistance);
                fixed3 radiance = _LightColor0 * attenuation;
                L = normalize(L);
                fixed3 H = normalize(V + L);
                fixed NdotL = saturate(dot(N, L));
                fixed NdotV = saturate(dot(N, V));

                fixed NDF = DistributionGGX(N, H, roughness);
                fixed G   = GeometrySmith(N, V, L, roughness);
                fixed3 F  = fresnelSchlick(saturate(dot(H, V)), F0);

                fixed3 numerator = NDF * G * F;
                float denominator = 4.0 * NdotV * NdotL + 0.0001;
                fixed3 specular = numerator / denominator;

                fixed3 kS = F;
                fixed3 kD = 1 - kS;
                kD *= (1.0 - metallic);

                Lo += (kD * albedo / PI + specular) * radiance * NdotL;

                fixed3 ambient = fixed3(0.03, 0.03, 0.03) * albedo * ao;

                color.rgb = ambient + Lo;

                // NDF = max(dot(N, H) * sign(NdotL), 0.0);
                // color.rgb = specular;
                // color.rgb = Lo;

                return color;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
