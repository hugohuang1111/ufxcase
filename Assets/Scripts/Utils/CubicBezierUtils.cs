using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicBezierUtils
{
    public static Vector2 CubicBezierNormal(Vector2 ctl1, Vector2 ctl2, float t)
    {
        ResetTempValue(ctl1.x, ctl1.y, ctl2.x, ctl2.y);
        var y = solve(Mathf.Clamp01(t), 0.001f);

        return new Vector2(t, y);
    }

    private static void ResetTempValue(float p1x, float p1y, float p2x, float p2y)
    {
        // Calculate the polynomial coefficients, implicit first and last control points are (0,0) and (1,1).
        cx = 3.0f * p1x;
        bx = 3.0f * (p2x - p1x) - cx;
        ax = 1.0f - cx - bx;

        cy = 3.0f * p1y;
        by = 3.0f * (p2y - p1y) - cy;
        ay = 1.0f - cy - by;
    }

    private static float sampleCurveX(float t)
    {
        // `ax t^3 + bx t^2 + cx t' expanded using Horner's rule.
        return ((ax * t + bx) * t + cx) * t;
    }

    private static float sampleCurveY(float t)
    {
        return ((ay * t + by) * t + cy) * t;
    }

    private static float sampleCurveDerivativeX(float t)
    {
        return (3.0f * ax * t + 2.0f * bx) * t + cx;
    }

    // Given an x value, find a parametric value it came from.
    private static float solveCurveX(float x, float epsilon)
    {
        float t0;
        float t1;
        float t2;
        float x2;
        float d2;

        int i;

        // First try a few iterations of Newton's method -- normally very fast.
        for (t2 = x, i = 0; i < 8; i++)
        {
            x2 = sampleCurveX(t2) - x;
            if (Mathf.Abs(x2) < epsilon)
            {
                return t2;
            }
            d2 = sampleCurveDerivativeX(t2);
            if (Mathf.Abs(d2) < 1e-6)
            {
                break;
            }

            t2 = t2 - x2 / d2;
        }

        // Fall back to the bisection method for reliability.
        t0 = 0.0f;
        t1 = 1.0f;
        t2 = x;

        while (t0 < t1)
        {
            x2 = sampleCurveX(t2);
            if (Mathf.Abs(x2 - x) < epsilon)
            {
                return t2;
            }
            if (x > x2)
            {
                t0 = t2;
            }
            else
            {
                t1 = t2;
            }

            t2 = (t1 - t0) * .5f + t0;
        }

        // Failure.
        return t2;
    }

    // Evaluates y at the given x. The epsilon parameter provides a hint as to the required
    // accuracy and is not guaranteed.
    private static float solve(float x, float epsilon)
    {
        if (x < 0.0f)
        {
            return 0.0f;
        }
        if (x > 1.0f)
        {
            return 1.0f;
        }

        return sampleCurveY(solveCurveX(x, epsilon));
    }

    private static float ax;
    private static float bx;
    private static float cx;

    private static float ay;
    private static float by;
    private static float cy;

}
