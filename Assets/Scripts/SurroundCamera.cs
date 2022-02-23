using UnityEngine;

public class SurroundCamera : MonoBehaviour
{

    /// <summary>
    /// ���transform
    /// </summary>
    public Transform Target;

    /// <summary>
    /// ��ɫ���ĵ�ƫ��
    /// </summary>
    public Vector3 TargetPivotOffset = new Vector3(0.0f, 1.0f, 0.0f);

    /// <summary>
    /// ˮƽ��ת�ٶ�
    /// </summary>
    public float HorizontalSpeed = 1.0f;

    /// <summary>
    /// ��ֱ��ת�ٶ�
    /// </summary>
    public float VerticalSpeed = 1.0f;

    /// <summary>
    /// ��ͷ�ƽ����ٶ�
    /// </summary>
    public float ZoomSpeed = 1.0f;

    /// <summary>
    /// ���Ĵ�ֱ�Ƕ�
    /// </summary>
    public float MaxVerticalAngle = 30.0f;

    /// <summary>
    /// ��С�Ĵ�ֱ�Ƕ�
    /// </summary>
    public float MinVerticalAngle = -60.0f;

    /// <summary>
    /// �����ƫ�Ƶĵ����ֵ
    /// </summary>
    public float MaxDistance = 20.0f;

    /// <summary>
    /// �����ƫ�Ƶĵ���Сֵ
    /// </summary>
    public float MinDistance = 10.0f;




    private bool mFirstApply = true;
    private bool mbDraging = false;
    private Vector3 mDeltaAngle = new Vector3();

    public const string INPUT_MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    public const string ERROR_UN_BINDCAM = "ThirdPersonCam�ű�û�а������!";
    public const string ERROR_UN_PLAYER = "ThirdPersonCam�ű�û��ָ�����";

    /// <summary>
    /// ������Ļ�������
    /// </summary>
    private Vector3 CamBaseAxis = Vector3.back;

    /// <summary>
    /// ���������ײ��Ľ�����������Ĺ۲���ƶ��ľ���
    /// </summary>
    private float CollisionReturnDis = 1f;

    /// <summary>
    /// �����
    /// </summary>
    private Transform mCameraObj;

    /// <summary>
    /// ˮƽ��ת�ĽǶ�
    /// </summary>
    private float mAngleH = 0.0f;

    /// <summary>
    /// ��ֱ��ת�ĽǶ�
    /// </summary>
    private float mAngleV = -30.0f;

    /// <summary>
    /// �����ƫ�Ƶı���
    /// </summary>
    private float mDistance = 0.0f;

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

        HandleInput(ref mDeltaAngle);
        if (0 == mDeltaAngle.magnitude && !mFirstApply) {
            return;
        }
        mFirstApply = false;

        mAngleH += (Mathf.Clamp(mDeltaAngle.x, -1.0f, 1.0f) * HorizontalSpeed);
        mAngleV += (Mathf.Clamp(mDeltaAngle.y, -1.0f, 1.0f) * VerticalSpeed);
        mDistance -= mDeltaAngle.z * ZoomSpeed;

        mAngleV = Mathf.Clamp(mAngleV, MinVerticalAngle, MaxVerticalAngle);
        mDistance = Mathf.Clamp(mDistance, MinDistance, MaxDistance);

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

    private void HandleInput(ref Vector3 move) {
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
