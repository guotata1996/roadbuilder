using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    public GameObject rend;

    // Use this for initialization
    void Start()
    {
        Vector2 P0 = new Vector2(0.1f, 0f);
        Vector2 P1 = new Vector2(0.1f, 1.0f);
        Vector2 P2 = new Vector2(0f, 1.0f);
        Vector2 P3 = new Vector2(-0.1f, 1.0f);
        Vector2 P4 = new Vector2(-0.1f, 0f);

        Polygon p = new Polygon(new List<Curve> { new Line(P0, P1), new Arc(P2, P1, Mathf.PI), new Line(P3, P4), new Line(P4, P0)});

        GameObject rendins = Instantiate(rend, transform);
        rendins.transform.parent = this.transform;
        CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
        Material normalMaterial = new Material(Shader.Find("Standard"));
        normalMaterial.mainTexture = Resources.Load<Texture>("Textures/road");
        decomp.CreateMesh(new Bezeir(new Vector2(0f, 0f), new Vector2(5f, -15f), new Vector2(10f, 0f), 0f, 0f), 0f, normalMaterial, p);



    }
}
