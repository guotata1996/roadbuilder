using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour {
	public GameObject RoadManager;

	// Use this for initialization
	void Start () {
        RoadDrawing drawing= GameObject.FindWithTag("Road/curveIndicator").GetComponent<RoadDrawing>();

        RoadManager manager = drawing.roadManager;
        manager.addRoad(new Line(new Vector2(0f, 0f), new Vector2(50f, 0f), 0f, 0f), new List<string> { "lane", "dash_yellow", "lane" });
        manager.addRoad(new Line(new Vector2(50f, 0f), new Vector2(100f, 0f), 0f, 0f), new List<string> { "lane" });
        manager.addRoad(new Line(new Vector2(50f, 0f), new Vector2(50f, -50f), 0f, 0f), new List<string>{"lane"});
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
