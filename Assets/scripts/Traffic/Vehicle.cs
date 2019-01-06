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
    public float distTraveledOnSeg;  //always inc from 0->length of seg
    bool firstUpdate;

    /*lateral info
    laneOn: |   |   ||  |   |
            | 0   1 || 1  0 |
            |   |   ||  |   |
    */
    int laneOn;
    float rightOffset;

    /*longitudinal info*/
    public float speed;
    public float acceleration;
    float wheelRotation;
    readonly float wheeRadius = 0.14f;
    readonly float lateralSpeed = 2f;
    public float bodyLength = 3.9f;

    RoadDrawing drawing;
    bool isshowingPath;

    public GameObject roadIndicatorPrefab;

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

            float distToTravel;
            if (acceleration < 0f && speed < (-acceleration) * Time.deltaTime)
            {
                distToTravel = speed * speed / (2 * (-acceleration));
            }
            else
            {
                distToTravel = speed * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime;
            }
            speed += Time.deltaTime * acceleration;
            speed = Mathf.Max(0f, speed);

            distTraveledOnSeg += distToTravel;
            bool termination;
            int nextSeg;
            Pair<Road, float> nextInfo = pathOn.travelAlong(currentSeg, currentParam, distToTravel, out nextSeg, out termination);

            if (termination)
            {
                Debug.Log("termination");
                if (pathOn.getHeadingOfCurrentSeg(currentSeg))
                {
                    pathOn.getRoadOfSeg(currentSeg).forwardVehicleController.VehicleLeave(this, laneOn);
                }
                else
                {
                    pathOn.getRoadOfSeg(currentSeg).backwardVehicleController.VehicleLeave(this, laneOn);
                }

                Reset();
                return;
            }

            Road roadOn = nextInfo.First;
            currentParam = nextInfo.Second;

            if (firstUpdate || currentSeg != nextSeg){

                if (!firstUpdate)
                {
                    if (pathOn.getHeadingOfCurrentSeg(currentSeg))
                    {
                        pathOn.getRoadOfSeg(currentSeg).forwardVehicleController.VehicleLeave(this, laneOn);
                    }
                    else
                    {
                        pathOn.getRoadOfSeg(currentSeg).backwardVehicleController.VehicleLeave(this, laneOn);
                    }
                }


                if (pathOn.getHeadingOfCurrentSeg(nextSeg))
                {
                    roadOn.forwardVehicleController.VehicleEnter(this, laneOn);
                }
                else
                {
                    roadOn.backwardVehicleController.VehicleEnter(this, laneOn);
                }

                distTraveledOnSeg = distToTravel;

            }
            currentSeg = nextSeg;

            rightOffset = Mathf.Sign(rightOffset) * Mathf.Max(Mathf.Abs(rightOffset) - lateralSpeed * Time.deltaTime, 0f);

            transform.position = roadOn.at(currentParam) +
                roadOn.rightNormal(currentParam) * (roadOn.getLaneCenterOffset(laneOn, pathOn.getHeadingOfCurrentSeg(currentSeg)) + rightOffset);

            transform.rotation = pathOn.getHeadingOfCurrentSeg(currentSeg) ?
                Quaternion.LookRotation(roadOn.frontNormal(currentParam), roadOn.upNormal(currentParam)) :
                Quaternion.LookRotation(-roadOn.frontNormal(currentParam), roadOn.upNormal(currentParam));

            if (rightOffset != 0f)
            {
                if (pathOn.getHeadingOfCurrentSeg(currentSeg))
                {
                    transform.Rotate(roadOn.upNormal(currentParam), -Mathf.Sign(rightOffset) * Mathf.Atan(lateralSpeed / speed) * Mathf.Rad2Deg);
                }
                else{
                    transform.Rotate(roadOn.upNormal(currentParam), Mathf.Sign(rightOffset) * Mathf.Atan(lateralSpeed / speed) * Mathf.Rad2Deg);
                }
            }

            wheelRotation = (wheelRotation + distToTravel / wheeRadius * Mathf.Rad2Deg) % 360;
            /*TODO: calculate wheel radius*/
            transform.GetChild(0).GetChild(1).localRotation = transform.GetChild(0).GetChild(2).localRotation =
                transform.GetChild(0).GetChild(3).localRotation= transform.GetChild(0).GetChild(4).localRotation = 
                    Quaternion.Euler(wheelRotation, 0f, 0f);


            if (firstUpdate)
                firstUpdate = false;
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

        toggleRouteView();
    }

    private void Reset()
    {
        pathOn = null;
        currentParam = Mathf.Infinity;
        speed = acceleration = 0f;
        currentSeg = 0;
        distTraveledOnSeg = 0f;
        laneOn = 0;
        rightOffset = 0f;
        firstUpdate = true;
    }

    public void ShiftLane(bool right){
        int newLane;
        newLane = right ? Mathf.Max(0, laneOn - 1) :
                               Mathf.Min(pathOn.getRoadOfSeg(currentSeg).validLaneCount(pathOn.getHeadingOfCurrentSeg(currentSeg)) - 1, laneOn + 1);
        Road roadOn = pathOn.getRoadOfSeg(currentSeg);

        rightOffset = roadOn.getLaneCenterOffset(laneOn, pathOn.getHeadingOfCurrentSeg(currentSeg)) -
                            roadOn.getLaneCenterOffset(newLane, pathOn.getHeadingOfCurrentSeg(currentSeg));

        laneOn = newLane;
    }

    public void toggleRouteView(){
        isshowingPath = !isshowingPath;

        List<Curve> path = pathOn.getCurveRepresentation();
        foreach (Curve c in path)
        {
            if (isshowingPath)
            {
                drawing.highLightRoad(c);
            }
            else
            {
                drawing.deHighLightRoad(c);

            }
        }
    }
}
