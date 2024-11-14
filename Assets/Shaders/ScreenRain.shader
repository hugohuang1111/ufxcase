Shader "UFXCase/ScreenRain"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
    #include "./Lib/Hash.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

    float _RainAmount;

    #define S(a, b, t) smoothstep(a, b, t)

    float3 N13(float p) {
        float3 p3 = frac(float3(p, p, p) * float3(.1031, .11369, .13787));
        p3 += dot(p3, p3.yzx + 19.19);
        return frac(float3((p3.x + p3.y)*p3.z, (p3.x + p3.z)*p3.y, (p3.y + p3.z)*p3.x));
    }
    float4 N14(float t) {
        return frac(sin(t*float4(123., 1024., 1456., 264.))*float4(6547., 345., 8799., 1564.));
    }
    float N(float t) {
        return frac(sin(t*12345.564)*7658.76);
    }

    float Saw(float b, float t) {
        return S(0., b, t)*S(1., b, t);
    }

    float DynamicDrops(float2 uv, float t) {
        float2 UV = uv;

        // 让 UV 在y的方向动起来, 也就是向下移动
        uv.y += t*0.75;

        float2 a = float2(6., 1.);
        float2 grid = a * 2.;
        float2 id = floor(uv*grid);

        // 让列的格子不再对齐, 比如相邻两列的格子, 就不会再对齐, 不会齐头并进向下
        uv.y += N(id.x);

        // id 又重新计算了一次, 因为, 上面那一步把 uv 重新变化了
        id = floor(uv*grid);
        // 通过 id 计算得到一个3维随机值
        float3 n = N13(id.x*35.2 + id.y*2376.1);
        // st 就是每个小格子的大小了, 同时 x 范围在 [-0.5, 0.5]
        float2 st = frac(uv*grid) - float2(.5, 0);

        // n 是通过 id 产生的, 所以 x 是对应到每个格子的
        float x = n.x - .5; // [-0.5, 0.5]
        // y 是 UV, 是整体的, 不是对格子而言
        float y = UV.y*20.; // [0, 20]
        // wiggle 是让雨滴左右摆动, 现在还只有y有关, 所以每一格都是一样的
        float wiggle = sin(y + sin(y));
        // 这里 x 加的 wiggle, 已经乘上了 每一格的 x 与 n, 所以在每一格上, 摆动就不一样了
        x += wiggle*(.5 - abs(x))*(n.z - .5);
        x *= .7;
        // 把 t 加上了 noise, 并只要小数部分, 因为 noise 由 id 而来, 那 ti 就与 cell 有关了
        float ti = frac(t + n.z);

        // 只在 0.85 这个时间点, 是 1, 后面又去减 .5, 又去乘 .9 , 加 .5 的,
        y = (Saw(.85, ti) - .5)*.9 + .5; // [0.05, 1.05]

        // 这里 p 是每个格子中水滴的中心点, y 与 ti 有关, 所以水滴的位置在不断变化
        float2 p = float2(x, y);

        // cell中, 离 p 的距离, *a.yx 就是把所有的点都变成正圆形
        float d = length((st - p)*a.yx);

        float mainDrop = S(.4, .0, d);

        // y 方向上距中心的距离, 再开方 r
        float r = sqrt(S(1., y, st.y));
        // st.y 比 y 大的地方会是 1, y 方向的变化, 中心到1都是1, 这里相当于一个 y 方向
        float trailFront = S(-.02, .02, st.y - y);

        y = UV.y;
        y = frac(y*10.) + (st.y - .5);
        float dd = length(st - float2(x, y));
        float droplets = S(.3, 0., dd);
        float m = mainDrop + droplets * r * trailFront;

        return m;
    }

    /*
     * 静态雨滴, 将整个画面分成小格子, 每一格一个雨滴.
     * 每格雨滴的位置以格子的id, 生成, 相当于每一格的位置都是不一样的, 也就是随机了
     * 每格雨滴的大小用 S(.3, 0., d) 离中心点的距离, 同时乘上随机值, 那大小也不一样了
     * 每格雨滴乘上以时间为变量的 fade , 就可以形成淡入淡出的效果
     */
    float StaticDrops(float2 uv, float t) {
        uv *= 40.;

        float2 id = floor(uv);
        uv = frac(uv) - .5;
        float3 n = N13(id.x*107.45 + id.y*3543.654);
        float2 p = (n.xy - .5)*.7;
        float d = length(uv - p);

        // fade, 以时间有变量, 每一秒的 0.25 , 显示得最亮
        float fade = Saw(.025, frac(t + n.z));
        float c = S(.3, 0., d) * frac(n.z*10.) * fade;

        return c;
    }

    float Drops(float2 uv, float t, float l0, float l1, float l2) {
        float s = StaticDrops(uv, t)*l0;
        float m1 = DynamicDrops(uv, t)*l1;
        float m2 = DynamicDrops(uv*1.85, t)*l2;

        float c = s + m1 + m2;
        c = S(.3, 1., c);

        return c;
    }

    float4 Frag(VaryingsDefault i) : SV_Target {
        float2 aspectUV = ((i.texcoord * _ScreenParams.xy) - .5*_ScreenParams.xy) / _ScreenParams.y;
        float2 uv = i.texcoord.xy;
        float t = _Time.y * 0.9;

        float static1 = S(-.5, 1., _RainAmount) * 1.5;
        float dynamic1 = S(.25, .75, _RainAmount);
        float dynamic2 = S(.0, .5, _RainAmount);

        float offset1 = Drops(aspectUV, t, static1, dynamic1, dynamic2);
        float2 e = float2(.001, 0.);
        float offset2 = Drops(aspectUV + e, t, static1, dynamic1, dynamic2);
        float offset3 = Drops(aspectUV + e.yx, t, static1, dynamic1, dynamic2);
        float2 n = float2(offset2 - offset1, offset3 - offset1);

        float2 texCoord = float2(uv.x + n.x, uv.y + n.y);
        float4 lod = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texCoord);
        float3 col = lod.rgb;

        return float4(col, 1);
    }

    ENDHLSL

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

            ENDHLSL
        }
    }
}

