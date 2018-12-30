using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    RoadDrawing drawing;
    public GameObject carModelPrefab;

    int numCar = 1;
    GameObject[] vh;

    // Use this for initialization
    private void Awake()
    {
        vh = new GameObject[numCar];
    }

    void Start()
    {
        drawing = GameObject.Find("curveIndicator").GetComponent<RoadDrawing>();

        Curve b = new Bezeir(new Vector2(0f, 0f), new Vector2(0f, 50f), new Vector2(80f, 40f), 0f, 10f);
        List<string> sampleLaneConfig = new List<string>{ "solid_white", "lane", "dash_white", "lane", "solid_yellow", "lane" };


        drawing.roadManager.addRoad(b, sampleLaneConfig);
        Curve c = new Line(new Vector2(80f, 40f), new Vector2(80f, 0f), 10f, 0f);
        drawing.roadManager.addRoad(c, sampleLaneConfig);
        Curve d = new Line(new Vector2(50f, -40f), new Vector2(80f, 0f),0f, 0f);
        drawing.roadManager.addRoad(d, sampleLaneConfig);

        Path sp = drawing.roadManager.findPath(drawing.roadManager.allroads[0], 0f, drawing.roadManager.allroads[2], 0f);
        if (sp != null)
        {
            Debug.Log(sp);
        }
        else
        {
            Debug.Log("path not found");
        }

        Debug.Assert(drawing.roadManager != null);

        for (int i = 0; i != numCar; ++i)
        {
            vh[i] = Instantiate(carModelPrefab);
            vh[i].GetComponent<Vehicle>().SetStart(new Vector3(0f, 0f, 0f));
            vh[i].GetComponent<Vehicle>().SetDest(new Vector3(50f, 0f, -40f));
        }



    }

    private void Update()
    {

        for (int i = 0; i != numCar; ++i)
        {
            vh[i].GetComponent<Vehicle>().Accelerate(Random.Range(0.2f, 0.3f));
        }
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
    }

    void learn(){
        System.Nullable<int> a = 5;
        bool? bb = null;
    }
}

class Test<T>{
    T a;
    bool valid;
    public T bb{
        get{
            return a;
        }

    }
}