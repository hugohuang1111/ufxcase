using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class BezierUtils
{
    /// <summary>
    /// 线性贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
    /// </summary>
    /// <param name="p0"></param>起始点
    /// <param name="p1"></param>控制点
    /// <param name="t"></param>T值
    /// <returns></returns>根据T值计算出来的贝赛尔曲线点
    private static Vector3 CalculateLineBezierPoint(Vector3 p0, Vector3 p1, float t)
    {
        float u = 1 - t;
         
        Vector3 p = u * p0;
        p +=  t * p1;
    
 
        return p;
    }
 
    /// <summary>
    /// 二次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
    /// </summary>
    /// <param name="p0"></param>起始点
    /// <param name="p1"></param>控制点
    /// <param name="p2"></param>目标点
    /// <param name="t"></param>T值
    /// <returns></returns>根据T值计算出来的贝赛尔曲线点
    private static Vector3 CalculateCubicBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
 
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
 
        return p;
    }
 
    /// <summary>
    /// 三次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
    /// </summary>
    /// <param name="p0">起点</param>
    /// <param name="p1">控制点1</param>
    /// <param name="p2">控制点2</param>
    /// <param name="p3">尾点</param>
    /// <param name="t"></param>T值
    /// <returns></returns>
    private static Vector3 CalculateThreePowerBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float ttt = tt * t;
        float uuu = uu * u;
 
        Vector3 p = uuu * p0;
        p += 3 * t * uu * p1;
        p += 3 * tt * u * p2;
        p += ttt * p3;
 
        return p;
    }
 
    public static Vector2 CubicBezier(Vector2 ctl1, Vector2 ctl2, float t) {
        var p0 = Vector2.zero;
        var p1 = ctl1;
        var p2 = ctl2;
        var p3 = Vector2.one;

        var revT = 1 - t;

        //var p = Vector2.zero;
        //p.x = Mathf.Pow(revT, 3) * p0.x + t * p1.x * ;

        var p = Mathf.Pow(revT, 3) * p0 + 3 * Mathf.Pow(revT, 2) * t * p1 + 3 * revT * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3;

        return p;

        //var v = CalculateThreePowerBezierPoint(Vector3.zero, ctl1, ctl2, Vector3.one, t);
        //return new Vector2(v.x, v.y);

        // Debug.Log($"CubicBezier ctl1:{ctl1} ctl2:{ctl2} t:{t}");
        // var a = Vector3.Lerp(Vector3.zero, ctl1, t);
        // var b = Vector3.Lerp(ctl1, ctl2, t);
        // var c = Vector3.Lerp(ctl2, Vector2.one, t);

        // Debug.Log($"CubicBezier a:{a} b:{b} c:{c}");

        // a = Vector3.Lerp(a, b, t);
        // b = Vector3.Lerp(b, c, t);

        // Debug.Log($"CubicBezier a:{a} b:{b}");

        // a = Vector3.Lerp(a, b, t);

        // Debug.Log($"CubicBezier a:{a}");

        // return a;
    }
 
    /// <summary>
    /// 获取存储贝塞尔曲线点的数组
    /// </summary>
    /// <param name="startPoint"></param>起始点
    /// <param name="controlPoint"></param>控制点
    /// <param name="endPoint"></param>目标点
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>存储贝塞尔曲线点的数组
    public static Vector3[] GetLineBeizerList(Vector3 startPoint,  Vector3 endPoint, int segmentNum)
    {
        Vector3[] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 pixel = CalculateLineBezierPoint(startPoint, endPoint, t);
            path[i - 1] = pixel;
            Debug.Log(path[i - 1]);
        }
        return path;
 
    }
 
    /// <summary>
    /// 获取存储的二次贝塞尔曲线点的数组
    /// </summary>
    /// <param name="startPoint"></param>起始点
    /// <param name="controlPoint"></param>控制点
    /// <param name="endPoint"></param>目标点
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>存储贝塞尔曲线点的数组
    public static Vector3[] GetCubicBeizerList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum)
    {
        Vector3[] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 pixel = CalculateCubicBezierPoint(startPoint, controlPoint, endPoint, t);
            path[i - 1] = pixel;
            Debug.Log(path[i - 1]);
        }
        return path;
 
    }
 
    /// <summary>
    /// 获取存储的三次贝塞尔曲线点的数组
    /// </summary>
    /// <param name="startPoint"></param>起始点
    /// <param name="controlPoint1"></param>控制点1
    /// <param name="controlPoint2"></param>控制点2
    /// <param name="endPoint"></param>目标点
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>存储贝塞尔曲线点的数组
    public static Vector3[] GetThreePowerBeizerList(Vector3 startPoint, Vector3 controlPoint1, Vector3 controlPoint2 , Vector3 endPoint, int segmentNum)
    {
        Vector3[] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 pixel = CalculateThreePowerBezierPoint(startPoint, controlPoint1, controlPoint2, endPoint, t);
            path[i - 1] = pixel;
            Debug.Log(path[i - 1]);
        }
        return path;
 
    }
}
