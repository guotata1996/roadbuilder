using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    RoadDrawing drawing;
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

        List<string> narrowLaneConfig = new List<string> {"fence","lane", "solid_yellow", "lane","fence" };
        List<string> wideLaneConfig = new List<string> { "fence", "lane", "dash_white", "lane", "solid_yellow", "lane", "dash_white", "lane", "fence"};

        Curve l1 = new Line(new Vector3(0f, 2f, 0f), new Vector3(160f, 2f, 0f));
        drawing.roadManager.addRoad(l1, wideLaneConfig);
        Curve l2 = new Line(new Vector3(80f, 2f, 0f), new Vector3(80f, 2f, -160f));
        drawing.roadManager.addRoad(l2, narrowLaneConfig);

    }

    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.N)){
            for (int i = 0; i != numCar; ++i)
            {
                vh[i].GetComponent<Vehicle>().ShiftLane(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.M)){
            for (int i = 0; i != numCar; ++i)
            {
                vh[i].GetComponent<Vehicle>().ShiftLane(true);
            }
        }
        */

    }
}
