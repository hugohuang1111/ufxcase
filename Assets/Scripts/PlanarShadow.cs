using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarShadow : MonoBehaviour
{
    public Light light;

    private Material material;

    // Start is called before the first frame update
    void Start()
    {
        var mr = GetComponent<MeshRenderer>();
        if (null != mr)
        {
            material = mr.material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlanarShadow();
    }

    private void UpdatePlanarShadow()
    {
        if (null == material)
        {
            return;
        }

        Vector4 worldpos = transform.position;
        Vector4 projdir = light.transform.forward;

        material.SetVector("_WorldPos", worldpos);
        material.SetVector("_ShadowProjDir", projdir);
    }
}
