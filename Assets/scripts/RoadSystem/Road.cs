using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Road
{
    public Road(Curve _curve, List<string> _lane, GameObject _roadObj = null)
    {
        curve = _curve;
        laneconfigure = new List<string>();
        if (_lane != null)
        {
            foreach (string l in _lane)
            {
                laneconfigure.Add(l);
            }
        }
        roadObject = _roadObj;
        forwardVehicleController = new VehicleController(validLaneCount(true));
        backwardVehicleController = new VehicleController(validLaneCount(false));

        setMargins(0f, 0f, 0f, 0f);
    }
    public Curve curve;
    public List<string> laneconfigure;
    public GameObject roadObject;
    internal bool virtualRoad
    {
        get
        {
            return laneconfigure.Count == 0;
        }
    }

    public float width
    {
        get
        {
            return RoadRenderer.getConfigureWidth(laneconfigure);
        }
    }

    public float getLaneCenterOffset(int laneNum, bool direction){
        if (laneNum < 0 || laneNum >= validLaneCount(direction))
        {
            Debug.Assert(false);
            return 0;
        }

        laneNum = direction ? totalLaneCount - 1 - laneNum : laneNum;
        for (int foundLanes = 0, j = 0; foundLanes <= laneNum; j++){
            if (laneconfigure[j] == "lane"){
                foundLanes++;
            }
            if (foundLanes == laneNum + 1){
                return RoadRenderer.getConfigureWidth(laneconfigure.GetRange(0, j)) +
                                   0.5f * RoadRenderer.getConfigureWidth(laneconfigure.GetRange(j, 1)) -
                                   0.5f * width;
            }
        }

        Debug.Assert(false);
        return 0;
    }

    public int validLaneCount(bool direction){
        int mainSeparatorIndex = laneconfigure.FindIndex((obj) => obj.EndsWith("yellow"));
        if (mainSeparatorIndex == -1){
            /*if no yellow line, suppose road's dir is same with curve*/
            return direction ? totalLaneCount : 0;
        }
        else{
            return direction ?
                laneconfigure.GetRange(mainSeparatorIndex, laneconfigure.Count - mainSeparatorIndex - 1).Count(config => config == "lane") :
                             laneconfigure.GetRange(0, mainSeparatorIndex).Count(config => config == "lane");
                            
        }
    }

    int totalLaneCount{
        get{
            return laneconfigure.Count(config => config == "lane");
        }
    }

    public float SPWeight{
        get
        {
            return curve.length;
        }
    }

    /*actual render info for vehicle*/

    public float margin0End, margin1End;
    Curve[] renderingFragements;

    public void setMargins(float _margin0L, float _margin0R, float _margin1L, float _margin1R){
        float indicatorMargin0Bound = Mathf.Max(_margin0L, _margin0R);
        float indicatorMargin1Bound = Mathf.Max(_margin1L, _margin1R);
        renderingFragements = RoadRenderer.splitByMargin(curve, indicatorMargin0Bound, indicatorMargin1Bound);
        if (renderingFragements[0] != null){
            margin0End = (float)curve.paramOf(renderingFragements[0].at(1f));
        }
        else{
            margin0End = 0;
        }
        if (renderingFragements[2] != null){
            margin1End = (float)curve.paramOf(renderingFragements[2].at(0f));
        }
        else{
            margin1End = 1;
        }
    }

    delegate Vector3 curveValueFinder(int id, float p);

    Vector3 renderingCurveSolver(float param, curveValueFinder finder){
        Debug.Assert(renderingFragements != null);
        if (param < margin0End && renderingFragements[0] != null)
        {
            return finder(0, param / margin0End);
        }
        else
        {
            if (param > margin1End && renderingFragements[2] != null)
            {
                return finder(2, (param - margin1End) / (1f - margin1End));
            }
            else
            {
                return finder(1, (param - margin0End) / (margin1End - margin0End));
            }
        }
    }

    public Curve marginedOutCurve{
        get{
            return renderingFragements[1];
        }
    }

    public Vector3 at(float param){
        return renderingCurveSolver(param, at_finder);
    }

    public Vector3 frontNormal(float param){
        return renderingCurveSolver(param, frontNormal_finder);
    }

    public Vector3 upNormal(float param){
        return renderingCurveSolver(param, upNormal_finder);
    }

    public Vector3 rightNormal(float param){
        return renderingCurveSolver(param, rightNormal_finder);
    }

    Vector3 at_finder(int id, float p){
        return renderingFragements[id].at(p);
    }

    Vector3 frontNormal_finder(int id, float p){
        return renderingFragements[id].frontNormal(p);
    }

    Vector3 upNormal_finder(int id, float p){
        return renderingFragements[id].upNormal(p);
    }

    Vector3 rightNormal_finder(int id, float p){
        return renderingFragements[id].rightNormal(p);
    }


    public VehicleController forwardVehicleController, backwardVehicleController;

}

