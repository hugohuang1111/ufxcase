Shader "UFXCase/WaterWave"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _AnimTime("Anim/Wait/Alph1/Alph2", Vector) = (2.93, 1.2, 1.266, 1.43)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 200
        Blend One OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _AnimTime;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        /**
         * 方形贝塞尔曲线, 一般常用于动画中, 根据时间变化, 某个值的变化
         * p1 是曲线起点, p4 是曲线的终点
         * p2 是p1的控制点, p3 是p4的控制点
         *
         *
         */
        float2 CubicBezierInterp(float2 p1, float2 p2, float2 p3, float2 p4, float t)
        {
            float2 a = lerp(p1, p2, t);
            float2 b = lerp(p2, p3, t);
            float2 c = lerp(p3, p4, t);

            float2 d = lerp(a, b, t);
            float2 e = lerp(b, c, t);

            return lerp(d, e, t);
        }

        float2 scaleUV(float2 texUV, float scale)
        {
            float2 uv = float2(texUV.x, texUV.y);
            float expand = 1/scale;
            uv *= expand;
            uv -= (expand/2);
            uv += 0.5;

            return uv;
        }

        // 去掉负数部分
        float makeBigOrEqual0(float v) {
            return v * saturate(sign(v));
        }

        float calcPeriodTime(float i)
        {
            float animTime = _AnimTime.x;
            float waitTime = _AnimTime.y;
            float t = _Time.y - waitTime * i;
            t = makeBigOrEqual0(t);
            t = t % (animTime + waitTime);
            t *= saturate(sign(animTime - t));
            // equal
            /*
            if (t > animTime) {
                t = 0;
            }
            */
            t /= animTime;

            return t;
        }

        float calcScale(float t)
        {
            return CubicBezierInterp(float2(0, 0.4), float2(0.33, 0.4), float2(0.4, 1), float2(1, 0.93), t).y;
        }

        float clacAlpha(float t)
        {
            float alpha1 = _AnimTime.z/_AnimTime.x;
            float alpha2 = _AnimTime.w/_AnimTime.x;

            float alpha = 0;
            if (t < alpha1) {
                t /= alpha1;
                alpha = CubicBezierInterp(float2(0, 0), float2(0.33, 0), float2(0.67, 1), float2(1, 1), t).y;
            } else if (t < alpha2) {
                t = 1;
                alpha = 1;
            } else {
                t = (t - alpha2)/(1-alpha2);
                alpha = CubicBezierInterp(float2(0, 0), float2(0.19, 0), float2(0.46, 1), float2(1, 1), 1 - t).y;
            }

            return alpha;
        }

        fixed4 blend(fixed4 src, fixed4 dst)
        {
            fixed4 clr = fixed4(1, 1, 1, 1);
            clr.rgb = max(src.rgb, dst.rgb);
            clr.a = max(src.a, dst.a);

            return clr;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float t01 = calcPeriodTime(0);
            float scale = calcScale(t01);
            float alpha = clacAlpha(t01);
            float2 uv = scaleUV(IN.uv_MainTex, scale);
            fixed4 c = tex2D (_MainTex, uv);
            c.a *= alpha;

            t01 = calcPeriodTime(1);
            scale = calcScale(t01);
            alpha = clacAlpha(t01);
            uv = scaleUV(IN.uv_MainTex, scale);
            fixed4 c1 = tex2D (_MainTex, uv);
            c1.a *= alpha;
            c = blend(c1, c);

            t01 = calcPeriodTime(2);
            scale = calcScale(t01);
            alpha = clacAlpha(t01);
            uv = scaleUV(IN.uv_MainTex, scale);
            fixed4 c2 = tex2D (_MainTex, uv);
            c2.a *= alpha;
            c = blend(c2, c);

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a; 
        }
        ENDCG
    }
    FallBack "Diffuse"
}
