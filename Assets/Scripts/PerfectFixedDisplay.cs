using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(Camera))]
public class PerfectFixedDisplay : MonoBehaviour
{
    public Transform LookAt;
    public Vector3 LookAtOffset;
    public Vector3 LookAtDirection = new Vector3(-1, -1, -1);
    public Rect ScreenArea = new Rect(0, 0, 1, 1);

    private static readonly float DistanceMultiplier = 10f;
    private static readonly Vector3 Vector3Min = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    private static readonly Vector3 Vector3Max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 _moveInCameraCoor = Vector3.zero;
    private Camera _camera;

    private Bounds _targetBBInWorld;
    private Vector3[] _targetBBVerticesInWorld;
    private Vector3[] _targetBBVerticesInCamera;
    private Vector3[] _targetBBVerticesInViewport;
    private Bounds _targetBBInCamera;
    private float _tanValOfFOV;
    private float _tanValOfHorFOV;

    private void Awake()
    {
        _gizmosCubePoints = new List<Vector3>();
        _camera = GetComponent<Camera>();

        _tanValOfFOV = Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        _tanValOfHorFOV = _tanValOfFOV * _camera.aspect;
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            MatkPerspectiveCameraFitScreen();
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            MatkOrthographicCameraFitScreen();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            RestoreCameraParams();
        }
    }
