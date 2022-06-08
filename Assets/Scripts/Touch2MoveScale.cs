using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch2MoveScale : MonoBehaviour
{

    public int FOVMin = 40;
    public int FOVMax = 80;
    public float Sensitivity = 1;

    private bool bMoveEnable;
    private Camera cameraComp;
    private Vector2 touchPosition1;
    private Vector2 touchPosition2;
    private bool bSingleFinger;

    void Start()
    {
        cameraComp = GetComponent<Camera>();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseEvent();
#elif UNITY_ANDROID
        HandleTouchEvent();
#endif
    }

    private void HandleMouseEvent()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bMoveEnable = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            bMoveEnable = false;
        }
        if (bMoveEnable)
        {
            float X = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime * 100;
            float Y = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime * 100;
            PlaneMove(new Vector2(-X, -Y));
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel") * Sensitivity * Time.deltaTime * 100;
        ViewScale(scroll);
    }

    private void HandleTouchEvent()
    {
        if (0 == Input.touchCount)
        {
            return;
        }
        if (1 == Input.touchCount)
        {
            bSingleFinger = true;
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                bMoveEnable = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (bMoveEnable)
                {
                    PlaneMove(-touch.deltaPosition * Sensitivity * touch.deltaTime);
                }
                bMoveEnable = true;
            }
            else if (touch.phase == TouchPhase.Stationary)
            {
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                bMoveEnable = false;
            }
        }
        if (2 == Input.touchCount)
        {
            var touch1 = Input.GetTouch(0);
            var touch2 = Input.GetTouch(1);
            if (bSingleFinger)
            {
                touchPosition1 = touch1.position;
                touchPosition2 = touch2.position;
                bSingleFinger = false;
            }
            else
            {
                if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    var pos1 = touch1.position;
                    var pos2 = touch2.position;

                    float dis = Vector3.Distance(pos1, pos2);
                    float lastDis = Vector3.Distance(touchPosition1, touchPosition2);
                    float scale = (dis - lastDis) * Sensitivity * Time.deltaTime;

                    ViewScale(scale);
                }
            }
        }
    }

    private void PlaneMove(Vector2 moveDelta)
    {
        transform.Translate(new Vector3(moveDelta.x, moveDelta.y, 0));
    }

    private void ViewScale(float scale)
    {
        if (0 == scale)
        {
            return;
        }

        if (cameraComp.fieldOfView <= FOVMax && cameraComp.fieldOfView >= FOVMin)
        {
            cameraComp.fieldOfView -= scale;
            if (cameraComp.fieldOfView > FOVMax)
            {
                cameraComp.fieldOfView = FOVMax;
            }
            if (cameraComp.fieldOfView < FOVMin)
            {
                cameraComp.fieldOfView = FOVMin;
            }
        }

    }

}
