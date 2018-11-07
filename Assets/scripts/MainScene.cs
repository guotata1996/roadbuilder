using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    public GameObject rend;

    // Use this for initialization
    void Start()
    {
        Vector2 P0 = new Vector2(1f, 0f);
        Vector2 P1 = new Vector2(1f, 1f);
        Vector2 P2 = new Vector2(0f, 1f);
        Vector2 P3 = new Vector2(0f, 2f);
        Vector2 P4 = new Vector2(1f, 2f);
        Vector2 P5 = new Vector2(1f, 3f);
        Vector2 P6 = new Vector2(-1f, 3f);
        Vector2 P7 = new Vector2(-1f, 0f);

        Polygon p = new Polygon(new List<Curve> { new Line(P0, P1), new Line(P1, P2), new Line(P2, P3), new Line(P3, P4), new Line(P4, P5),
            new Line(P5, P6), new Line(P6, P7), new Line(P7, P0)});

        GameObject rendins = Instantiate(rend, transform);
        rendins.transform.parent = this.transform;
        CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
        Material normalMaterial = new Material(Shader.Find("Standard"));
        normalMaterial.mainTexture = Resources.Load<Texture>("Textures/road");
        decomp.CreateMesh(new Bezeir(new Vector2(0f, 0f), new Vector2(0f, 10f), new Vector2(5f, 10f), 0f, 0f), 0f, normalMaterial, p);



    }
}
