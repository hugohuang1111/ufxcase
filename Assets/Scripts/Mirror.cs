using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public Vector4 MirrorPlane;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    Matrix4x4 MatMirror {
        get {
            /*
                P = (x,y,z,1)
                Pm =(Xm,Ym,Zm,1)

                (Pm + P)/2  = P - N*D
                            = P - N*P·Plane
                => Pm   = P - 2 * （ N * (P · Plane) ）
                        = (x,y,z,w) - 2 * ((nx,ny,nz,0)*(x,y,z,w)·(nx,ny,nz,d))
                        = (x,y,z,w) - 2 (x*nx + y*ny + z * nz + w * d) * (nx,ny,nz,0)

                => Xm   = x - 2 (x*nx + y * ny + z * nz + w * d) * nx = x - (x,y,z,w) · (2*nx*nx,2*nx*ny,2*nx*nz,2*nx*d);
                => Ym   = y - 2 (x*nx + y * ny + z * nz + w * d) * ny = y - (x,y,z,w) · (2*ny*nx,2*ny*ny,2*nz*ny,2*ny*d);
                => Zm   = z - 2 (x*nx + y * ny + z * nz + w * d) * nz = z - (x,y,z,w) · (2*nz*nx,2*nz*ny,2*nz*nz,2*nz*d);
                => Wm   = w - 2 (x*nx + y * ny + z * nz + w * d) * 0 = w;
                | 1-2*nx*nx     -2*nx*ny        -2*nx*nz    0  |
                | -2*ny*nx      1 - 2*ny*ny     -2*ny*nz    0  |
                | -2*nz*nx      -2*nz*ny        1-2*nz*nz   0  |
                | -2*d*nx       -2*d*ny         -2*d*nz     1  |
            */

            float nx = MirrorPlane.x;
            float ny = MirrorPlane.y;
            float nz = MirrorPlane.z;
            float d = MirrorPlane.w;
            Matrix4x4 matReflection = new Matrix4x4();

            matReflection[0, 0] = 1 - 2 * nx * nx;
            matReflection[0, 1] = -2 * ny * nx;
            matReflection[0, 2] = -2 * ny * nx;
            matReflection[0, 3] = 0;

            matReflection[1, 0] = -2 * nx * ny;
            matReflection[1, 1] = 1 - 2 * ny * ny;
            matReflection[1, 2] = -2 * nz * ny;
            matReflection[1, 3] = 0;

            matReflection[2, 0] = -2 * nx * nz;
            matReflection[2, 1] = -2 * ny * nz;
            matReflection[2, 2] = 1 - 2 * nz * nz;
            matReflection[2, 3] = 0;

            matReflection[3, 0] = - 2 * d * nx;
            matReflection[3, 1] = -2 * d * ny;
            matReflection[3, 2] = -2 * d * nz;
            matReflection[3, 3] = 1.0f;

            return matReflection;
        }
    }

}
