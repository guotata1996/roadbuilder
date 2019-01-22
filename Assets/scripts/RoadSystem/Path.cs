using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Path
{
    List<Pair<Road, bool>> components;
    float startParam, endParam;
    Node sourceNode, destNode;

    /*EndNode[i] = the node following segment i*/
    Dictionary<Road, Node> EndNodes;

    public Path(List<Node> passingNodes, List<Road> comp)
    {
        Debug.Assert(passingNodes.Count == comp.Count + 1);

        components = new List<Pair<Road, bool>>();
        EndNodes = new Dictionary<Road, Node>();

        for (int i = 0; i != comp.Count; ++i)
        {
            if (Algebra.isclose(passingNodes[i].position, comp[i].at(0f)))
            {
                components.Add(new Pair<Road, bool>(comp[i], true));
            }
            else
            {
                components.Add(new Pair<Road, bool>(comp[i], false));

            }
            EndNodes[comp[i]] = passingNodes[i + 1];

            if (i != comp.Count - 1)
            {
                Road virtualNodePath = passingNodes[i + 1].getVirtualRoad(comp[i], comp[i + 1]);
                if (virtualNodePath != null)
                {
                    components.Add(new Pair<Road, bool>(virtualNodePath, true));
                    EndNodes[virtualNodePath] = passingNodes[i + 1];
                }
            }

        }
        startParam = endParam = Mathf.Infinity;
        sourceNode = passingNodes.First();
        destNode = passingNodes.Last();
    }

    /*trivial case*/
    public Path(Road r, float startP, float endP)
    {
        components = new List<Pair<Road, bool>>();
        components.Add(new Pair<Road, bool>(r, endP > startP));
        startParam = startP;
        endParam = endP;
    }

    public void insertAtStart(Road road, float param)
    {
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
        EndNodes[road] = sourceNode;

        if (components.Count > 1 && road != components[1].First)
        {
            //otherwise, must not be SP
            Road virtualNodePath = sourceNode.getVirtualRoad(road, components[1].First);
            if (virtualNodePath != null)
            {
                components.Insert(1, new Pair<Road, bool>(virtualNodePath, true));
                EndNodes[virtualNodePath] = sourceNode;
            }
        }
        startParam = param;
    }

    public void insertAtEnd(Road road, float param)
    {
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

        if (components.Count > 1 && road != components[components.Count - 2].First)
        {
            //otherwise, must not be SP
            Road virtualNodePath = destNode.getVirtualRoad(components[components.Count - 2].First, road);
            if (virtualNodePath != null)
            {
                components.Insert(components.Count - 1, new Pair<Road, bool>(virtualNodePath, true));
                EndNodes[virtualNodePath] = destNode;
            }
        }
        endParam = param;
    }

    public float length
    {
        get
        {
            float len = 0f;
            for (int i = 0; i != SegCount; ++i){
                len += getTotalLengthOfSeg(i);
            }
            return len;
        }
    }

    public override string ToString()
    {
        string rtn = "";
        rtn += "Vroad count " + components.Count(a => a.First.noEntity);
        rtn += " ;road count " + components.Count(a => !a.First.noEntity);
        return rtn;
    }

    public int? getCorrespondingLaneOfNextSeg(int segnum, int lane){
        if (segnum >= SegCount - 1){
            /*this is the very last segment*/
            return null;
        }
        Node refNode = EndNodes[components[segnum].First];

        if (components[segnum].First.noEntity){
            //leaving a crossing
            int ValidLaneStart = refNode.getValidOutRoadLanes(components[segnum - 1].First, components[segnum + 1].First).First;
            return lane + ValidLaneStart;
        }
        else{
            if (components[segnum + 1].First.noEntity)
            {
                //entering a crossing
                int laneNumInValidLanes = lane - refNode.getValidInRoadLanes(components[segnum].First, components[segnum + 2].First).First;
                if (laneNumInValidLanes < 0 || (laneNumInValidLanes > components[segnum + 1].First.validLaneCount(true) - 1)){
                    return null;
                }
                else{
                    return laneNumInValidLanes;
                }                                                              
            }
            else
            {
                return lane;
            }
        }
    }

    public int ? getCorrespondingLaneOfPrevSeg(int segnum, int lane){
        if (segnum == 0){
            /*this is the very first segment*/
            return null;
        }
        Node refNode = EndNodes[components[segnum - 1].First];
        if (components[segnum - 1].First.noEntity){
            //at the end of prev seg, leaving a crossing
            int laneNumInValidLanes = lane - refNode.getValidOutRoadLanes(components[segnum - 2].First, components[segnum].First).First;
            if (laneNumInValidLanes < 0 || laneNumInValidLanes > components[segnum - 1].First.validLaneCount(true) - 1){
                return null;
            }
            else{
                return laneNumInValidLanes;
            }
        }
        else{
            if (components[segnum].First.noEntity){
                //at the end of prev seg, entering a crossing
                int validLaneStart = refNode.getValidInRoadLanes(components[segnum - 1].First, components[segnum + 1].First).First;
                return lane + validLaneStart;
            }
            else{
                return lane;
            }
        }
    }
     
    public Pair<int, int> getOutgoingLaneRangeOfSeg(int segnum){
        if (segnum == SegCount - 1 || components[segnum].First.noEntity)
        {
            return new Pair<int, int>(0, GetRoadOfSeg(segnum).validLaneCount(GetHeadingOfSeg(segnum)) - 1);
        }
        else{
            Node refNode = EndNodes[components[segnum].First];
            if (components[segnum + 1].First.noEntity)
            {
                return refNode.getValidInRoadLanes(GetRoadOfSeg(segnum), GetRoadOfSeg(segnum + 2));
            }
            else
            {
                return refNode.getValidInRoadLanes(GetRoadOfSeg(segnum), GetRoadOfSeg(segnum + 1));
            }
        }
    }

    public Road GetRoadOfSeg(int segnum)
    {
        return components[segnum].First;
    }

    public bool GetHeadingOfSeg(int segnum)
    {
        return components[segnum].Second;
    }

    public VehicleController GetVhCtrlOfSeg(int segnum){
        return GetHeadingOfSeg(segnum) ?
            GetRoadOfSeg(segnum).forwardVehicleController : GetRoadOfSeg(segnum).backwardVehicleController;
    }

    public int SegCount{
        get{
            return components.Count;
        }
    }

    public float getTotalLengthOfSeg(int segNum){
        if (SegCount == 1){
            return components[0].First.curve.cutByParam(Mathf.Min(startParam, endParam), Mathf.Max(startParam, endParam)).length;
        }
        /*non-trivial case*/
        if (segNum == 0){
            return components[0].Second ?
                                components[0].First.curve.cutByParam(startParam, components[0].First.margin1End).length :
                                components[0].First.curve.cutByParam(components[0].First.margin0End, startParam).length;
        }
        if (segNum == SegCount - 1){
            return components[SegCount - 1].Second ?
                                           components[SegCount - 1].First.curve.cutByParam(components[SegCount - 1].First.margin0End, endParam).length :
                                           components[SegCount - 1].First.curve.cutByParam(endParam, components[SegCount - 1].First.margin1End).length;
        }

        return components[segNum].First.marginedOutCurve.length;
    }

    public bool Valid{
        get{
            return !(EndNodes != null && EndNodes.Any((n) => n.Value == null) ||
                     components.Any(c => !c.First.noEntity && c.First.roadObject == null));
        }
    }

    public Pair<Road, float> travelAlong(int segnum, float param, float distToTravel, int lane, out int nextseg, out int nextLane, out bool termination)
    {
        //chech validity of path in case any seg is deleted
        if (!Valid){
            termination = true;
            nextseg = segnum;
            nextLane = 0;
            return null;
        }

        //check whether to jump segs
        if (components[segnum].Second && (param >= components[segnum].First.margin1End) ||
            (!components[segnum].Second) && (param <= components[segnum].First.margin0End) ||
            (segnum == components.Count - 1 && components[segnum].Second && param >= endParam) ||
            (segnum == components.Count - 1 && !components[segnum].Second && param <= endParam))
        {
            segnum++;
            if (segnum == components.Count)
            {
                termination = true;
                nextseg = segnum;
                nextLane = 0;
                return null;
            }
            else
            {
                param = components[segnum].Second ? components[segnum].First.margin0End : components[segnum].First.margin1End;

                Node refNode = EndNodes[components[segnum - 1].First];
                if (components[segnum].First.noEntity)
                {
                    //enter a crossing
                    int laneNumInValidLanes = lane - refNode.getValidInRoadLanes(components[segnum - 1].First, components[segnum + 1].First).First;
                    nextLane = Mathf.Clamp(laneNumInValidLanes, 0, components[segnum].First.validLaneCount(true) - 1);
                }
                else
                {
                    if (components[segnum - 1].First.noEntity)
                    {
                        //leave a crossing
                        int ValidLanesStart = refNode.getValidOutRoadLanes(components[segnum - 2].First, components[segnum].First).First;
                        nextLane = lane + ValidLanesStart;
                    }
                    else
                    {
                        //no virtualroad at this crossing
                        nextLane = lane;
                    }
                }
            }
        }
        else
        {
            nextLane = lane;
        }

        //Do not jump to seg 
        var roadOn = components[segnum];
        float newParam = roadOn.First.curve.TravelAlong(param, distToTravel, roadOn.Second);
        termination = false;
        nextseg = segnum;

        return new Pair<Road, float>(roadOn.First, newParam);
    }

    /* derived property */
    List<Pair<Curve, float>> curveRepresentation;

    public List<Pair<Curve, float>> getCurveRepresentation()
    {

        if (curveRepresentation == null)
        {
            curveRepresentation = new List<Pair<Curve, float>>();

            foreach(var c in components){
                Curve mainCurve = c.First.marginedOutCurve;
                for (int i = 0; i != c.First.validLaneCount(c.Second); ++i){
                    if (c.Second)
                    {
                        curveRepresentation.Add(new Pair<Curve, float>(mainCurve, c.First.getLaneCenterOffset(i, c.Second)));
                    }
                    else{
                        curveRepresentation.Add(new Pair<Curve, float>(mainCurve.reversed(), -c.First.getLaneCenterOffset(i, c.Second)));
                    }
                }
            }
        }

        return curveRepresentation;
    }

}
