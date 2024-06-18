using System.Collections.Generic;
using UnityEngine;

public class BoundForPoints : MonoBehaviour
{
    public List<Vector3> _points = new();

    void Start()
    {
        List<Vector3> points = new List<Vector3>
        {
            new Vector3(0, 0),
            new Vector3(1, 1),
            new Vector3(2, 2),
            new Vector3(2, 0),
            new Vector3(1, 3),
            new Vector3(0, 2)
        };

        foreach (var point in points)
        {
            _points.Add(new Vector3(point.x, point.y, 0));
        }

        List<Vector3> hull = RotatingCalipersUtils.GetMinimumBoundingRectangle(_points);

        foreach (var point in hull)
        {
            //Debug.Log($"Hull Point: ({point.x}, {point.y})");
            _gizmosCubePoints.Add(new Vector3(point.x, point.y, 0));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            List<Vector3> hull = RotatingCalipersUtils.GetMinimumBoundingRectangle(_points);

            _gizmosCubePoints.Clear();
            foreach (var point in hull)
            {
                _gizmosCubePoints.Add(new Vector3(point.x, point.y, 0));
            }
        }
    }

    #region GizmosRelated
    private Vector3 _moveInCameraCoor = Vector3.zero;
    private List<Vector3> _gizmosCubePoints = new();

    private void OnDrawGizmos()
    {
        var m = transform.localToWorldMatrix;
        var savedMat = Gizmos.matrix;

        Gizmos.matrix = m * Matrix4x4.Translate(_moveInCameraCoor);
        DrawGizmosPoints();
        DrawGizmosLines();
        Gizmos.matrix = savedMat;
    }

    private void DrawGizmosPoints()
    {
        if (null == _gizmosCubePoints)
        {
            return;
        }
        if (_gizmosCubePoints.Count < 4)
        {
            return;
        }

        Gizmos.color = Color.green;
        foreach (var p in _points)
        {
            Gizmos.DrawSphere(p, 0.1f);
        }
    }

    private void DrawGizmosLines()
    {
        if (null == _gizmosCubePoints)
        {
            return;
        }
        if (_gizmosCubePoints.Count < 4)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(_gizmosCubePoints[0], _gizmosCubePoints[1]);
        Gizmos.DrawLine(_gizmosCubePoints[1], _gizmosCubePoints[2]);
        Gizmos.DrawLine(_gizmosCubePoints[2], _gizmosCubePoints[3]);
        Gizmos.DrawLine(_gizmosCubePoints[3], _gizmosCubePoints[0]);

    }
    #endregion

}
