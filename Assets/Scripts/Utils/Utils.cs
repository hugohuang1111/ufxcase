using UnityEngine;

public class Utils {
    public static void MakeSureDegreeFirstPeriod(ref float deg) {
        while (deg < 0) {
            deg += 360;
        }
        while (deg > 360) {
            deg -= 360;
        }
    }

    public static bool IsFloatEqual(float a, float b) {
        float dif = Mathf.Abs(a - b);

        return dif < 0.01f;
    }

    public static bool IsStringEmpty(string a) {
        if (a is null)
        {
            return true;
        }
        if (0 == a.Length)
        {
            return true;
        }

        return false;
    }

}
