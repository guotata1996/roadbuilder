using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    float startParam;
    Road startRoad;
    /*dynamic Path info*/
    Path pathOn;
    int currentSeg;
    float currentParam;

    /*lateral info
    laneOn: view from behind the vehicle, left to right 
    */
    int laneOn;
    float rightOffset;

    /*longitudinal info*/
    float speed = 0f;
    float wheelRotation = 0f;

    public float wheeRadius = 0.14f;
    public float lateralSpeed = 2f;

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

            rightOffset = Mathf.Sign(rightOffset) * Mathf.Max(Mathf.Abs(rightOffset) - lateralSpeed * Time.deltaTime, 0f);

            if (termination)
            {
                Reset();
            }
            else
            {
                transform.position = roadOn.at(currentParam) + (
                    pathOn.getHeadingOfCurrentSeg(currentSeg) ?
                    roadOn.rightNormal(currentParam) * (roadOn.getLaneCenterOffset(laneOn) + rightOffset):
                    -roadOn.rightNormal(currentParam) * (roadOn.getLaneCenterOffset(laneOn) + rightOffset));
                /*
                transform.rotation = pathOn.getHeadingOfCurrentSeg(currentSeg) ?
                    Quaternion.LookRotation(curveOn.frontNormal(currentParam), curveOn.upNormal(currentParam)) :
                    Quaternion.LookRotation(-curveOn.frontNormal(currentParam), curveOn.upNormal(currentParam));
                */
                transform.rotation = pathOn.getHeadingOfCurrentSeg(currentSeg) ?
                    Quaternion.LookRotation(roadOn.frontNormal(currentParam), roadOn.upNormal(currentParam)) :
                    Quaternion.LookRotation(-roadOn.frontNormal(currentParam), roadOn.upNormal(currentParam));

                if (rightOffset > 0)
                {
                    transform.Rotate(roadOn.upNormal(currentParam), -Mathf.Atan(lateralSpeed / speed) * Mathf.Rad2Deg);
                }
                if (rightOffset < 0){
                    transform.Rotate(roadOn.upNormal(currentParam), Mathf.Atan(lateralSpeed / speed) * Mathf.Rad2Deg);
                }

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
        laneOn = 0;
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
        laneOn = 0;
        rightOffset = 0f;
    }

    public void ShiftLane(bool right){
        int newLane;
        newLane = right ? Mathf.Min(pathOn.getRoadOfSeg(currentSeg).validLaneCount - 1, laneOn + 1) :
                              Mathf.Max(0, laneOn - 1);
            
        Road roadOn = pathOn.getRoadOfSeg(currentSeg);
        int laneOnByRoad = pathOn.getHeadingOfCurrentSeg(currentSeg) ? laneOn : roadOn.validLaneCount - laneOn - 1;
        int newlaneOnByRoad = pathOn.getHeadingOfCurrentSeg(currentSeg) ? newLane : roadOn.validLaneCount - newLane - 1;

        rightOffset = pathOn.getHeadingOfCurrentSeg(currentSeg) ?
                            roadOn.getLaneCenterOffset(laneOnByRoad) - roadOn.getLaneCenterOffset(newlaneOnByRoad) :
                            roadOn.getLaneCenterOffset(newlaneOnByRoad) - roadOn.getLaneCenterOffset(laneOnByRoad);

        laneOn = newLane;
    }

    public void Accelerate(float a){
        speed += a * Time.deltaTime;
    }
}
