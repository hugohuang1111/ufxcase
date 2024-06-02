using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitCubeInView : MonoBehaviour
{
    public GameObject cube;
    public Camera cam;

    void Start()
    {
        FitCube();
    }

    void FitCube()
    {
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        Bounds bounds = cubeRenderer.bounds;
        Vector3 cubeCenter = bounds.center;

        float aspectRatio = cam.aspect;
        float verticalFOV = cam.fieldOfView * Mathf.Deg2Rad;
        float horizontalFOV = 2 * Mathf.Atan(Mathf.Tan(verticalFOV / 2) * aspectRatio);

        Vector3[] vertices = new Vector3[8];
        vertices[0] = bounds.min;
        vertices[1] = bounds.max;
        vertices[2] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        vertices[3] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        vertices[4] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
        vertices[5] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        vertices[6] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        vertices[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);

        float maxDistance = 0f;
        foreach (Vector3 vertex in vertices)
        {
            float distance = CalculateCameraDistance(vertex, verticalFOV, horizontalFOV, aspectRatio);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }

        cam.transform.position = cubeCenter - cam.transform.forward * maxDistance;
        cam.transform.LookAt(cubeCenter);
    }

    float CalculateCameraDistance(Vector3 vertex, float verticalFOV, float horizontalFOV, float aspectRatio)
    {
        Vector3 offset = vertex - cube.transform.position;

        float distance = Mathf.Max(
            Mathf.Abs(offset.y) / Mathf.Tan(verticalFOV / 2),
            Mathf.Abs(offset.x) / Mathf.Tan(horizontalFOV / 2),
            Mathf.Abs(offset.z)
        );

        return distance;
    }
}
