using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionRect : MonoBehaviour
{
    public Color Color = new Color(0, 1, 0, 0.1f);
    public Color EdgeColor = Color.green;

    private bool mbDrawing;
    private Material mRectMat;
    private Vector2 mStartPosition = Vector2.zero;
    private Vector2 mEndPosition = Vector2.zero;

    void Start()
    {
        mRectMat = new Material(Shader.Find("UFXCase/VertexColor"));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            mbDrawing = true;
            mStartPosition = Input.mousePosition;
            SelectionMgr.Instance.BeginDraw();
        }
        if (Input.GetMouseButtonUp(0)) {
            mbDrawing = false;
        }
    }

    private void OnPostRender() {
        if (!mbDrawing) {
            return;
        }
        if (!mRectMat) {
            return;
        }

        GL.PushMatrix();
        mEndPosition = Input.mousePosition;

        SelectionMgr.Instance.Drawing(mStartPosition, mEndPosition);
        mRectMat.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.QUADS);
        GL.Color(Color);
        GL.Vertex3(mStartPosition.x, mStartPosition.y, 0f);
        GL.Vertex3(mEndPosition.x, mStartPosition.y, 0);
        GL.Vertex3(mEndPosition.x, mEndPosition.y, 0);
        GL.Vertex3(mStartPosition.x, mEndPosition.y, 0);
        GL.End();
        GL.Begin(GL.LINES);
        GL.Color(EdgeColor);
        GL.Vertex3(mStartPosition.x, mStartPosition.y, 0f);
        GL.Vertex3(mEndPosition.x, mStartPosition.y, 0f);
        GL.Vertex3(mEndPosition.x, mStartPosition.y, 0f);
        GL.Vertex3(mEndPosition.x, mEndPosition.y, 0f);
        GL.Vertex3(mEndPosition.x, mEndPosition.y, 0f);
        GL.Vertex3(mStartPosition.x, mEndPosition.y, 0f);
        GL.Vertex3(mStartPosition.x, mEndPosition.y, 0f);
        GL.Vertex3(mStartPosition.x, mStartPosition.y, 0f);
        GL.End();
        GL.PopMatrix();
    }
}
