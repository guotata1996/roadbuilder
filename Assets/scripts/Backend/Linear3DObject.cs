using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//object that is rendered in a continuous manner
class Linear3DObject
{
    public Material linearMaterial, crossMaterial;
    public float dashLength, dashInterval;
    public Polygon cross_section;
    public float offset;
    public float margin_0, margin_1;
    public string tag;
    public Linear3DObject(string name, float param = 0f)
    {
        //TODO: read config dynamically
        //TODO: non-symmetry case
        tag = name;
        margin_0 = margin_1 = offset = 0f;
        switch (name)
        {
            case "fence":
                {
                    crossMaterial = Resources.Load<Material>("Materials/roadBarrier1");
                    linearMaterial = Resources.Load<Material>("Materials/roadBarrier2");
                    Vector2 P0 = new Vector2(0.1f, 0f);
                    Vector2 P1 = new Vector2(0.1f, 1.0f);
                    Vector2 P2 = new Vector2(0f, 1.0f);
                    Vector2 P3 = new Vector2(-0.1f, 1.0f);
                    Vector2 P4 = new Vector2(-0.1f, 0f);
                    cross_section = new Polygon(new List<Curve> { new Line(P0, P1), new Arc(P2, P1, Mathf.PI), new Line(P3, P4), new Line(P4, P0) });
                    dashLength = 0.2f;
                    dashInterval = 3f;
                }
                break;
            case "lowbar":
                {
                    crossMaterial = linearMaterial = Resources.Load<Material>("Materials/white");
                    Vector2 P0 = new Vector2(0f, 0.4f);
                    Vector2 P1 = new Vector2(0.1f, 0.4f);
                    Vector2 P2 = new Vector2(-0.1f, 0.4f);
                    cross_section = new Polygon(new List<Curve> { new Arc(P0, P1, Mathf.PI), new Arc(P0, P2, Mathf.PI) });
                    dashInterval = 0f;
                }
                break;
            case "highbar":
                {
                    crossMaterial = linearMaterial = Resources.Load<Material>("Materials/white");
                    Vector2 P0 = new Vector2(0f, 0.9f);
                    Vector2 P1 = new Vector2(0.1f, 0.9f);
                    Vector2 P2 = new Vector2(-0.1f, 0.9f);
                    cross_section = new Polygon(new List<Curve> { new Arc(P0, P1, Mathf.PI), new Arc(P0, P2, Mathf.PI) });
                    dashInterval = 0f;
                }
                break;

            case "squarecolumn":
                linearMaterial = crossMaterial = Resources.Load<Material>("Materials/concrete");
                cross_section = new Polygon(new List<Curve>{
                    new Line(new Vector2(0.5f, -0.2f), new Vector2(-0.5f, -0.2f)),
                    new Line(new Vector2(-0.5f, -0.2f), new Vector2(-0.5f, -1f)),
                    new Line(new Vector2(-0.5f, -1f), new Vector2(0.5f, -1f)),
                    new Line(new Vector2(0.5f, -1f), new Vector2(0.5f, -0.2f))
                },
                                            new List<float>{
                    0f,
                    0f,
                    -1.0f,
                    -1.0f
                }
                );

                dashLength = 1f;
                dashInterval = 16f;
                break;
            case "crossbeam":
                linearMaterial = crossMaterial = Resources.Load<Material>("Materials/concrete");
                if (param > 0f)
                {
                    setParam(param);
                }
                dashLength = 1f;
                dashInterval = 16f;
                break;
            case "bridgepanel":
                linearMaterial = Resources.Load<Material>("Materials/roadsurface");
                crossMaterial = Resources.Load<Material>("Materials/concrete");
                if (param > 0f)
                {
                    setParam(param);
                }
                dashInterval = 0f;
                break;
            default:
                break;
        }
    }

    public void setParam(float param)
    {
        switch (tag)
        {
            case "crossbeam":
                cross_section = new Polygon(new List<Curve>{
                        new Line(new Vector2(param/2, -0.2f), new Vector2(-param/2, -0.2f)),
                        new Line(new Vector2(-param/2, -0.2f), new Vector2(-param/2, -1f)),
                        new Line(new Vector2(-param/2, -1f), new Vector2(-1f, -1.3f)),
                        new Line(new Vector2(-1f, -1.3f), new Vector2(1f, -1.3f)),
                        new Line(new Vector2(1f, -1.3f), new Vector2(param/2, -1f)),
                        new Line(new Vector2(param/2, -1f), new Vector2(param/2, -0.2f))
                    });
                break;
            case "bridgepanel":
                cross_section = new Polygon(new List<Curve>
                {
                    new Line(new Vector2(param/2, 0f), new Vector2(-param/2, 0f)),
                    new Line(new Vector2(-param/2, 0f), new Vector2(-param/2, -0.2f)),
                    new Line(new Vector2(-param/2, -0.2f), new Vector2(param/2, -0.2f)),
                    new Line(new Vector2(param/2, -0.2f), new Vector2(param/2, 0f))
                });
                break;
        }
    }
}
