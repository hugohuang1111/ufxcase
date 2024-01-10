
#ifndef NOISE_LIB
#define NOISE_LIB

// https://zhuanlan.zhihu.com/p/571815349
// https://zhuanlan.zhihu.com/p/341673601

#include "Hash.hlsl"

float valueNoise(float2 uv)
{
    float2 intPos = floor(uv); //uv晶格化, 取 uv 整数值
    float2 fracPos = frac(uv); //取 uv 小数值

    //二维插值权重，一个类似smoothstep的函数，叫Hermit插值函数，也叫S曲线：S(x) = -2 x^3 + 3 x^2
    //利用Hermit插值特性：可以在保证函数输出的基础上保证插值函数的导数在插值点上为0，这样就提供了平滑性
    float2 u = fracPos * fracPos * (3.0 - 2.0 * fracPos); 

    //四方取点，由于intPos是固定的，所以栅格化了（同一晶格内四点值相同，只是小数部分不同拿来插值）
    float va = hash2to1( intPos + float2(0.0, 0.0) ); 
    float vb = hash2to1( intPos + float2(1.0, 0.0) );
    float vc = hash2to1( intPos + float2(0.0, 1.0) );
    float vd = hash2to1( intPos + float2(1.0, 1.0) );

    //lerp的展开形式，完全可以用lerp(a,b,c)嵌套实现
    float k0 = va;
    float k1 = vb - va;
    float k2 = vc - va;
    float k4 = va - vb - vc + vd;
    float value = k0 + k1 * u.x + k2 * u.y + k4 * u.x * u.y;

    return value;
}

float2 hash2d2(float2 uv)
{
    const float2 k = float2( 0.3183099, 0.3678794 );
    uv = uv * k + k.yx;
    return -1.0 + 2.0 * frac( 16.0 * k * frac( uv.x * uv.y*(uv.x+uv.y)));
}

float perlinNoise(float2 uv)
{
    float2 intPos = floor(uv);
    float2 fracPos = frac(uv);

    float2 u = fracPos * fracPos * (3.0 - 2.0 * fracPos);  

    float2 ga = hash2d2(intPos + float2(0.0,0.0)); 
    float2 gb = hash2d2(intPos + float2(1.0,0.0));
    float2 gc = hash2d2(intPos + float2(0.0,1.0));
    float2 gd = hash2d2(intPos + float2(1.0,1.0));

    float va = dot(ga, fracPos - float2(0.0,0.0));
    float vb = dot(gb, fracPos - float2(1.0,0.0));
    float vc = dot(gc, fracPos - float2(0.0,1.0));
    float vd = dot(gd, fracPos - float2(1.0,1.0));

    float value = va + u.x * (vb - va) + u.y * (vc - va) + u.x * u.y * (va - vb - vc + vd);

    return value;
}

float simpleNoise(float2 uv)
{
    //transform from triangle to quad
    const float K1 = 0.366025404; // (sqrt(3)-1)/2; //quad 转 2个正三角形 的公式参数
    //transform from quad to triangle
    const float K2 = 0.211324865; // (3 - sqrt(3))/6;

    float2 quadIntPos = floor(uv + (uv.x + uv.y)*K1);
    float2 vecFromA = uv - quadIntPos + (quadIntPos.x + quadIntPos.y) * K2;

    float IsLeftHalf = step(vecFromA.y,vecFromA.x);  //判断左右
    float2  quadVertexOffset = float2(IsLeftHalf,1.0 - IsLeftHalf);

    float2  vecFromB = vecFromA - quadVertexOffset + K2;
    float2  vecFromC = vecFromA - 1.0 + 2.0 * K2;

    //衰减计算
    float3  falloff = max(0.5 - float3(dot(vecFromA,vecFromA), dot(vecFromB,vecFromB), dot(vecFromC,vecFromC)), 0.0);

    float2 ga = hash22(quadIntPos + 0.0);
    float2 gb = hash22(quadIntPos + quadVertexOffset);
    float2 gc = hash22(quadIntPos + 1.0);

    float3 simplexGradient = float3(dot(vecFromA,ga), dot(vecFromB,gb), dot(vecFromC, gc));
    float3 n = falloff * falloff * falloff * falloff * simplexGradient;
    return dot(n, float3(70,70,70));
}

float voronoiNoise(float2 uv)
{
    float dist = 16;
    float2 intPos = floor(uv);
    float2 fracPos = frac(uv);

    for(int x = -1; x <= 1; x++) //3x3九宫格采样
    {
        for(int y = -1; y <= 1 ; y++)
        {
            //hash22(intPos + float2(x,y)) 相当于offset，定义为在周围9个格子中的某一个特征点
            //float2(x,y) 相当于周围九格子root
            //如没有 offset，那么格子是规整的距离场
            //如果没有 root，相当于在自己的晶格范围内生成特征点，一个格子就有九个“球球”
            float d = distance(hash22(intPos + float2(x,y)) +  float2(x,y), fracPos); //fracPos作为采样点，hash22(intPos)作为生成点，来计算dist
            dist = min(dist, d);
        }
    }
    return dist;
}

float FBMvalueNoise(float2 uv)
{
    float value = 0;
    float amplitude = 0.5;
    float frequency = 0;

    for (int i = 0; i < 8; i++) 
    {
        value += amplitude * valueNoise(uv); //使用最简单的value噪声做分形，其余同理。
        uv *= 2.0;
        amplitude *= .5;
    }
    return value;
}
#endif