#endif

    #region GizmosRelated
    private Bounds _gizmosCube;

    /// <summary>
    /// front: left-bottom, left-top, right-top, right-bottom
    /// rear : left-bottom, left-top, right-top, right-bottom
    /// </summary>
    private List<Vector3> _gizmosCubePoints;

    private void OnDrawGizmos()
    {
        var m = transform.localToWorldMatrix;
        var savedMat = Gizmos.matrix;

        Gizmos.matrix = m * Matrix4x4.Translate(_moveInCameraCoor);
        //DrawGizmosCube();
        DrawGizmosLines();
        Gizmos.matrix = savedMat;
    }

    private void DrawGizmosCube()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_gizmosCube.center, _gizmosCube.size);
    }
    private void DrawGizmosLines()
    {
        if (null == _gizmosCubePoints)
        {
            return;
        }
        if (8 != _gizmosCubePoints.Count)
        {
            return;
        }

        /// _gizmosCubePoints
        /// <summary>
        /// front: left-bottom, left-top, right-top, right-bottom
        /// rear : left-bottom, left-top, right-top, right-bottom
        /// </summary>

        // front
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_gizmosCubePoints[0], _gizmosCubePoints[1]);
        Gizmos.DrawLine(_gizmosCubePoints[1], _gizmosCubePoints[2]);
        Gizmos.DrawLine(_gizmosCubePoints[2], _gizmosCubePoints[3]);
        Gizmos.DrawLine(_gizmosCubePoints[3], _gizmosCubePoints[0]);

        // rear
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_gizmosCubePoints[4], _gizmosCubePoints[5]);
        Gizmos.DrawLine(_gizmosCubePoints[5], _gizmosCubePoints[6]);
        Gizmos.DrawLine(_gizmosCubePoints[6], _gizmosCubePoints[7]);
        Gizmos.DrawLine(_gizmosCubePoints[7], _gizmosCubePoints[4]);

        // side
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_gizmosCubePoints[0], _gizmosCubePoints[4]);
        Gizmos.DrawLine(_gizmosCubePoints[1], _gizmosCubePoints[5]);
        Gizmos.DrawLine(_gizmosCubePoints[2], _gizmosCubePoints[6]);
        Gizmos.DrawLine(_gizmosCubePoints[3], _gizmosCubePoints[7]);
    }
    #endregion

    public void MatkOrthographicCameraFitScreen()
    {
        ResaveCameraParams();

        Transform camTF = _camera.transform;
        var lookAtPos = LookAt.position + LookAtOffset;

        var BBInWorld = CalculateBoundingBox(LookAt);
        var camPos = lookAtPos - LookAtDirection * BBInWorld.size.magnitude * 2;
        camTF.position = camPos;
        camTF.LookAt(lookAtPos);
        _camera.orthographic = true;

        var BBVerticesInWorld = GetBoundingBoxVertices(BBInWorld);
        var BBVerticesInCamera = TransVerticesWorld2Camera(_camera, BBVerticesInWorld);
        var rectInCamera = CalculateRectForVectices(BBVerticesInCamera);

        float hsize = Mathf.Max(Mathf.Abs(rectInCamera.min.y), Mathf.Abs(rectInCamera.max.y));
        float wsize = Mathf.Max(Mathf.Abs(rectInCamera.min.x), Mathf.Abs(rectInCamera.max.x));
        var aspectOfValidRect = _camera.aspect * ScreenArea.size.x / ScreenArea.size.y;
        var orthographicSize = Mathf.Max(hsize, wsize / aspectOfValidRect);
        orthographicSize /= ScreenArea.size.y;
        Vector3 offset = ScreenArea.center - Vector2.one * 0.5f;
        offset *= new Vector2(orthographicSize * 2 * _camera.aspect, orthographicSize * 2);
        offset = camTF.TransformDirection(offset);

        _camera.orthographicSize = orthographicSize;
        camPos.x -= offset.x;
        camPos.y -= offset.y;
        camPos.z -= offset.z;
        camTF.position = camPos;
    }

    public void MatkPerspectiveCameraFitScreen()
    {
        ResaveCameraParams();

        Transform camTF = _camera.transform;
        var lookAtPos = LookAt.position + LookAtOffset;

        var BBInWorld = CalculateBoundingBox(LookAt);
        var camPos = lookAtPos - LookAtDirection * BBInWorld.size.magnitude * 2;
        camTF.position = camPos;
        camTF.LookAt(lookAtPos);
        _camera.orthographic = false;
    }

    private Vector3 _resavedPosition;
    private Quaternion _resavedRoate;
    private bool _resavedOrthographic;
    private void ResaveCameraParams()
    {
        var camTF = _camera.transform;
        _resavedPosition = camTF.position;
        _resavedRoate = camTF.rotation;
        _resavedOrthographic = _camera.orthographic;
    }

    public void RestoreCameraParams()
    {
        var camTF = _camera.transform;
        camTF.position = _resavedPosition;
        camTF.rotation = _resavedRoate;
        _camera.orthographic = _resavedOrthographic;
    }

    private Vector3[] TransVerticesWorld2Camera(Camera cam, Vector3[] vecticesInWorld)
    {
        var vecticesInCamera = new Vector3[vecticesInWorld.Length];
        for (var i = 0; i < vecticesInWorld.Length; i++)
        {
            var p = cam.transform.InverseTransformPoint(vecticesInWorld[i]);
            vecticesInCamera[i] = p;
        }

        return vecticesInCamera;
    }

    Vector3[] GetBoundingBoxVertices(Bounds bounds)
    {
        Vector3[] corners = new Vector3[8];

        corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        corners[1] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        corners[2] = bounds.max;
        corners[3] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);

        corners[4] = bounds.min;
        corners[5] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        corners[6] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
        corners[7] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);

        return corners;
    }

    private Rect CalculateRectForVectices(Vector3[] vertices)
    {
        Vector2 max = Vector3Min;
        Vector2 min = Vector3Max;

        foreach (Vector2 v in vertices)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        return new Rect(min, max - min);
    }

    private Bounds CalculateBoundingBox(Transform target)
    {
        Bounds bounds = new Bounds(target.position, Vector3.zero);
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    private bool RayIntersectPlane(Vector3 rayOrigin, Vector3 rayDirection, Vector3 planePoint, Vector3 planeNormal, out Vector3 intersectionPoint)
    {
        intersectionPoint = Vector3.zero;

        float denominator = Vector3.Dot(planeNormal, rayDirection);

        if (Mathf.Abs(denominator) < Mathf.Epsilon)
        {
            return false;
        }

        float t = Vector3.Dot(planeNormal, planePoint - rayOrigin) / denominator;

        if (t < 0)
        {
            return false;
        }

        intersectionPoint = rayOrigin + t * rayDirection;
        return true;
    }


















    private void AdjustCameraPosition()
    {
        Transform camTF = _camera.transform;
        var lookAtPos = LookAt.position + LookAtOffset;
        var camPos = lookAtPos - LookAtDirection * DistanceMultiplier;
        camTF.position = camPos;
        camTF.LookAt(lookAtPos);

        _targetBBInWorld = CalculateBoundingBox(LookAt);
        _targetBBVerticesInWorld = GetBoundingBoxVertices(_targetBBInWorld);
        _targetBBVerticesInCamera = TransVerticesWorld2Camera(_camera, _targetBBVerticesInWorld);

        float minMove = float.MaxValue;
        Vector2 center = Vector2.zero;
        for (var i = 0; i < _targetBBVerticesInCamera.Length; i++)
        {
            var p = _targetBBVerticesInCamera[i];
            center.x += p.x;
            center.y += p.y;
        }
        center /= 8;

        for (var i = 0; i < _targetBBVerticesInCamera.Length; i++)
        {
            var p = _targetBBVerticesInCamera[i];
            p.x -= center.x;
            p.y -= center.y;
            p.x /= _tanValOfHorFOV;
            p.y /= _tanValOfFOV;
            _targetBBVerticesInCamera[i] = p;
            minMove = Mathf.Min(minMove, p.z - Mathf.Abs(p.x));
            minMove = Mathf.Min(minMove, p.z - Mathf.Abs(p.y));
        }

        //var srect = CalcTargetBBVerticesInViewportRect();

        //var nearPlaneSize = GetCameraNearClipPlaneSize(_camera);

        //var offsetInViewport = new Vector2(0.5f, 0.5f) - srect.center;
        //for (var i = 0; i < _targetBBVerticesInViewport.Length; i++)
        //{
        //    var p = _targetBBVerticesInViewport[i];
        //    p.x -= offsetInViewport.x;
        //    p.y -= offsetInViewport.y;
        //    _targetBBVerticesInViewport[i] = p;
        //}
        //float minMove = float.MaxValue;
        //for (var i = 0; i < _targetBBVerticesInViewport.Length; i++)
        //{
        //    var p = _targetBBVerticesInViewport[i];
        //    p.z = p.z * (1 - Mathf.Min(p.x, p.y));
        //    _targetBBVerticesInViewport[i] = p;
        //    minMove = Mathf.Min(minMove, p.z);
        //}
        //var offsetInViewSize = offsetInViewport * nearPlaneSize;

        var offsetInCamera = new Vector3(center.x, center.y, minMove);
        camTF.position += camTF.TransformVector(offsetInCamera);

        // camTF.position += offset;
    }

    float CalculateDistanceToFitObjectInView(Bounds bounds)
    {
        float objectSize = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));

        float cameraFOV = _camera.fieldOfView * Mathf.Deg2Rad;

        float distance = objectSize / (2 * Mathf.Tan(cameraFOV / 2));

        return distance;
    }

    private void AdjustCameraPosition3()
    {
        Transform camTF = _camera.transform;

        _targetBBInWorld = CalculateBoundingBox(LookAt);

        var targetCenter = LookAt.position + LookAtOffset;

        var camPos = targetCenter - LookAtDirection * DistanceMultiplier;
        camTF.position = targetCenter;
        // camTF.LookAt(targetCenter);

        var boundsSize = _targetBBInWorld.size;

        var screenWidth = Screen.width;
        var screenHeight = Screen.height;

        var projectionMatrix = _camera.projectionMatrix;

        var targetProjection = projectionMatrix * Matrix4x4.TRS(targetCenter, Quaternion.identity, boundsSize);

        var targetWidth = targetProjection.m00 / targetProjection.m03;
        var targetHeight = targetProjection.m11 / targetProjection.m03;

        var cameraOffsetX = (targetWidth - screenWidth) / 2;
        var cameraOffsetY = (targetHeight - screenHeight) / 2;

        camTF.position -= camTF.right * cameraOffsetX;
        camTF.position -= camTF.up * cameraOffsetY;

    }

    private void AdjustCameraPosition2()
    {
        if (LookAt == null)
        {
            Debug.Log($"diaplay target is null");
            return;
        }

        Transform camTF = _camera.transform;

        _targetBBInWorld = CalculateBoundingBox(LookAt);
        _targetBBVerticesInWorld = GetBoundingBoxVertices(_targetBBInWorld);
        CalcBoundVerticesInCamera();
        CalcBoundInCamera();

        var targetCenter = LookAt.position + LookAtOffset;

        // 1. let camera look at postion with fixed distance
        var camPos = targetCenter - LookAtDirection * DistanceMultiplier;
        camTF.position = camPos;
        camTF.LookAt(targetCenter);

        // 2. move camera postion to suitable show target's boundingbox in camera cooridate
        AdjustCameraPostionForTranslatedBB(_targetBBInWorld);

        AdjustCameraPostionForTargetBB();
    }

    private void AdjustCameraPostionForTargetBB()
    {
        Transform camTF = _camera.transform;
        var vpMin = Vector3Max;
        var vpMax = Vector3Min;
        var topVer = Vector3.zero;
        var bottomVer = Vector3.zero;
        var leftVer = Vector3.zero;
        var rightVer = Vector3.zero;
        foreach (var point in _targetBBVerticesInWorld)
        {
            var vpPoint = _camera.WorldToViewportPoint(point);
            //if (Mathf.Abs(vpPoint.x - 0) < Mathf.Epsilon
            //    || Mathf.Abs(vpPoint.x - 1) < Mathf.Epsilon
            //    || Mathf.Abs(vpPoint.y - 0) < Mathf.Epsilon
            //    || Mathf.Abs(vpPoint.y - 1) < Mathf.Epsilon)
            //{
            //    // target bb vertices's view point reach screen edge, so needn't adjust camera position
            //    return;
            //}

            vpMin = Vector3.Min(vpPoint, vpMin);
            vpMax = Vector3.Max(vpPoint, vpMax);
        }


    }

    private Rect CalcTargetBBInViewportRect()
    {
        var vpMin = Vector3Max;
        var vpMax = Vector3Min;
        foreach (var point in _targetBBVerticesInWorld)
        {
            var vpPoint = _camera.WorldToViewportPoint(point);
            vpMin = Vector3.Min(vpPoint, vpMin);
            vpMax = Vector3.Max(vpPoint, vpMax);
        }

        return new Rect(vpMin, vpMax - vpMin);
    }

    private Rect CalcTargetBBVerticesInViewportRect()
    {
        var vpMin = Vector3Max;
        var vpMax = Vector3Min;
        _targetBBVerticesInViewport = new Vector3[_targetBBVerticesInWorld.Length];
        for (var i = 0; i < _targetBBVerticesInWorld.Length; i++)
        {
            var vpPoint = _camera.WorldToViewportPoint(_targetBBVerticesInWorld[i]);
            _targetBBVerticesInViewport[i] = vpPoint;
            vpMin = Vector3.Min(vpPoint, vpMin);
            vpMax = Vector3.Max(vpPoint, vpMax);
        }

        return new Rect(vpMin, vpMax - vpMin);
    }

    private Vector2 GetCameraNearClipPlaneSize(Camera cam)
    {
        //float halfFOV = camera.fieldOfView * 0.5f;
        //float aspectRatio = Screen.width / (float)Screen.height;

        //float halfHeight = Mathf.Tan(Mathf.Deg2Rad * halfFOV);
        //float halfWidth = halfHeight * aspectRatio;

        //float nearWidth = halfWidth * camera.nearClipPlane * 2;
        //float nearHeight = halfHeight * camera.nearClipPlane * 2;

        float nearPlaneH = 2 * cam.nearClipPlane * _tanValOfFOV;
        float nearPlaneW = nearPlaneH * cam.aspect;

        Debug.Log($"Near Clip Plane W:{nearPlaneW} H:{nearPlaneH}");
        return new Vector2(nearPlaneW, nearPlaneH);
    }

    private void AdjustCameraPostionForTranslatedBB(Bounds bb)
    {
        Transform camTF = _camera.transform;
        _targetBBInCamera = BB2CameraCoor(_camera, bb);
        Debug.Log($"bb in cam: {_targetBBInCamera}");
        _gizmosCubePoints.Clear();
        _gizmosCubePoints.AddRange(GetBoundingBoxVertices(_targetBBInCamera));

        var posInCam = CalcPosForShowBBInCameraCoor(_camera, _targetBBInCamera);
        Debug.Log($"pos in cam: {posInCam}");
        _moveInCameraCoor = -posInCam;
        SetCameraPostion(_camera, camTF.TransformPoint(posInCam));
    }

    private void SetCameraPostion(Camera c, Vector3 pos)
    {
        var old = c.transform.position;
        c.transform.position = pos;
    }

    private bool RectInScreen(Rect r)
    {
        if (r.min.x < 0)
        {
            return false;
        }
        if (r.min.y < 0)
        {
            return false;
        }
        if (r.max.x > Screen.width)
        {
            return false;
        }
        if (r.max.y > Screen.height)
        {
            return false;
        }
        return true;
    }

    private Rect BB2ScreenRect(Camera cam, Bounds bb)
    {
        var points = GetBoundingBoxVertices(bb);
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        foreach (var point in points)
        {
            var p = cam.WorldToScreenPoint(point);
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        return new Rect(min, max - min);
    }

    private Vector3 CalcPosForShowTFInCameraCoor(Camera cam, Bounds bb)
    {
        Vector3 p = bb.center;
        var d1 = bb.extents.y / Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f);
        var hFov = ConvertVerticalFOVToHorizontally(cam.fieldOfView, cam.aspect);
        var d2 = bb.extents.x / Mathf.Tan(hFov * Mathf.Deg2Rad * 0.5f);
        d1 = Mathf.Max(d1, d2);

        var nearMax = bb.max;
        nearMax.z = bb.min.z;
        var dir = (bb.min + nearMax) / 2 - bb.center;
        dir.Normalize();
        return bb.center + dir * (bb.extents.z + d1);
    }

    private Vector3 CalcPosForShowBBInCameraCoor(Camera cam, Bounds bb)
    {
        Vector3 p = bb.center;
        var d1 = bb.extents.y / Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f);
        var hFov = ConvertVerticalFOVToHorizontally(cam.fieldOfView, cam.aspect);
        var d2 = bb.extents.x / Mathf.Tan(hFov * Mathf.Deg2Rad * 0.5f);
        d1 = Mathf.Max(d1, d2);

        var nearMax = bb.max;
        nearMax.z = bb.min.z;
        var dir = (bb.min + nearMax) / 2 - bb.center;
        dir.Normalize();
        return bb.center + dir * (bb.extents.z + d1);
    }

    private void CalcBoundVerticesInCamera()
    {
        _targetBBVerticesInCamera = new Vector3[_targetBBVerticesInWorld.Length];
        for (var i = 0; i < _targetBBVerticesInWorld.Length; i++)
        {
            _targetBBVerticesInCamera[i] = _camera.transform.InverseTransformPoint(_targetBBVerticesInWorld[i]);
        }
    }
    private void CalcBoundInCamera()
    {
        var min = Vector3Max;
        var max = Vector3Min;
        foreach (var p in _targetBBVerticesInCamera)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        _targetBBInCamera = new Bounds((min + max) / 2, max - min);
    }

    private Bounds BB2CameraCoor(Camera cam, Bounds bb)
    {
        var points = GetBoundingBoxVertices(bb);
        var min = Vector3Max;
        var max = Vector3Min;
        Debug.Log($"camera position: {cam.transform.position}, rot:{cam.transform.eulerAngles}");
        foreach (var point in points)
        {
            var p = cam.transform.InverseTransformPoint(point);
            Debug.Log($"trans point: {point} => {p}");
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        Debug.Log($"bb in cam, min:{min}, max:{max}");

        return new Bounds((min + max) / 2, max - min);
    }

    private float ConvertVerticalFOVToHorizontally(float verFOV, float aspect)
    {
        float verFovRadian = verFOV * Mathf.Deg2Rad;
        float camHalfHeight = Mathf.Tan(verFovRadian / 2);
        float horFOVRadian = Mathf.Atan(camHalfHeight * aspect) * 2;
        float horFOV = horFOVRadian * Mathf.Rad2Deg;

        return horFOV;
    }

}
