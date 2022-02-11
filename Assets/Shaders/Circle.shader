Shader "UFXCase/Circle"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,0,1)
        _Ring("R1,W1,R2,W2", Vector) = (0.3, 0.05, 0.5, 0.1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass {
            CGPROGRAM

            #pragma vertex vert alpha
            #pragma fragment frag alpha

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex: SV_POSITION;
                float3 worldPos: TEXCOORD0;
                half3 worldNormal: TEXCOORD1;
            };

            v2f vert (appdata_base v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            float4 _Color;
            half4 _Ring;

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float dotVal = dot(viewDir, normalize(i.worldNormal));
                dotVal = 1 - dotVal;
                float4 color = _Color;
                color.a = 0;

                float r1 = abs(dotVal - _Ring.x);
                float r2 = abs(dotVal - _Ring.z - frac(_Time.y));

                // r1 = step(r1, _Ring.y);
                // r2 = step(r2, _Ring.w);
                r1 = smoothstep(_Ring.y / 2, 0, r1);
                r2 = smoothstep(_Ring.w / 2, 0, r2);
                color.a = saturate(r1 + r2);

                return color;
            }

            ENDCG
        }

    }
    FallBack "Diffuse"
}
