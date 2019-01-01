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
        forwardVehicleController = new VehicleController(validLaneCount);
        backwardVehicleController = new VehicleController(validLaneCount);

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

    public float getLaneCenterOffset(int laneNum){
        if (laneNum < 0 || laneNum >= validLaneCount){
            Debug.Assert(false);
            return 0;
        }

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

    /*TODO: support "valid"*/
    public int validLaneCount{
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

    float margin0End, margin1End;
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
        if (param < margin0End)
        {
            return finder(0, param / margin0End);
        }
        else
        {
            if (param > margin1End)
            {
                return finder(2, (param - margin1End) / (1f - margin1End));
            }
            else
            {
                return finder(1, (param - margin0End) / (margin1End - margin0End));
            }
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

/*defines a path between two NODEs plus two additional segments if exists*/
public class Path
{
    List<Pair<Road, bool>> components;
    float startParam, endParam;
    Node sourceNode, destNode;

    public Path(Node source, List<Road> comp, Node dest){
        components = comp.ConvertAll((input) => new Pair<Road, bool>(input, true));
        for (int i = 0; i != components.Count; ++i){
            if (i == 0){
                if (Algebra.isclose(comp[0].curve.at(1f), source.position)){
                    components[0].Second = false;
                }
            }
            else{
                if (Algebra.isclose(comp[i].curve.at(0f), comp[i-1].curve.at_ending(!components[i-1].Second))){
                    components[i].Second = false;
                }
            }
        }
        startParam = endParam = Mathf.Infinity;
        sourceNode = source;
        destNode = dest;
    }

    public void insertAtStart(Road road, float param){
        Debug.Assert(float.IsPositiveInfinity(startParam));

        if (Algebra.isclose(road.curve.at(0f), sourceNode.position))
        {
            components.Insert(0, new Pair<Road, bool>(road, false));
        }
        else
        {
            Debug.Assert(Algebra.isclose(road.curve.at(1f), sourceNode.position));
            components.Insert(0, new Pair<Road, bool>(road, true));
        }

        startParam = param;
    }

    public void insertAtEnd(Road road, float param){
        Debug.Assert(float.IsPositiveInfinity(endParam));

            if (Algebra.isclose(road.curve.at(0f), destNode.position))
            {
                components.Add(new Pair<Road, bool>(road, true));
            }
            else
            {
                Debug.Assert(Algebra.isclose(road.curve.at(1f), destNode.position));
                components.Add(new Pair<Road, bool>(road, false));
            }
        endParam = param;
    }

    public float length{
        get{
            float NNLength = components.GetRange(1, components.Count - 2).Sum((Pair<Road, bool> arg1) => arg1.First.curve.length);

            if (components[0].First != components[components.Count - 1].First)
            {
                float startLength, endLength;
                if (components[0].Second)
                {
                    startLength = (startParam == 1) ? components[0].First.curve.split(startParam).Last().length : 0f;
                }
                else
                {
                    startLength = (startParam == 0) ? components[0].First.curve.split(startParam).First().length : 0f;
                }

                if (components[components.Count - 1].Second)
                {
                    endLength = (endParam == 0) ? components[components.Count - 1].First.curve.split(endParam).First().length : 0f;
                }
                else
                {
                    endLength = (endParam == 1) ? components[components.Count - 1].First.curve.split(endParam).Last().length : 0f;
                }
                return NNLength + startLength + endLength;
            }
            else{
                return components[0].First.curve.cut(startParam, endParam).length;
            }
        }
    }

    public override string ToString()
    {
        string str = components[0].First.curve.at(startParam) + " ==> ";

        if (components.Count > 2)
        {
            foreach (var component in components)
            {
                if (component != components.Last())
                {
                    str += component.First.curve.at_ending(!component.Second);
                    str += " ==> ";
                }
                else
                {
                    str += component.First.curve.at(endParam);
                }
            }
        }
        else{
            if (components[0].First != components[1].First){
                str += components[0].First.curve.at_ending(!components[0].Second);
                str += "==>";
            }
            str += components[1].First.curve.at(endParam);
        }
        return str;
    }


    public Pair<Road, float> travelAlong(int segnum, float param, float distToTravel, out int nextseg, out bool termination){
        //check whether to jump at the very beginning

        if (components[segnum].Second && Algebra.isclose(param, 1f) || (!components[segnum].Second) && Algebra.isclose(param, 0f))
        {
            segnum++;
            if (segnum == components.Count)
            {
                termination = true;
                nextseg = segnum;
                return null;
            }
            else
            {
                param = components[segnum].Second ? 0f : 1f;
            }
        }

        //Do not jump to second road
        var roadOn = components[segnum];
        float newParam = roadOn.First.curve.TravelAlong(param, distToTravel, roadOn.Second);
        termination = false;
        nextseg = segnum;
        return new Pair<Road, float>(roadOn.First, newParam);
    }

    public Road getRoadOfSeg(int segnum){
        return components[segnum].First;
    }

    public bool getHeadingOfCurrentSeg(int segnum){
        return components[segnum].Second;
    }

}