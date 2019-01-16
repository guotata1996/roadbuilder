using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MainScene : MonoBehaviour
{
    RoadDrawing drawing;
    public GameObject rend;
    public GameObject carModelPrefab;

    int numCar = 1;
    List<GameObject> vh;

    // Use this for initialization
    private void Awake()
    {
        vh = new List<GameObject>();
    }

    void Start()
    {

        drawing = GameObject.Find("curveIndicator").GetComponent<RoadDrawing>();

        List<string> nofencecfg3 = new List<string> { "lane", "dash_white", "lane", "dash_white", "lane" };
        List<string> nofencecfg2 = new List<string> { "lane", "dash_white", "lane" };

        List<string> fencecfg = new List<string> {"fence","lane", "dash_white", "lane","fence" };
        List<string> singleLane = new List<string> { "lane" };
        /*
        Vector2 P0 = new Vector2(-20f, 40f);
        Vector2 P1 = new Vector2(-20f, 20f);
        Vector2 P2 = new Vector2(-40f, 20f);
        Vector2 P3 = new Vector2(-40f, -20f);
        Vector2 P4 = new Vector2(-20f, -20f);
        Vector2 P5 = new Vector2(-20f, -40f);
        Vector2 P6 = new Vector2(20f, -40f);
        Vector2 P7 = new Vector2(20f, -20f);
        Vector2 P8 = new Vector2(40f, -20f);
        Vector2 P9 = new Vector2(40f, 20f);
        Vector2 P10 = new Vector2(20f, 20f);
        Vector2 P11 = new Vector2(20f, 40f);
        drawing.roadManager.addRoad(Arc.TryInit(P1, P0, Mathf.PI/2), nofencecfg3);
        drawing.roadManager.addRoad(Line.TryInit(P2, P3), nofencecfg2);
        drawing.roadManager.addRoad(Arc.TryInit(P4, P3, Mathf.PI / 2), nofencecfg3);
        drawing.roadManager.addRoad(Line.TryInit(P5, P6), nofencecfg2);
        drawing.roadManager.addRoad(Arc.TryInit(P7, P6, Mathf.PI / 2), nofencecfg3);
        drawing.roadManager.addRoad(Line.TryInit(P8, P9), nofencecfg2);
        drawing.roadManager.addRoad(Arc.TryInit(P10, P9, Mathf.PI / 2), nofencecfg3);
        drawing.roadManager.addRoad(Line.TryInit(P11, P0), nofencecfg2);

        Vector3 P12 = new Vector3(5f, 0f, -80f);
        Vector3 P13 = new Vector3(5f, 5f, -50f);
        Vector3 P14 = new Vector3(5f, 5f, 50f);
        Vector3 P15 = new Vector3(5f, 0f, 80f);
        drawing.roadManager.addRoad(Line.TryInit(P12, P13), fencecfg);
        drawing.roadManager.addRoad(Line.TryInit(P13, P14), fencecfg);
        drawing.roadManager.addRoad(Line.TryInit(P14, P15), fencecfg);

        Vector3 P16 = new Vector3(-5f, 0f, 80f);
        Vector3 P17 = new Vector3(-5f, 5f, 50f);
        Vector3 P18 = new Vector3(-5f, 5f, -50f);
        Vector3 P19 = new Vector3(-5f, 0f, -80f);
        drawing.roadManager.addRoad(Line.TryInit(P16, P17), fencecfg);
        drawing.roadManager.addRoad(Line.TryInit(P17, P18), fencecfg);
        drawing.roadManager.addRoad(Line.TryInit(P18, P19), fencecfg);

        Vector3 P20 = new Vector3(5f, 0f, -100f);
        Vector3 P21 = new Vector3(5f, 0f, 100f);
        Vector3 P22 = new Vector3(-5f, 0f, 100f);
        Vector3 P23 = new Vector3(-5f, 0f, -100f);
        drawing.roadManager.addRoad(Line.TryInit(P20, P12), nofencecfg3);
        drawing.roadManager.addRoad(Line.TryInit(P15, P21), nofencecfg3);
        drawing.roadManager.addRoad(Line.TryInit(P22, P16), nofencecfg3);
        drawing.roadManager.addRoad(Line.TryInit(P19, P23), nofencecfg3);

        List<string> doubledircfg = new List<string> { "lane", "solid_yellow", "lane" };

        Vector2 P24 = new Vector2(60f, 0f);
        Vector2 P25 = new Vector2(100f, 0f);
        Vector2 P26 = new Vector2(-60f, 0f);
        Vector2 P27 = new Vector2(-100f, 0f);
        drawing.roadManager.addRoad(Line.TryInit(P24, P25), doubledircfg);
        drawing.roadManager.addRoad(Line.TryInit(P26, P27), doubledircfg);

        drawing.roadManager.addRoad(Bezeir.TryInit(P11, new Vector2(20f, 60f), Algebra.toVector2(P15)), singleLane);
        drawing.roadManager.addRoad(Bezeir.TryInit(Algebra.toVector2(P16), new Vector2(-20f, 60f), P0), singleLane);
        drawing.roadManager.addRoad(Bezeir.TryInit(Algebra.toVector2(P12), new Vector2(20f, -60f), P6), singleLane);
        drawing.roadManager.addRoad(Bezeir.TryInit(P5, new Vector2(-20f, -60f), Algebra.toVector2(P19)), singleLane);

        drawing.roadManager.addRoad(Line.TryInit(P24, P9), singleLane);
        drawing.roadManager.addRoad(Line.TryInit(P8, P24), singleLane);
        drawing.roadManager.addRoad(Line.TryInit(P2, P26), singleLane);
        drawing.roadManager.addRoad(Line.TryInit(P26, P3), singleLane);

        Vector3 P28 = new Vector3(-5f, 5f, 20f);
        Vector3 P29 = new Vector3(-30f, 8f, 0f);
        Vector3 P30 = new Vector3(0f, 9f, -20f);
        Vector3 P31 = new Vector3(30f, 8f, 0f);
        Vector3 P32 = new Vector3(5f, 5f, 20f);
        Vector3 PCT1 = new Vector3(-30f, 5f, 20f);
        Vector3 PCT2 = new Vector3(-30f, 5f, -20f);
        Vector3 PCT3 = new Vector3(30f, 5f, -20f);
        Vector3 PCT4 = new Vector3(30f, 5f, 20f);

        List<string> doubledirfencecfg = new List<string> { "fence", "lane", "solid_yellow", "lane", "fence" };
        drawing.roadManager.addRoad(Bezeir.TryInit(P28, PCT1, P29), doubledirfencecfg);
        drawing.roadManager.addRoad(Bezeir.TryInit(P29, PCT2, P30), doubledirfencecfg);
        drawing.roadManager.addRoad(Bezeir.TryInit(P30, PCT3, P31), doubledirfencecfg);
        drawing.roadManager.addRoad(Bezeir.TryInit(P31, PCT4, P32), doubledirfencecfg);
        */

        Vector2 P0 = Vector2.zero;
        Vector2 P1 = new Vector2(0f, 20f);
        Vector2 P2 = new Vector2(0.1f, 40f);
        drawing.roadManager.addRoad(Line.TryInit(P0, P1), singleLane);
        drawing.roadManager.addRoad(Line.TryInit(P1, P2), singleLane);

    }

}
