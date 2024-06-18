using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingCalipersUtils
{
    public static List<Vector3> GetMinimumBoundingRectangle(List<Vector3> points)
    {
        if (points.Count < 1)
        {
            return new List<Vector3>();
        }

        List<Vector3> convexHull = GetConvexHull(points);
        if (convexHull.Count < 3)
        {
            return convexHull;
        }

        float minArea = float.MaxValue;
        List<Vector3> minRect = null;

        int n = convexHull.Count;
        for (int i = 0; i < n; i++)
        {
            // 计算边的方向
            Vector3 pi = convexHull[i];
            Vector3 pj = convexHull[(i + 1) % n];
            Vector2 edge = new Vector2(pj.x - pi.x, pj.y - pi.y);
            edge.Normalize();

            // 计算边的法向量
            Vector2 normal = new Vector2(-edge.y, edge.x);

            // 初始化最大和最小投影点
            float minProj = float.MaxValue;
            float maxProj = float.MinValue;
            float minOrthProj = float.MaxValue;
            float maxOrthProj = float.MinValue;

            // 计算所有点在该边上的投影
            foreach (var p in convexHull)
            {
                Vector2 vec = new Vector2(p.x - pi.x, p.y - pi.y);
                float proj = Vector2.Dot(vec, edge);
                float orthProj = Vector2.Dot(vec, normal);

                if (proj < minProj)
                {
                    minProj = proj;
                }
                if (proj > maxProj)
                {
                    maxProj = proj;
                }
                if (orthProj < minOrthProj)
                {
                    minOrthProj = orthProj;
                }
                if (orthProj > maxOrthProj)
                {
                    maxOrthProj = orthProj;
                }
            }

            float area = (maxProj - minProj) * (maxOrthProj - minOrthProj);
            if (area < minArea)
            {
                minArea = area;

                Vector2 minProjVec = edge * minProj;
                Vector2 maxProjVec = edge * maxProj;
                Vector2 minOrthProjVec = normal * minOrthProj;
                Vector2 maxOrthProjVec = normal * maxOrthProj;

                minRect = new List<Vector3>
                {
                    new Vector3(pi.x + minProjVec.x + minOrthProjVec.x, pi.y + minProjVec.y + minOrthProjVec.y),
                    new Vector3(pi.x + minProjVec.x + maxOrthProjVec.x, pi.y + minProjVec.y + maxOrthProjVec.y),
                    new Vector3(pi.x + maxProjVec.x + maxOrthProjVec.x, pi.y + maxProjVec.y + maxOrthProjVec.y),
                    new Vector3(pi.x + maxProjVec.x + minOrthProjVec.x, pi.y + maxProjVec.y + minOrthProjVec.y)
                };
            }
        }

        return minRect;
    }

    private static List<Vector3> GetConvexHull(List<Vector3> points)
    {
        if (points.Count < 3)
        {
            return new List<Vector3>(points);
        }

        // 找到最左下点
        Vector3 start = points[0];
        foreach (var p in points)
        {
            if (p.y < start.y || (p.y == start.y && p.x < start.x))
            {
                start = p;
            }
        }

        points.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.y - start.y, a.x - start.x);
            float angleB = Mathf.Atan2(b.y - start.y, b.x - start.x);
            if (angleA == angleB)
            {
                float distA = (a.x - start.x) * (a.x - start.x) + (a.y - start.y) * (a.y - start.y);
                float distB = (b.x - start.x) * (b.x - start.x) + (b.y - start.y) * (b.y - start.y);
                return distA.CompareTo(distB);
            }
            return angleA.CompareTo(angleB);
        });

        Stack<Vector3> hull = new Stack<Vector3>();
        hull.Push(start);
        hull.Push(points[1]);

        for (int i = 2; i < points.Count; i++)
        {
            Vector3 top = hull.Pop();
            while (CCW(hull.Peek(), top, points[i]) <= 0)
            {
                top = hull.Pop();
            }
            hull.Push(top);
            hull.Push(points[i]);
        }

        return new List<Vector3>(hull);
    }

    // 判断三个点是否构成逆时针转向
    private static float CCW(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
    }

}
