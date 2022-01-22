using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGUI : MonoBehaviour
{
    public SurroundCamera SurCamera;

    void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 200, 30), "Forward")) {
            moveToForward();
        }
        if (GUI.Button(new Rect(10, 50, 200, 30), "Back")) {
            moveToBack();
        }
        if (GUI.Button(new Rect(10, 90, 200, 30), "Left")) {
            moveToLeft();
        }
        if (GUI.Button(new Rect(10, 130, 200, 30), "RightUp")) {
            moveToRightUp();
        }
    }

    private void moveToForward() {
        SurCamera?.MoveTo(new Vector3(180, 0, 6));
    }

    private void moveToBack() {
        SurCamera?.MoveTo(new Vector3(0, 0, 6));
    }

    private void moveToLeft() {
        SurCamera?.MoveTo(new Vector3(270, 0, 6));
    }

    private void moveToRightUp() {
        SurCamera?.MoveTo(new Vector3(225, -50, 6));
    }

}
