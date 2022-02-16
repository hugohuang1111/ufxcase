using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionMgr : MonoBehaviour
{
    private static SelectionMgr gInstance;
    public static SelectionMgr Instance {
        get { return gInstance; }
    }

    public GameObject ObjTemplate;
    public int GenerateCount = 10;

    private Material mOriginMat;
    public Material SelectedMat;

    private List<MeshRenderer> meshRendererList;

    private void Awake() {
        gInstance = this;
    }

    void Start()
    {
        meshRendererList = new List<MeshRenderer>();

        for (int i = 0; i < GenerateCount; i++) {
            GameObject go = GameObject.Instantiate(ObjTemplate) as GameObject;
            go.name = (i + 1).ToString();
            go.transform.parent = transform;
            go.transform.position = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
            meshRendererList.Add(go.GetComponent<MeshRenderer>());
        }
        mOriginMat = meshRendererList[0].material;
    }

    // Update is called once per frame
    void Update() {
    }

    public void BeginDraw() {
        var item = meshRendererList.GetEnumerator();
        while (item.MoveNext()) {
            item.Current.material = mOriginMat;
        }
    }

    public void Drawing(Vector2 point1, Vector2 point2) {
        Rect selRect = new Rect();
        selRect.min = Vector2.Min(point1, point2);
        selRect.max = Vector2.Max(point1, point2);

        var item = meshRendererList.GetEnumerator();
        while (item.MoveNext()) {
            Vector3 position = Camera.main.WorldToScreenPoint(item.Current.transform.position);
            if (selRect.Contains(position)) {
                item.Current.material = SelectedMat;
            } else {
                item.Current.material = mOriginMat;
            }
        }
    }

}
