Shader "UFXCase/CollisionCurve"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Curve("X(Radius)/Y(Width)/Z(StartDeg)/W(EndDeg)", Vector) = (0.5, 0.1, 0, 90)
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
            float4 _Curve;

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

                r = abs(r - _Curve.x);
                color.a = saturate(lerp(1, 0, smoothstep(0, _Curve.y/2, r)));

                float3 v1 = normalize(i.objPos);
                float3 v2 = float3(1, 0, 0);
                float dotVal = dot(v1, v2);
                color.a *= step(cos(radians(_Curve.w)), dotVal);
                color.a *= step(dotVal, cos(radians(_Curve.z)));

                return color;
            }

            ENDCG
        }

    }
    FallBack "Diffuse"
}
