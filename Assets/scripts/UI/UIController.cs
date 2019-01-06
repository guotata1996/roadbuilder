using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ResetDrawing(){
        GameObject.Find("curveIndicator").GetComponent<RoadDrawing>().indicatorType = IndicatorType.none;
    }
}
