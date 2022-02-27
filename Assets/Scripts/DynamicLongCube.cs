using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicLongCube : MonoBehaviour {

    private float mWidth = 1;
    private float mHeight = 1;
    private List<int> mIndices;
    private List<Vector3> mVertices;
    private Vector3 mLastPoint;
    private Vector3 mLastPointDir;

    void Awake() {
        Init(3);
        AppendPoint(new Vector3(0, 0, 1));
        AppendPoint(new Vector3(0, 0, 8));
        AppendPoint(new Vector3(1, 0.5f, 9));
        // AppendPoint(new Vector3(3, 1, 9));
        EndPoint();

        /*
        Debug.LogFormat("Indices Count:{0}", mIndices.Count);
        foreach(var item in mIndices) {
            Debug.Log(item);
        }
         Debug.LogFormat("Vertices Count:{0}", mVertices.Count);
        foreach(var item in mVertices) {
            Debug.Log(item);
        }
        */

        var mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = new Material (Shader.Find ("Standard"));
        var mesh = gameObject.AddComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.SetVertices(mVertices);
        mesh.SetIndices(mIndices, MeshTopology.Triangles, 0, false);

        mesh.RecalculateBounds();
        // mesh.RecalculateTangents();
        mesh.RecalculateNormals();

    }

    void Start() {
    }

    void Update() {
    }

    public void Init(int pointLen) {
        mVertices = new List<Vector3>(pointLen*4);
        mIndices = new List<int>((pointLen-1)*8*3 + 4*3);
    }

    public void AppendPoint(Vector3 point) {
        float temp = point.y;
        // point.y = point.z;
        // point.z = temp;

        if (0 == mLastPoint.magnitude) {
            mLastPoint = point;
            return;
        }
        mLastPointDir = point - mLastPoint;
        ExpandPoint(mLastPoint, mLastPointDir, mWidth, mHeight, mVertices);
        mLastPoint = point;

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

        Vector3 fakeP = mLastPoint + mLastPointDir;
        AppendPoint(fakeP);
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

    private void ExpandPoint(Vector3 point, Vector3 dir, float w, float h, List<Vector3> points) {
        Vector3 Dir = dir.normalized;
        // Debug.LogFormat("Expand point: {0} dir: {1}", point, dir);

        Quaternion q = new Quaternion();
        q.SetLookRotation(Dir);
        Matrix4x4 m = Matrix4x4.Rotate(q);
        // m = Matrix4x4.LookAt(Vector3.left, Dir, Vector3.up).inverse;

        Vector3 v = m*(new Vector3(-w/2, -h/2, 0));
        points.Add(v + point);
        v = m*(new Vector3(w/2, -h/2, 0));
        points.Add(v + point);
        v = m*(new Vector3(w/2, h/2, 0));
        points.Add(v + point);
        v = m*(new Vector3(-w/2, h/2, 0));
        points.Add(v + point);
        // Debug.LogFormat("{0} => {1}, Dir:{2}", v, v2, Dir);
    }


    /*
     * >>> 下面这个 ExpandPoint_Useless 是曾经走过的弯路, 不想删, 主要是为了这个函数用了很多时间, 死了很脑细胞,
     * >>> 先留在这里, 记念一下吧
     *
     */

    /*
     * 这个函数是为了实现根据线(点的集合), 然后生成对应的方柱体,
     * 这个函数就是做将点往外扩展为两个点的部分
     *
     *
     * 根据线(P1,P2) 点把P1往外扩展四个点 E1, E2, E3, E4, 使扩展 E1P1, E2P1, E3P1, E4P1 垂直于P1P2这个方向
     * 
     * 可以用以下三个已知条件, 来计算得到对应的E1, E2, E3, E4的值
     * E1P1 * P1P2 = 0
     * E1P1 * E1P1 = pow(w/2, 2) + pow(h/2, 2)
     * E1P1 应该是在 y = P1.y + h/2 或 y = P1.y - h/2 这两个平面上的
     * 
     */
    private void ExpandPoint_Useless(Vector3 p1, Vector3 p2, float w, float h, List<Vector3> points) {
        Vector3 Dir = p2 - p1;

        /*
         * 再以数学方式表述一次
         * 现有点 P1, P2, 计算出顺着 P1,P2 的方柱体
         * 
         * 理解: P1P2 就是方柱体的整体方向, P1P2 所垂直的面就是方柱体的底面 Plane ,
         * 然后在这个 Plane 上将 P1 向外扩展4个点, 我们就得到了方柱体的底面的4个顶点
         * 然后下一个顶点 P3 到来,又可以得到 P2 的外扩的 4 个点,
         * 这样就得到一个最小的方柱体, 当有新点加入, 方柱体就可以不断延长, 而且是任意方向的延长
         *
         * 由以上的几个点, 我们假设外扩展的顶点为 V (V1, V2, V3, V4), 那么接合以上的数学理解,可知, 有以下几个公式存在
         * 1. VP1 * P1P2 = 0 (V在平面上, P1P2是面的法线, 所以点乘为零)
         * 2. V1P1 * V1P1 = pow(w/2, 2) + pow(h/2, 2) (w,h为方柱底面的宽高, 那么就有勾股定理存在)
         * 3. V.y = P1.y - h/2 或 V.y = P1.y + h/2
         * 这里的第三个条件是假设 这个方柱体与 XZ 这个面是平行的, 那 E 的 y 分量就可很直接的计算出来
         */

        // 将上面的公式结合当前的程序环境,将公式表达出来
        // 这里表达时, 为了减少公式的复杂性, 我们将计算坐标系原点放到 P1 位置, 计算完成再退回到 P1 本身所在的坐标系
        //
        // V = (x, y, z)
        // Dir = p2 - p1
        //
        // 1. V*Dir = 0 => V.x * Dir.x + V.y * Dir.y + V.z * Dir.z = 0
        // 2. pow(V.x, 2) + pow(V.y, 2) + pow(V.z, 2) = pow(w/2, 2) + pow(h/2, 2)
        // 3. V.y = -h/2 或 V.y = h/2
        //
        // => 
        // 令 D = pow(w/2, 2) + pow(h/2, 2)
        // 1. V.x = -(V.y * Dir.y + V.z * Dir.z) / Dir.x
        // 2. V.x = sqrt(D - pow(V.y, 2) - pow(V.z, 2))
        // 3. V.y = -h/2 或 V.y = h/2
        // =>
        // 1 与 2 结合
        // 1. -(V.y * Dir.y + V.z * Dir.z) / Dir.x = sqrt(D - pow(V.y, 2) - pow(V.z, 2))
        // 3. V.y = -h/2 或 V.y = h/2
        float D = w*w/4 + h*h/4;

        // => -(V.y * Dir.y + V.z * Dir.z) / Dir.x = sqrt(D - pow(V.y, 2) - pow(V.z, 2))
        // => 两边同乘 Dir.x
        // -V.y * Dir.y - V.z * Dir.z = Dir.x * sqrt(D - pow(V.y, 2) - pow(V.z, 2))
        // => 两边同时平方
        // pow(Dir.z, 2)*pow(V.z, 2) + 2 * Dir.y * Dir.z * V.y * V.z + pow(V.y*Dir.y, 2)
        //     = pow(Dir.x, 2) * (D - pow(V.y, 2) - pow(V.z, 2))
        // => 移项
        // pow(Dir.z, 2)*pow(V.z, 2) + 2 * Dir.y * Dir.z * V.y * V.z
        //     = pow(Dir.x, 2) * (D - pow(V.y, 2) - pow(V.z, 2)) - pow(V.y*Dir.y, 2)
        // =>
        // pow(Dir.z, 2)*pow(V.z, 2) + 2 * Dir.y * Dir.z * V.y * V.z
        //     = pow(Dir.x, 2) * (D - pow(V.y, 2)) - pow(Dir.x, 2)*pow(V.z, 2) - pow(V.y*Dir.y, 2)
        // =>
        // pow(Dir.z, 2)*pow(V.z, 2) + pow(Dir.x, 2)*pow(V.z, 2) + 2 * Dir.y * Dir.z * V.y * V.z
        //     = pow(Dir.x, 2) * (D - pow(V.y, 2)) - pow(V.y*Dir.y, 2)
        // =>
        // (pow(Dir.x, 2) + pow(Dir.z, 2))*pow(V.z, 2) + 2 * Dir.y * Dir.z * V.y * V.z
        //     = pow(Dir.x, 2) * (D - pow(V.y, 2)) - pow(V.y, 2) * pow(Dir.y, 2)
        // =>
        // (pow(Dir.x, 2) + pow(Dir.z, 2))*pow(V.z, 2) + 2 * Dir.y * Dir.z * V.y * V.z
        //     = pow(Dir.x, 2) * D - pow(Dir.x, 2)* pow(V.y, 2) - pow(V.y, 2) * pow(Dir.y, 2)
        // =>
        // (pow(Dir.x, 2) + pow(Dir.z, 2))*pow(V.z, 2) + 2 * Dir.y * Dir.z * V.y * V.z
        //     = pow(Dir.x, 2) * D - (pow(Dir.x, 2) + pow(Dir.y, 2))* pow(V.y, 2)
        //
        // 令 A = (pow(Dir.x, 2) + pow(Dir.z, 2))
        //    B = Dir.y * Dir.z * V.y
        //    C = pow(Dir.x, 2) * D - (pow(Dir.x, 2) + pow(Dir.y, 2))* pow(V.y, 2)
        float A = Dir.x * Dir.x + Dir.z * Dir.z;
        float B = 0; // Dir.y * Dir.z * V.y;
        float C = 0; // Dir.x * Dir.x * (D - V.y * V.y) - V.y*Dir.y*V.y*Dir.y;
        // => A * pow(V.z, 2) + 2 * B * V.z = C

        // => 当 A != 0 时
        // => pow(V.z, 2) + 2 * B/A * V.z = C/A
        // => pow(V.z, 2) + 2 * B/A * V.z + pow(B/A, 2) = C/A + pow(B/A, 2)
        // => pow(V.z + B/A, 2) = C/A + pow(B/A, 2)
        // => V.z + B/A = sqrt(C/A + pow(B/A, 2))
        // => V.z = sqrt(C/A + pow(B/A, 2)) - B/A

        // 当 A = 0 时
        // V.z = C/(2*B)

        float PY = -h/2;
        float PX = 0;
        float PZ = 0;

        B = Dir.y * Dir.z * PY;
        C = Dir.x * Dir.x * D - (Dir.x * Dir.x + Dir.y * Dir.y)* PY * PY;
        Debug.LogFormat("Dir: {0}", Dir);
        Debug.LogFormat("{0},{1},{2}", A, B, C);
        if (0 == A) {
            PZ = C/(2*B);
        } else {
            PZ = Mathf.Sqrt(C/A + Mathf.Pow(B/A, 2)) - B/A;
        }
        PX = Mathf.Sqrt(D - PY * PY - PZ * PZ);

        points.Add(new Vector3(PX, PY, PZ));
        points.Add(new Vector3(-PX, PY, -PZ));
        points.Add(new Vector3(-PX, PY, PZ));
        points.Add(new Vector3(PX, PY, -PZ));

        PY = h/2;
        B = Dir.y * Dir.z * PY;
        C = Dir.x * Dir.x * D - (Dir.x * Dir.x + Dir.y * Dir.y)* PY * PY;
        if (0 == A) {
            PZ = C/(2*B);
        } else {
            PZ = Mathf.Sqrt(C/A + Mathf.Pow(B/A, 2)) - B/A;
        }
        PX = Mathf.Sqrt(D - PY * PY - PZ * PZ);

        points.Add(new Vector3(PX, PY, PZ));
        points.Add(new Vector3(-PX, PY, -PZ));
        points.Add(new Vector3(-PX, PY, PZ));
        points.Add(new Vector3(PX, PY, -PZ));


        // Quaternion q = new Quaternion();
        // q.SetLookRotation(Dir);
        Matrix4x4 m = Matrix4x4.LookAt(Vector3.left, Dir, Vector3.up).inverse;
        Vector3 v = new Vector3(-w/2, -h/2, 0);


    }

}

