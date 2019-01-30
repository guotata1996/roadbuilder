using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MainScene : MonoBehaviour
{
    RoadDrawing drawing;
    public GameObject rend;
    public GameObject carModelPrefab;

    void Start()
    {
        drawing = GameObject.Find("curveIndicator").GetComponent<RoadDrawing>();

        List<string> nofencecfg3 = new List<string> { "lane", "dash_white", "lane", "dash_white", "lane" };
        List<string> nofencecfg2 = new List<string> { "lane", "dash_white", "lane" };

        List<string> fencecfg = new List<string> {"fence","lane", "dash_white", "lane","fence" };
        List<string> singleLane = new List<string> { "lane" };
        List<string> doubledircfg = new List<string> { "lane", "solid_yellow", "lane" };
        List<string> doubledirfencecfg = new List<string> { "fence", "lane", "solid_yellow", "lane", "fence" };

    }
}
