using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour {
	public GameObject RoadManager;

	// Use this for initialization
	void Start () {
        /*
        RoadDrawing drawing= GameObject.FindWithTag("Road/curveIndicator").GetComponent<RoadDrawing>();

        RoadManager manager = drawing.roadManager;
        manager.addRoad(new Line(new Vector2(0f, 50f), new Vector2(50f, 0f), 0f, 0f), new List<string> { "lane", "dash_yellow", "lane" });
        manager.addRoad(new Line(new Vector2(50f, 0f), new Vector2(100f, 50f), 0f, 0f), new List<string> { "lane", "dash_yellow", "lane" });
        manager.addRoad(new Line(new Vector2(50f, 50f), new Vector2(50f, 0f), 0f, 0f), new List<string>{"lane", "dash_yellow", "lane" });
        */
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cube.transform.position = new Vector3(0f, 0f, 0f);
        cube.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        cube.transform.localScale = new Vector3(1f, 10f, 1f);
        cube.GetComponent<MeshRenderer>().material = Resources.Load<Material>("indicator");
        Debug.Log(cube.GetComponent<MeshRenderer>().material.mainTexture);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
