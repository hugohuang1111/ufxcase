using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneEdgeMesh {

    private float mWidth = 1;
    private float mHeight = 1;
    private List<int> mIndices;
    private List<Vector3> mVertices;

    public GameObject GenLaneEdge(List<int> laneIndices, List<Vector3> laneVertices) {
        Init(laneVertices.Count / 2);

        for (int i = 0; i < 4; i+=2) {
            AppendPoint(laneVertices[i]);
        }
        // AppendPoint(new Vector3(0, 1, 0.5f));
        // AppendPoint(new Vector3(0, 1, 2));

        EndPoint();

        GameObject go = new GameObject();
        go.name = "LaneEdge";
        go.AddComponent<MeshRenderer>();
        var mesh = go.AddComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.SetVertices(mVertices);
        mesh.SetIndices(mIndices, MeshTopology.Triangles, 0, false);

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        return go;
    }

    public void Init(int pointLen) {
        mVertices = new List<Vector3>(pointLen*4);
        mIndices = new List<int>((pointLen-1)*8*3 + 4*3);
    }

    public void AppendPoint(Vector3 point) {
        float temp = point.y;
        point.y = point.z;
        point.z = temp;

        Vector3 p = point + new Vector3(-mWidth/2, -mHeight/2, 0);
        mVertices.Add(p);
        p = point + new Vector3(mWidth/2, -mHeight/2, 0);
        mVertices.Add(p);
        p = point + new Vector3(mWidth/2, mHeight/2, 0);
        mVertices.Add(p);
        p = point + new Vector3(-mWidth/2, mHeight/2, 0);
        mVertices.Add(p);

        int verticesLenght = mVertices.Count;
        if (4 == mVertices.Count) {
            /*
             * 3          2
             *  + ------ +
             *  |        |
             *  |        |
             *  + ------ +
             * 0          1
             * 
             *  0,3,2  0,2,1
             * 
             */
            // start face
            mIndices.Add(0);
            mIndices.Add(3);
            mIndices.Add(2);

            mIndices.Add(0);
            mIndices.Add(2);
            mIndices.Add(1);
        } else {
            /*
             * 
             *   7  + ------ + 6
             *     /       / |
             *  3 /     2 /  |
             *  + ------ +   + 5
             *  |   4    |  /
             *  |        | /
             *  + ------ +
             * 0         1
             *
             */

            /*
             * bottom face
             * 
             * 1          5
             *  + ------ +
             *  |        |
             *  |        |
             *  + ------ +
             * 0          4
             * 
             * 0,1,5    0,5,4
             * 
             */
            mIndices.Add(verticesLenght - 8);
            mIndices.Add(verticesLenght - 7);
            mIndices.Add(verticesLenght - 3);

            mIndices.Add(verticesLenght - 8);
            mIndices.Add(verticesLenght - 3);
            mIndices.Add(verticesLenght - 4);

            /*
             * right face
             * 
             * 2          6
             *  + ------ +
             *  |        |
             *  |        |
             *  + ------ +
             * 1          5
             * 
             * 1,2,6    1,6,5
             * 
             */
            mIndices.Add(verticesLenght - 7);
            mIndices.Add(verticesLenght - 6);
            mIndices.Add(verticesLenght - 2);

            mIndices.Add(verticesLenght - 7);
            mIndices.Add(verticesLenght - 2);
            mIndices.Add(verticesLenght - 3);

            /*
             * top face
             * 
             * 3          7
             *  + ------ +
             *  |        |
             *  |        |
             *  + ------ +
             * 2          6
             * 
             * 2,3,7    2,7,6
             * 
             */
            mIndices.Add(verticesLenght - 6);
            mIndices.Add(verticesLenght - 5);
            mIndices.Add(verticesLenght - 1);

            mIndices.Add(verticesLenght - 6);
            mIndices.Add(verticesLenght - 1);
            mIndices.Add(verticesLenght - 2);

            /*
             * left face
             * 
             * 0          4
             *  + ------ +
             *  |        |
             *  |        |
             *  + ------ +
             * 3          7
             * 
             * 3,0,4    3,4,7
             * 
             */
            mIndices.Add(verticesLenght - 5);
            mIndices.Add(verticesLenght - 8);
            mIndices.Add(verticesLenght - 4);

            mIndices.Add(verticesLenght - 5);
            mIndices.Add(verticesLenght - 4);
            mIndices.Add(verticesLenght - 1);
        }
    }

    public void EndPoint() {
        /*
         * 3          2
         *  + ------ +
         *  |        |
         *  |        |
         *  + ------ +
         * 0          1
         * 
         * 0,1,2   0,2,3
         */
        // end face
        int verticesLenght = mVertices.Count;
        mIndices.Add(verticesLenght - 4);
        mIndices.Add(verticesLenght - 3);
        mIndices.Add(verticesLenght - 2);

        mIndices.Add(verticesLenght - 4);
        mIndices.Add(verticesLenght - 2);
        mIndices.Add(verticesLenght - 1);
    }

    /*
     * 这个函数是为了做根据一系列的点, 然后生成对应的方柱体,
     * 这个函数就是做将点往外扩展为两个点的部分
     * 
     * 根据 P1,P2 点把P1往外扩展四个点 E1, E2, E3, E4, 使扩展 E1P1, E2P1, E3P1, E4P1 垂直于P1P2这个方向
     * 
     * 可以用以下三个已知条件, 来计算得到对应的E1, E2, E3, E4的值
     * E1P1 * P1P2 = 0
     * E1P1 * E1P1 = pow(w/2, 2) + pow(h/2, 2)
     * E1P1 应该是在 y = P1.y + h/2 或 y = P1.y - h/2 这两个平面上的
     * 
     */
    private void ExpandPoint(Vector3 p1, Vector3 p2, float w, float h) {
        Vector3 dir = p2 - p1;

        // (x-p1.x, h/2, z-p1.z)*dir = 0
        // =>     x = p1.x + ((p1.z - z)*dir.z - h/2*dir.y)/dir.x

        // h/2 * h/2 + w/2 * w/2 = (x-p1.x)*(x-p1.x) + h/2*h/2 + (z-p1.z)*(z-p1.z)
        // =>     x = p1.x + sqrt(w/2*w/2 - (z-p1.z)*(z-p1.z))

        // 接合上面两个等式
        // p1.x + ((p1.z - z)*dir.z - h/2*dir.y)/dir.x == p1.x + sqrt(w/2*w/2 - (z-p1.z)*(z-p1.z))
        // => 
    }

}

