using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindPlane : MonoBehaviour
{
    private Material _mat;
    private Vector4 _angle;

    void Awake()
    {
        _mat = GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void SetAngle(float h, float v)
    {
        if (Utils.IsFloatEqual(h, 0) && Utils.IsFloatEqual(v, 0))
        {
            return;
        }
        Debug.Log($"Angle:{h}, {v}");
        _angle.x = h;
        _angle.y = v;
        _mat.SetVector("_Angle", _angle);
    }


}
