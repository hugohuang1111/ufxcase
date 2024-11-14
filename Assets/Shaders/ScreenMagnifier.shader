Shader "UFXCase/ScreenMagnifier"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
    #include "./Lib/Hash.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float _Radius;
    float _Amplify;
    float _Rate;
    float _Ratio;
    float2 _Position;

    float2 Magnify(float2 uv, float2 center, float radius, float amplify) {
        float2 delta = uv - center;
        float dist = length(delta);
        float ratio = dist / radius;
        float2 dir = delta / dist;

        float2 offset = dir * (sin(ratio * 3.1415926) * radius * amplify);

        return offset;
    }

    // 标准的放大镜
    float2 Amplif(float2 uv, float2 center, float radius, float amplify) {
        float2 dir = uv - center;
        float dis = length(dir);

        // 1. 放大的边缘是渐变的
        // uv -= dir * (amplify - 1) / amplify * smoothstep(radius + 0.05, radius, dis);

        // 2. 放大的边缘是直接突变
        // uv -= dir * (amplify - 1) / amplify * step(dis, radius);

        // 3. 凸透镜, sin 实现
        // uv -= dir * (amplify - 1) / amplify * cos(dis * 1.57 / radius) * step(dis, radius);

        // 4. 凸透镜, pow 实现, 更真实点
        uv -= dir * (amplify - 1) / amplify * (1- pow(dis/radius, _Rate)) * step(dis, radius);

        return uv;
    }

    float4 Frag(VaryingsDefault i) : SV_Target {
        float2 uv = i.texcoord;
        float2 ratioUV = uv;
        ratioUV.x *= _Ratio;
        _Position.x *= _Ratio;

        _Amplify = max(0.1, _Amplify);
        uv = Amplif(ratioUV, _Position, _Radius, _Amplify);
        uv.x /= _Ratio;

        float4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        return texCol;
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

