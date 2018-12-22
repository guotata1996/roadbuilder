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

        Curve b = new Bezeir(new Vector2(0f, 0f), new Vector2(0f, 50f), new Vector2(80f, 40f), 0f, 5f);
        drawing.roadManager.addRoad(b, new List<string> { "lane", "surface" });
        Curve c = new Line(new Vector2(80f, 40f), new Vector2(80f, 0f), 5f, 5f);
        drawing.roadManager.addRoad(c, new List<string> { "lane", "surface" });
        Curve d = new Line(new Vector2(50f, -40f), new Vector2(80f, 0f),5f, 5f);
        drawing.roadManager.addRoad(d, new List<string> { "lane", "surface" });

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
            vh[i].GetComponent<Vehicle>().SetDest(new Vector3(50f, 4f, -40f));
        }

    }

    private void Update()
    {

        for (int i = 0; i != numCar; ++i)
        {
            vh[i].GetComponent<Vehicle>().Accelerate(Random.Range(0.2f, 0.3f));
        }
    }
}
