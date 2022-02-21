using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SecretTouchAuth : MonoBehaviour
{
    public string SecretCode;
    public UnityEvent OnMatchSecretCode;

    private string curCode;

    void Start() {
    }

    // Update is called once per frame
    void Update() {
        HandleInputPosition();
    }

    private void HandleInputPosition() {
        if (0 == SecretCode.Length) {
            return;
        }

        Vector2 pos = new Vector2(-1, -1);
        if (Input.GetMouseButtonUp(0)) {
            pos = Input.mousePosition;
        } else if (Input.touchCount > 1) {
            pos = Input.touches[0].position;
        }
        if (pos.x < 0) {
            return;
        }
        pos /= new Vector2(Screen.width, Screen.height);
        int code = Pos2Code(pos);
        curCode += code;
        if (curCode.Length > SecretCode.Length) {
            curCode = curCode.Substring(curCode.Length - SecretCode.Length, SecretCode.Length);
        }
        // Debug.LogFormat("Touch Pos:{0} code:{1}", pos, curCode);
        if (curCode == SecretCode) {
            Debug.Log("Match Secret Code");
            OnMatchSecretCode.Invoke();
        }
    }

    private int Pos2Code(Vector2 pos) {
        int row = (int)Mathf.Floor(pos.y / 0.3333f);
        int col = (int)Mathf.Floor(pos.x / 0.3333f);

        return row * 3 + col;
    }

}
