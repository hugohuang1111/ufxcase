using UnityEngine;

public class SurroundCamera : MonoBehaviour
{

    /// <summary>
    /// 玩家transform
    /// </summary>
    public Transform Target;

    /// <summary>
    /// 角色中心点偏移
    /// </summary>
    public Vector3 TargetPivotOffset = new Vector3(0.0f, 1.0f, 0.0f);

    /// <summary>
    /// 水平旋转速度
    /// </summary>
    public float HorizontalSpeed = 1.0f;

    /// <summary>
    /// 垂直旋转速度
    /// </summary>
    public float VerticalSpeed = 1.0f;

    /// <summary>
    /// 镜头推进的速度
    /// </summary>
    public float ZoomSpeed = 1.0f;

    /// <summary>
    /// 最大的垂直角度
    /// </summary>
    public float MaxVerticalAngle = 30.0f;

    /// <summary>
    /// 最小的垂直角度
    /// </summary>
    public float MinVerticalAngle = -60.0f;

    /// <summary>
    /// 摄像机偏移的的最大值
    /// </summary>
    public float MaxDistance = 20.0f;

    /// <summary>
    /// 摄像机偏移的的最小值
    /// </summary>
    public float MinDistance = 10.0f;




    private bool mFirstApply = true;
    private bool mbDraging = false;
    private Vector3 mDeltaAngle = new Vector3();

    public const string INPUT_MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    public const string ERROR_UN_BINDCAM = "ThirdPersonCam脚本没有绑定摄像机!";
    public const string ERROR_UN_PLAYER = "ThirdPersonCam脚本没有指定玩家";

    /// <summary>
    /// 摄像机的基础方向
    /// </summary>
    private Vector3 CamBaseAxis = Vector3.back;

    /// <summary>
    /// 摄像机和碰撞体的交点向摄像机的观察点移动的距离
    /// </summary>
    private float CollisionReturnDis = 1f;

    /// <summary>
    /// 摄像机
    /// </summary>
    private Transform mCameraObj;

    /// <summary>
    /// 水平旋转的角度
    /// </summary>
    private float mAngleH = 0.0f;

    /// <summary>
    /// 垂直旋转的角度
    /// </summary>
    private float mAngleV = -30.0f;

    /// <summary>
    /// 摄像机偏移的倍率
    /// </summary>
    private float mDistance = 0.0f;

    private Vector3 mDestionatePos = Vector3.zero;

    void Awake() {
        mCameraObj = GetComponent<Camera>().transform;
        mDistance = (MinDistance + MaxDistance) * 0.5f;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (mCameraObj == null) {
            Debug.LogError(ERROR_UN_BINDCAM);
            return;
        }

        if (Target == null) {
            Debug.LogError(ERROR_UN_PLAYER);
            return;
        }

        if (Vector3.zero == mDestionatePos) {
            HandleInput(ref mDeltaAngle);
            if (0 == mDeltaAngle.magnitude && !mFirstApply) {
                return;
            }
            mFirstApply = false;

            mAngleH += (Mathf.Clamp(mDeltaAngle.x, -1.0f, 1.0f) * HorizontalSpeed);
            mAngleV += (Mathf.Clamp(mDeltaAngle.y, -1.0f, 1.0f) * VerticalSpeed);
            mDistance -= mDeltaAngle.z * ZoomSpeed;
        } else {
            Utils.MakeSureDegreeFirstPeriod(ref mAngleH);
            mAngleH = Mathf.Lerp(mAngleH, mDestionatePos.x, Time.deltaTime);
            mAngleV = Mathf.Lerp(mAngleV, mDestionatePos.y, Time.deltaTime);
            mDistance = Mathf.Lerp(mDistance, mDestionatePos.z, Time.deltaTime);

            if (Utils.IsFloatEqual(mAngleH, mDestionatePos.x) &&
                Utils.IsFloatEqual(mAngleV, mDestionatePos.y) &&
                Utils.IsFloatEqual(mDistance, mDestionatePos.z)) {
                    mDestionatePos.x = 0;
                    mDestionatePos.y = 0;
                    mDestionatePos.z = 0;
                }
        }

        mAngleV = Mathf.Clamp(mAngleV, MinVerticalAngle, MaxVerticalAngle);
        mDistance = Mathf.Clamp(mDistance, MinDistance, MaxDistance);

        // Debug.Log(string.Format("H, V, D: {0}, {1}, {2}", mAngleH, mAngleV, mDistance));

        Quaternion animRotation = Quaternion.Euler(-mAngleV, mAngleH, 0.0f);
        Quaternion camYRotation = Quaternion.Euler(0.0f, mAngleH, 0.0f);
        mCameraObj.rotation = animRotation;
        Vector3 lookatpos = Target.position + camYRotation * TargetPivotOffset;
        Vector3 camdir = animRotation * CamBaseAxis;
        camdir.Normalize();
        mCameraObj.position = lookatpos + camdir * mDistance;

        RaycastHit rayhit;
        bool hit = Physics.Raycast(lookatpos, camdir, out rayhit, mDistance);
        if (hit) {
            bool charcol = rayhit.collider as CharacterController;
            if (!charcol) {
                mCameraObj.position = rayhit.point - camdir * CollisionReturnDis;

                float distance = Vector3.Distance(mCameraObj.position, lookatpos);
                distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
                mCameraObj.position = lookatpos + camdir * distance;
            }
        }

    }

    public void MoveTo(Vector3 destInfo) {
        mDestionatePos = destInfo;
        Utils.MakeSureDegreeFirstPeriod(ref mDestionatePos.x);
    }

    private void HandleInput(ref Vector3 move) {
        move.x = 0;
        move.y = 0;
        move.z = 0;
        if (Input.GetMouseButtonDown(0)) {
            mbDraging = true;
        }
        if (Input.GetMouseButtonUp(0)) {
            mbDraging = false;
        }
        if (mbDraging) {
            move.x = Input.GetAxis("Mouse X");
            move.y = Input.GetAxis("Mouse Y");
        } else if (Input.touchCount > 0){
            // try touch event
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved) {
                move = new Vector3(t.deltaPosition.y, t.deltaPosition.x, 0);
                move.x = t.deltaPosition.x;
                move.y = t.deltaPosition.y;
            }
        }
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        move.z = zoom;
    }

}
