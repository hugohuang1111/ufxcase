Shader "UFXCase/Circle"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,0,1)
        _Radius ("Radius", float) = 0.5
        _Width1 ("Width1", float) = 0.1
        _Width2 ("Width2", float) = 0.1
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
                float3 normal: NORMAL;
            };

            v2f vert (appdata_base v)
            {
                v2f o;

                o.normal = v.normal;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            float4 _Color;
            float _Radius;
            float _Width1;
            float _Width2;

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float dotVal = dot(viewDir, normalize(i.normal));

                float4 color = _Color;

                color.a = 0;
                float gap = 0.1;
                float r1 = _Radius + _Width1;
                float r2 = r1 + gap;
                float r3 = r2 + _Width2;
                if (dotVal > _Radius && dotVal < r1) {
                    color.a = 1;
                    color.b = 0;
                } else if (dotVal > r2 && dotVal < r3) {
                    color.a = 1;
                    color.g = 0;
                } else {
                    color.a = 0;
                }

                return color;
            }

            ENDCG
        }

    }
    FallBack "Diffuse"
}
