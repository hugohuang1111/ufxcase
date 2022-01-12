using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    #region 常量

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
    private float CollisionReturnDis = 0.5f;

    #endregion

    #region 变量

    /// <summary>
    /// 玩家transform
    /// </summary>
    public Transform PlayerObj;

    /// <summary>
    /// 角色中心点偏移
    /// </summary>
    public Vector3 PivotOffset = new Vector3(0.0f, 1.0f, 0.0f);

    /// <summary>
    /// 水平瞄准速度
    /// </summary>
    public float HorizontalAimingSpeed = 40.0f;

    /// <summary>
    /// 垂直瞄准速度
    /// </summary>
    public float VerticalAimingSpeed = 40.0f;

    /// <summary>
    /// 最大的垂直角度
    /// </summary>
    public float MaxVerticalAngle = 30.0f;

    /// <summary>
    /// 最小的垂直角度
    /// </summary>
    public float MinVerticalAngle = -60.0f;

    /// <summary>
    /// 基础摄像机偏移的倍率的最大值
    /// </summary>
    public float MaxDistance = 2.0f;

    /// <summary>
    /// 基础摄像机偏移的倍率的最小值
    /// </summary>
    public float MinDistance = 1.0f;

    /// <summary>
    /// 镜头推进的速度
    /// </summary>
    public float ZoomSpeed = 5.0f;

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
    /// 基础摄像机偏移的倍率
    /// </summary>
    private float mDistance = 0.0f;

    #endregion

    #region 内置函数

    void Awake()
    {
        mCameraObj = GetComponent<Camera>().transform;
        mDistance = (MinDistance + MaxDistance) * 0.5f;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (mCameraObj == null)
        {
            Debug.LogError(ERROR_UN_BINDCAM);
            return;
        }

        if (PlayerObj == null)
        {
            Debug.LogError(ERROR_UN_PLAYER);
            return;
        }

        float mousex = Input.GetAxis("Mouse X");
        float mousey = Input.GetAxis("Mouse Y");
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        mAngleH += (Mathf.Clamp(mousex, -1.0f, 1.0f) * HorizontalAimingSpeed);
        mAngleV += (Mathf.Clamp(mousey, -1.0f, 1.0f) * VerticalAimingSpeed);
        mDistance -= zoom * ZoomSpeed;

        mAngleV = Mathf.Clamp(mAngleV, MinVerticalAngle, MaxVerticalAngle);
        mDistance = Mathf.Clamp(mDistance, MinDistance, MaxDistance);

        Quaternion animRotation = Quaternion.Euler(-mAngleV, mAngleH, 0.0f);
        Quaternion camYRotation = Quaternion.Euler(0.0f, mAngleH, 0.0f);
        mCameraObj.rotation = animRotation;

        Vector3 lookatpos = PlayerObj.position + camYRotation * PivotOffset;
        Vector3 camdir = animRotation * CamBaseAxis;
        camdir.Normalize();
        mCameraObj.position = lookatpos + camdir * mDistance;

        // 计算碰撞后的摄像机点
        RaycastHit rayhit;
        bool hit = Physics.Raycast(lookatpos, camdir, out rayhit, mDistance);
        if (hit)
        {
            // 屏蔽角色碰撞
            bool charcol = rayhit.collider as CharacterController;
            if (!charcol)
            {
                mCameraObj.position = rayhit.point - camdir * CollisionReturnDis;

                // 距离修正在范围内(1, 避免摄像机穿插进入角色)
                float distance = Vector3.Distance(mCameraObj.position, lookatpos);
                distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
                mCameraObj.position = lookatpos + camdir * distance;
            }
        }
    }

    void OnDestroy()
    {

    }

    void LateUpdate()
    {

    }

    #endregion
}

