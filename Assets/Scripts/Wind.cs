using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Wind : MonoBehaviour
{
    private bool _bMoveEnable;
    private WindPlane[] _planes;
    private Vector2 _angle;

    void Awake()
    {
        _planes = transform.GetComponentsInChildren<WindPlane>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _bMoveEnable = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _bMoveEnable = false;
        }

        if (_bMoveEnable)
        {
            OnLevel(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
    }

    private void OnLevel(float difH, float difV)
    {
        _angle.x -= difH * 0.1f;
        _angle.y += difV * 0.1f;
        _angle.x = Mathf.Clamp(_angle.x, -1, 1);
        _angle.y = Mathf.Clamp(_angle.y, -1, 1);
        for (int i = 0; i < _planes.Length; i++)
        {
            var plane = _planes[i];
            plane.SetAngle(_angle.x, _angle.y);
        }
    }

}
