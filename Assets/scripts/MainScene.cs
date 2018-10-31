using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    public GameObject rend;

    // Use this for initialization
    void Start()
    {
        Line x1 = new Line(new Vector2(1, 0), new Vector2(1, 1), 0f, 0f);
        Line x2 = new Line(new Vector2(1, 1), new Vector2(0, 1), 0f, 0f);
        Line x3 = new Line(new Vector2(0, 1), new Vector2(0, 2), 0f, 0f);
        Line x4 = new Line(new Vector2(0, 2), new Vector2(1, 2), 0f, 0f);
        Line x5 = new Line(new Vector2(1, 2), new Vector2(1, 3), 0f, 0f);
        Line x6 = new Line(new Vector2(1, 3), new Vector2(-1, 3), 0f, 0f);
        Line x7 = new Line(new Vector2(-1, 3), new Vector2(-1, 0), 0f, 0f);
        Line x8 = new Line(new Vector2(-1, 0), new Vector2(1, 0), 0f, 0f);

        //Line x5 = new Line(new Vector2(1, 8), new Vector2(1, 0), 0f, 0f);
        Polygon p = new Polygon(new List<Curve> {x1, x2, x3, x4, x5, x6, x7, x8});

        GameObject rendins = Instantiate(rend, transform);
        rendins.transform.parent = this.transform;
        CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
        Material normalMaterial = new Material(Shader.Find("Standard"));
        normalMaterial.mainTexture = Resources.Load<Texture>("Textures/road");
        decomp.CreateMesh(new Bezeir(new Vector2(0f, 0f), new Vector2(0f, 5f), new Vector2(2f, 5f), 0f, 0f), 0f, normalMaterial, p);



    }
}
