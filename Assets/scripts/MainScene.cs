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

        /*
        Curve b = new Bezeir(new Vector2(0f, 0f), new Vector2(0f, 40f), new Vector2(80f, 40f), 0f, 10f);
        List<string> sampleLaneConfig = new List<string>{ "solid_white", "lane", "dash_white", "lane", "solid_yellow", "lane" };

        drawing.roadManager.addRoad(b, sampleLaneConfig);
        Curve c = new Line(new Vector2(80f, 40f), new Vector2(80f, 0f), 10f, 0f);
        drawing.roadManager.addRoad(c, sampleLaneConfig);

        Curve d = new Line(new Vector2(40f, 0f), new Vector2(120f, 0f),0f, 0f);
        drawing.roadManager.addRoad(d, sampleLaneConfig);
        */

        List<string> sampleLaneConfig = new List<string> { "lane", "dash_white", "lane", "solid_yellow", "lane" };
        List<string> wideLaneConfig = new List<string> { "lane", "dash_white", "lane", "solid_yellow", "lane", "dash_white", "lane" };

        Curve l1 = new Line(new Vector3(0f, 0f, 0f), new Vector3(80f, 0f, 0f));
        drawing.roadManager.addRoad(l1, sampleLaneConfig);
        Curve l2 = new Line(new Vector3(40f, 0f, 0f), new Vector3(40f, 0f, -40f));
        drawing.roadManager.addRoad(l2, wideLaneConfig);

        //Path sp = drawing.roadManager.findPath(drawing.roadManager.allroads[0], 0.1f, drawing.roadManager.allroads[2], 0.1f);
        //if (sp != null)
        //{
        //    Debug.Log(sp);
        //}
        //else
        //{
        //    Debug.LogError("path not found");
        //}

        Debug.Assert(drawing.roadManager != null);
        /*
        for (int i = 0; i != numCar; ++i)
        {
            vh[i] = Instantiate(carModelPrefab);
            vh[i].GetComponent<Vehicle>().SetStart(new Vector3(0f, 0f, 0f));
            vh[i].GetComponent<Vehicle>().SetDest(new Vector3(50f, 0f, -40f));
        }
        */

    }

    private void Update()
    {

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

        if (Input.GetKeyDown(KeyCode.J)){
            GameObject newVh = Instantiate(carModelPrefab);
            newVh.GetComponent<Vehicle>().SetStart(new Vector3(60f, 0f, 0f));
            newVh.GetComponent<Vehicle>().SetDest(new Vector3(0f, 0f, 0f));
            vh.Add(newVh);
        }

        if (Input.GetKey(KeyCode.T)){
            vh[0].GetComponent<Vehicle>().speed = 0f;
        }
    }
}
