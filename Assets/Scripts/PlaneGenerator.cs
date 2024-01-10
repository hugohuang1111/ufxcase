
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class PlaneGenerator : MonoBehaviour
{
    public string MeshName;

    public Vector2 LeftTop      = new Vector2(-0.5f, 0);
    public Vector2 RightTop     = new Vector2( 0.5f, 0);
    public Vector2 LeftBottom   = new Vector2(-0.5f, 1);
    public Vector2 RightBottom  = new Vector2( 0.5f, 1);

    public Vector2 CtlLeft1     = new Vector2( 0, 0);
    public Vector2 CtlLeft2     = new Vector2( 1, 1);
    public Vector2 CtlRight1    = new Vector2( 0, 0);
    public Vector2 CtlRight2    = new Vector2( 1, 1);

    // public Vector2Int Size   = Vector2Int.one;
    public Vector2Int GridSize = Vector2Int.one;

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = GenPlaneMesh();
    }

#if UNITY_EDITOR
    public bool GenerateMesh = false;
    void Update()
    {
        if (GenerateMesh)
        {
            GenerateMesh = false;
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = GenPlaneMesh();
        }
    }
#endif

    private Mesh GenPlaneMesh()
    {
        Vector3[] vertices;
        Vector2[] uvs;
        int[] indices;

        GenVertices(GridSize.x, GridSize.y, out vertices, out uvs, out indices);

        Mesh mesh = new Mesh();
        mesh.name = Utils.IsStringEmpty(MeshName) ? "PlaneMesh" : MeshName;
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    /// <summary>
    /// 在 XZ 平面生成顶点数据
    ///     顶点         UV
    /// -1 < X < 1   0 < X < 1
    ///  0 < Z < 2   θ < Y < 1
    /// </summary>
    /// <param name="gridw"></param>
    /// <param name="gridH"></param>
    /// <param name="vertices"></param>
    /// <param name="uvs"></param>
    /// <param name="indices"></param>
    private void GenVertices(int gridW,int gridH, out Vector3[] vertices, out Vector2[] uvs, out int[] indices)
    {
        vertices = new Vector3[(gridW + 1)*(gridH + 1)];
        uvs = new Vector2[(gridW + 1)*(gridH + 1)];
        indices = new int[gridW * gridH * 2 * 3];

        int idx=0;
        for (int z = 0; z <= gridH; z++)
        {
            float zpercent = (float)z / gridH;
            var bezierLeft = CubicBezierUtils.CubicBezierNormal(CtlLeft1, CtlLeft2, zpercent);
            var bezierRight = CubicBezierUtils.CubicBezierNormal(CtlRight1, CtlRight2, zpercent);

            var left = VectorLerp(LeftTop, LeftBottom, bezierLeft);
            var right = VectorLerp(RightTop, RightBottom, bezierRight);
            // Debug.Log($"l:{left} r:{right}, pl:{bezierLeft} pr:{bezierRight} p:{zpercent}");
            for (int x = 0; x <= gridW; x++)
            {
                float xpercent = (float)x / gridW;
                var p = Vector2.Lerp(left, right, xpercent);
                idx = z * (gridW + 1) + x;
                vertices[idx].x = p.x;
                vertices[idx].y = 0;
                vertices[idx].z = p.y;
                
                uvs[idx].x = xpercent;
                uvs[idx].y = zpercent;
            }
        }

        var indicesIdx = 0;
        for (int z = 1; z <= gridH; z++)
        {
            for (int x = 0; x <= gridW; x++)
            {
                idx = z*(gridW+1)+x;
                var idx0 = idx - (gridW + 1) - 1;
                var idx2 = idx0 + 1;
                if (x > 0)
                {
                    indices[indicesIdx] = idx0; indicesIdx++;
                    indices[indicesIdx] = idx; indicesIdx++;
                    indices[indicesIdx] = idx2; indicesIdx++;
                }
                
                idx0 = idx2;
                idx2 = idx + 1;
                
                if (x < gridW)
                {
                    indices[indicesIdx] = idx0; indicesIdx++;
                    indices[indicesIdx] = idx; indicesIdx++;
                    indices[indicesIdx] = idx2; indicesIdx++;
                }
            }
        }
    }

    private Vector2 VectorLerp(Vector2 a, Vector2 b, Vector2 t)
    {
        var v = Vector2.zero;
        v.x = a.x + (b.x - a.x) * t.x;
        v.y = a.y + (b.y - a.y) * t.y;

        return v;
    }

}

