using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    float startParam;
    Road startRoad;
    /*dynamic Path info*/
    public Path pathOn;
    int currentSeg;
    float currentParam;
    public float distTraveledOnSeg;  //always inc from 0->length of seg

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
    readonly float lateralSpeed = 3f;
    public float bodyLength = 3.9f;

    RoadDrawing drawing;
    bool isshowingPath;

    public GameObject roadIndicatorPrefab;

    public delegate void MyDel();

    public event MyDel stopEvent;

    void Awake()
    {
        Reset();
        drawing = GameObject.Find("curveIndicator").GetComponent<RoadDrawing>();
    }

    public int LaneOn{
        get{
            return laneOn;
        }
    }

    bool headingOfCurrentSeg{
        get{
            return pathOn.GetHeadingOfSeg(currentSeg);
        }
    }
	
    Road roadOfCurrentSeg{
        get{
            return pathOn.GetRoadOfSeg(currentSeg);
        }
    }

    public int? correspondingLaneOfNextSeg{
        get{
            return pathOn.getCorrespondingLaneOfNextSeg(currentSeg, laneOn);
        }
    }

    public int? correspondingLaneOfPrevSeg{
        get{
            return pathOn.getCorrespondingLaneOfPrevSeg(currentSeg, laneOn);
        }
    }

    float lengthOfCurrentSeg{
        get{
            return pathOn.getTotalLengthOfSeg(currentSeg);
        }
    }

    public float distTowardsEndOfSeg{
        get{
            return lengthOfCurrentSeg - distTraveledOnSeg;
        }
    }

    public bool onLastSeg{
        get{
            return currentSeg == pathOn.SegCount - 1;
        }
    }

    public Pair<int, int> outgoingLaneRangeOfCurrentSeg{
        get{
            return pathOn.getOutgoingLaneRangeOfSeg(currentSeg);
        }
    }

    public bool isChangingLane{
        get{
            return !Algebra.isclose(rightOffset, 0f);
        }
    }

    /*if I can enter next seg on current lane, return 0
    * If I have to R-shift before crossroads, return -(minimum # of shifts)
    * If I have to L-shift before crossroads, return +(minimum # of shifts)
    */
    public int laneChangingPreference{
        get{
            if (laneOn < outgoingLaneRangeOfCurrentSeg.First){
                return outgoingLaneRangeOfCurrentSeg.First - laneOn;
            }
            else{
                if (laneOn > outgoingLaneRangeOfCurrentSeg.Second){
                    return outgoingLaneRangeOfCurrentSeg.Second - laneOn;
                }
                else{
                    return 0;
                }
            }
        }
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
            int nextSeg, nextLane;
            Pair<Road, float> nextInfo = pathOn.travelAlong(currentSeg, currentParam, distToTravel, laneOn, out nextSeg, out nextLane, out termination);

            if (termination)
            {
                Debug.Log("termination");
                VhCtrlOfCurrentSeg.VehicleLeave(this, laneOn);
                stopEvent.Invoke();

                Reset();
                return;
            }

            Road roadOn = nextInfo.First;
            currentParam = nextInfo.Second;

            if (currentSeg != nextSeg){
                VhCtrlOfCurrentSeg.VehicleLeave(this, laneOn);
                distTraveledOnSeg = distToTravel;
                laneOn = nextLane;
                currentSeg = nextSeg;

                VhCtrlOfCurrentSeg.VehicleEnter(this, laneOn);

            }

            rightOffset = Mathf.Sign(rightOffset) * Mathf.Max(Mathf.Abs(rightOffset) - lateralSpeed * Time.deltaTime, 0f);

            transform.position = roadOn.at(currentParam) +
                roadOn.rightNormal(currentParam) * (roadOn.getLaneCenterOffset(laneOn, headingOfCurrentSeg) + rightOffset);

            transform.rotation = headingOfCurrentSeg ?
                Quaternion.LookRotation(roadOn.frontNormal(currentParam), roadOn.upNormal(currentParam)) :
                Quaternion.LookRotation(-roadOn.frontNormal(currentParam), roadOn.upNormal(currentParam));

            if (rightOffset != 0f)
            {
                if (headingOfCurrentSeg)
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
                   
        }
	}

    public void SetStart(Vector3 position){
        Vector3 modifiedPosition = drawing.roadManager.approxNodeToExistingRoad(position, out startRoad);
        startParam = currentParam = (float)startRoad.curve.paramOf(modifiedPosition);
        laneOn = 0;
    }

    public void SetDest(Vector3 position, bool randomizeLane = false, float initialSpeed = 0f){
        Road endRoad;
        Debug.Assert(startRoad != null);
        Vector3 modifiedPosition = drawing.roadManager.approxNodeToExistingRoad(position, out endRoad);
        float endParam = (float)endRoad.curve.paramOf(modifiedPosition);
        pathOn = drawing.roadManager.findPath(startRoad, startParam, endRoad, endParam);

        if (pathOn == null){
            Debug.LogWarning("Dest not reachable !");
            return;
        }

        laneOn = randomizeLane ? Random.Range(0, roadOfCurrentSeg.validLaneCount(headingOfCurrentSeg)) : 0;
        VhCtrlOfCurrentSeg.VehicleEnter(this, laneOn);

        speed = initialSpeed;
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
    }

    public void Abort(){
        VhCtrlOfCurrentSeg.VehicleLeave(this, LaneOn);
    }

    public void ShiftLane(bool right){
        int newLane;
        newLane = right ? Mathf.Max(0, laneOn - 1) :
                               Mathf.Min(roadOfCurrentSeg.validLaneCount(headingOfCurrentSeg) - 1, laneOn + 1);

        rightOffset += roadOfCurrentSeg.getLaneCenterOffset(laneOn, headingOfCurrentSeg) -
                            roadOfCurrentSeg.getLaneCenterOffset(newLane, headingOfCurrentSeg);

        laneOn = newLane;
    }

    public void toggleRouteView(){
        isshowingPath = !isshowingPath;

        var path = pathOn.getCurveRepresentation();
        foreach (var c in path)
        {
            if (isshowingPath)
            {
                drawing.highLightRoad(c);
                stopEvent += delegate {
                    drawing.deHighLightRoad(c);
                };
            }
            else
            {
                drawing.deHighLightRoad(c);
                stopEvent = stopEvent - delegate
                {
                    drawing.deHighLightRoad(c);
                };
            }
        }


    }

    public VehicleController VhCtrlOfPrevSeg
    {
        get
        {
            return currentSeg == 0 ? null : pathOn.GetVhCtrlOfSeg(currentSeg - 1);
        }
    }

    public VehicleController VhCtrlOfNextSeg
    {
        get{
            return currentSeg == pathOn.SegCount - 1 ? null : pathOn.GetVhCtrlOfSeg(currentSeg + 1);
        }
    }

    public VehicleController VhCtrlOfCurrentSeg{
        get{
            return pathOn.GetVhCtrlOfSeg(currentSeg);
        }
    }

}
