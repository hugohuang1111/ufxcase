Shader "UFXCase/ScreenRain"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float _Rotate;
    float _Speed;
    float2 _WinSize;

    float N11(float t) {
        return frac(sin(t*10234.324)*123423.23512);
    }

    float N12(float2 p)
    {
        // Dave Hoskins - https://www.shadertoy.com/view/4djSRW
        float3 p3  = frac(float3(p.xyx) * float3(443.897, 441.423, 437.195));
        p3 += dot(p3, p3.yzx + 19.19);
        return frac((p3.x + p3.y) * p3.z);
    }

    float2 rain(float2 uv, float2 gridsWH, float speed) {
        float time = _Time.x;
        uv.y += time * speed;

        float2 cellUV = uv*gridsWH;
        float2 id = floor(cellUV);
        cellUV = frac(cellUV);
        float noise = N12(id);
        time += noise * 6.14;

        float2 dropUV = cellUV - 0.5;
        float2 gridSize = _WinSize/gridsWH;
        gridSize /= gridSize.y;
        float rDrop = 0.;
        dropUV.y += sin(time + sin(time + sin(time)* 0.5)) * 0.4 * speed;
        rDrop = length(dropUV * gridSize);
        rDrop = smoothstep(0.1, 0.08, rDrop);
         
        float2 trailUV = cellUV * float2(1., 8.);
        trailUV = frac(trailUV);
        trailUV -= 0.5;
        float rTrail = length(trailUV * gridSize / float2(1., 8.)); 
        rTrail = smoothstep(0.05, 0.03, rTrail);
        rTrail *= smoothstep(-0.01, 0., dropUV.y);
        rTrail *= smoothstep(0.4, 0., dropUV.y);

        // if (cellUV.x > .99 || cellUV.y > .99) rDrop = 1.;

        // return float2(rDrop * dropUV + rTrail * trailUV);
        return float2(rDrop * dropUV * step(rTrail, rDrop) + rTrail * trailUV * step(rDrop, rTrail));
    }

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float2 originUV = i.texcoord;
        float2 uv = originUV;
        float4 color = float4(0, 0, 0, 0);

        float x = _Rotate;
        float s = 0;
        float c = 0;
        sincos(x, s, c);
        float2x2 rot = float2x2(c, -s, s, c);
        uv -= float2(0.5, 0.5);
        uv = mul(rot, uv);
        uv += float2(0.5, 0.5);

        float2 offsetUV = float2(0, 0);
        offsetUV += rain(uv, float2(10., 6.),  0.1 * _Speed);
        offsetUV += rain(uv, float2(26., 15.),  0.03 * _Speed);

        /*
        float v = length(offsetUV);
        color += float4(v, v, v, v);
        float4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, originUV);
        color += texCol;
        */

        float4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, originUV + offsetUV);
        color = texCol;

        return color;
    }

    ENDHLSL 

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

            ENDHLSL
        }
    }
}

