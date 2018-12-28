using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    float startParam;
    Road startRoad;

    Path pathOn;
    Road currentRoad;
    int currentSeg;
    float currentParam;

    float speed = 0f;
    float wheelRotation = 0f;

    public float wheeRadius = 0.14f;

    RoadDrawing drawing;

    private void Awake()
    {
        Reset();
        drawing = GameObject.Find("curveIndicator").GetComponent<RoadDrawing>();
    }

    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        if (pathOn != null){
            float distToTravel = speed * Time.deltaTime;
            bool termination;
            int nextSeg;
            Pair<Road, float> nextInfo = pathOn.travelAlong(currentSeg, currentParam, distToTravel, out nextSeg, out termination);
            //Curve curveOn = nextInfo.First.curve;
            Road roadOn = nextInfo.First;
            currentParam = nextInfo.Second;
            currentSeg = nextSeg;

            if (termination)
            {
                Reset();
            }
            else
            {
                //transform.position = curveOn.at(currentParam);
                transform.position = roadOn.at(currentParam);
                /*
                transform.rotation = pathOn.getHeadingOfCurrentSeg(currentSeg) ?
                    Quaternion.LookRotation(curveOn.frontNormal(currentParam), curveOn.upNormal(currentParam)) :
                    Quaternion.LookRotation(-curveOn.frontNormal(currentParam), curveOn.upNormal(currentParam));
                */
                transform.rotation = pathOn.getHeadingOfCurrentSeg(currentSeg) ?
                    Quaternion.LookRotation(roadOn.frontNormal(currentParam), roadOn.upNormal(currentParam)) :
                    Quaternion.LookRotation(-roadOn.frontNormal(currentParam), roadOn.upNormal(currentParam));

                wheelRotation = (wheelRotation + distToTravel / wheeRadius * Mathf.Rad2Deg) % 360;
                /*TODO: calculate wheel radius*/
                transform.GetChild(0).GetChild(1).localRotation = transform.GetChild(0).GetChild(2).localRotation =
                    transform.GetChild(0).GetChild(3).localRotation= transform.GetChild(0).GetChild(4).localRotation = 
                        Quaternion.Euler(wheelRotation, 0f, 0f);

            }

        }
	}

    public void SetStart(Vector3 position){
        Vector3 modifiedPosition = drawing.roadManager.approxNodeToExistingRoad(position, out startRoad);
        startParam = currentParam = (float)startRoad.curve.paramOf(modifiedPosition);
        currentRoad = startRoad;
    }

    public void SetDest(Vector3 position){
        Road endRoad;
        Debug.Assert(startRoad != null);
        Vector3 modifiedPosition = drawing.roadManager.approxNodeToExistingRoad(position, out endRoad);
        float endParam = (float)endRoad.curve.paramOf(modifiedPosition);
        pathOn = drawing.roadManager.findPath(startRoad, startParam, endRoad, endParam);
    }

    private void Reset()
    {
        pathOn = null;
        currentParam = Mathf.Infinity;
        speed = 0f;
        currentSeg = 0;
    }

    public void Accelerate(float a){
        speed += a * Time.deltaTime;
    }
}
