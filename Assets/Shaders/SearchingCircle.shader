Shader "UFXCase/SearchingCircle"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
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
                float2 uv: TEXCOORD0;
                float4 objPos: TEXCOORD1;
            };

            #define PI 3.1415926
            float4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata_base v) {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.objPos = v.vertex;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 color = tex2D (_MainTex, i.uv) * _Color;

                float r = length(i.objPos);
                r /= 5; // normalize

                // float t = (1 - _CosTime.w * sign(_SinTime.w)) / 2;
                float PI2 = PI / 2;
                // float t = 1 - sin(PI2 + frac(_Time.y / PI2)*PI2);
                float t = pow(frac(_Time.y), 2) * 1.4;
                float ring = abs(r - t);
                color.a = saturate(lerp(1, 0, ring * 4));

                t = pow(frac(_Time.y + 0.5), 2) * 1.4;
                ring = abs(r - t);
                color.a += saturate(lerp(1, 0, ring * 5));

                return color;
            }

            ENDCG
        }

    }
    FallBack "Diffuse"
}
