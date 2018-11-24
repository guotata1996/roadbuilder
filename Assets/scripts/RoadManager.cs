using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Road
{
    public Road(Curve _curve, List<string> _lane, GameObject _roadObj)
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
    }
    //To add: enterable for, walkable for
    public Curve curve;
    public List<string> laneconfigure;
    public GameObject roadObject;
    internal bool virtualRoad{
        get{
            return laneconfigure.Count == 0;
        }
    }

    public float width{
        get{
            return RoadRenderer.getConfigureWidth(laneconfigure);
        }
    }
}

public class RoadManager : MonoBehaviour
{
    public GameObject road;

    public GameObject intersectIndicator;

    public GameObject nodePrefab;

    static List<Road> allroads = new List<Road>();

    static Dictionary<Vector3, Node> allnodes = new Dictionary<Vector3, Node>();

    public const float ApproxLimit = 3f;

    public void addRoad(Curve curve, List<string> laneConfigure)
    {
        List<Vector3> allNewIntersectPoints = new List<Vector3>();

        foreach (Road oldroad in allroads.ToList())
        {
            List<Vector3> intersectPoints = Geometry.curveIntersect(curve, oldroad.curve);

            foreach (Vector3 point in intersectPoints){
                Debug.Log("Interset points btw curve " + curve + " \nand "+ oldroad.curve + " is " + string.Format("{0:C4},{1:C4},{2:C4}", point.x, point.y, point.z));
            }
            if (intersectPoints.Count > 0)
            {
                allNewIntersectPoints.AddRange(intersectPoints);
                //remove oldroad
                removeRoadWithoutChangingNodes(oldroad);
                //Add new fragments
                List<float> intersectParamsWithBeginAndEnd = interSectPoints2Fragments(intersectPoints, oldroad.curve);
                addAllFragments(intersectParamsWithBeginAndEnd, oldroad.curve, oldroad.laneconfigure);
            }
        }

        allNewIntersectPoints = allNewIntersectPoints.Distinct(new IntersectPointComparator()).ToList();
        List<float> intersectParamsWithBeginAndEnd1 = interSectPoints2Fragments(allNewIntersectPoints, curve);

        /*TODO: add to UI*/
        List<string> modifiedLaneConfigure = laneConfigure.ConvertAll((input) => input);
        modifiedLaneConfigure.Add("column");
        modifiedLaneConfigure.Insert(0, "fence");
        modifiedLaneConfigure.Add("fence");

        addAllFragments(intersectParamsWithBeginAndEnd1, curve, modifiedLaneConfigure);

        /*TODO revise*/

        foreach(Road r in allroads.ToList()){
            updateMargin(r);
        }

    }

    private List<float> interSectPoints2Fragments(List<Vector3> intersectPoints, Curve originalCurve)
    {
        var intersectParams = from intersectNode in intersectPoints
            select (float)originalCurve.paramOf(Algebra.toVector2(intersectNode));
        List<float> intersectParams1 = intersectParams.ToList();

        intersectParams1.Sort();

        if (intersectParams1.Count == 0 || !Algebra.isclose(0f, intersectParams1.First()))
        {
            intersectParams1.Insert(0, 0f);
        }
        if (!Algebra.isclose(1f, intersectParams1.Last()))
        {
            intersectParams1.Add(1f);
        }
        return intersectParams1;
    }

    private void addAllFragments(List<float> intersectParams, Curve curve, List<string> laneConfigure)
    {
        for (int i = 0; i != intersectParams.Count - 1; ++i)
        {
            addPureRoad(curve.cut(intersectParams[i], intersectParams[i + 1], byLength: false), laneConfigure);
        }
    }

    private Road addPureRoad(Curve curve, List<string> laneConfigure)
    {
        GameObject roadInstance = Instantiate(road, transform);
        RoadRenderer roadConfigure = roadInstance.GetComponent<RoadRenderer>();
        roadConfigure.generate(curve, laneConfigure);
        Road newRoad = new Road(curve, laneConfigure, roadInstance);
        allroads.Add(newRoad);
        createOrAddtoNode(newRoad);
        Debug.Log(curve + " added");
        return newRoad;
    }

    /* only destroys road obj, and remove it from record/nodes
     * not changing other properties of node
     */ 
    private void removeRoadWithoutChangingNodes(Road road)
    {
        allroads.Remove(road);
        Node startNode, endNode;
        findNodeAt(road.curve.at(0f), out startNode);
        startNode.removeRoad(road);

        findNodeAt(road.curve.at(1f), out endNode);

        endNode.removeRoad(road);

        Destroy(road.roadObject);
        Debug.Log(road.curve + " removed!");

    }

    private void createOrAddtoNode(Road road)
    {
        List<Vector3> roadEnds = new List<Vector3> { road.curve.at(0f), road.curve.at(1f) };
        foreach (Vector3 roadEnd in roadEnds)
        {
            
            Node candidate;
            if (!findNodeAt(roadEnd, out candidate))
            {
                candidate = Instantiate(nodePrefab, transform).GetComponent<Node>();
                candidate.position = roadEnd;
                allnodes.Add(Algebra.approximate(roadEnd), candidate);
            }
            candidate.addRoad(road);
        }
    }

    public bool findNodeAt(Vector3 position, out Node rtn)
    {
        Vector3 approx = Algebra.approximate(position);

        if (allnodes.ContainsKey(approx)){
            rtn = allnodes[approx];
            return true;
        }
        else{
            rtn = null;
            return false;
        }
    }

    /*TODO treat additional/existing differenly by height*/
    public Vector3 approxNodeToExistingRoad(Vector3 p, out Road match, List<Curve> additionalInterestedLines = null){
        List<Road> allInterestedRoad;
        if (additionalInterestedLines != null){
            allInterestedRoad = new List<Road>();
            allInterestedRoad.AddRange(allroads);
            allInterestedRoad.AddRange(additionalInterestedLines.ConvertAll<Road>((Curve input) => new Road(input, null, null)));
        }
        else{
            allInterestedRoad = allroads;
        }
        //Debug.Assert(additionalInterestedLines == null || additionalInterestedLines.All((arg1) => arg1 is Line));

        List<Road> candidates = allInterestedRoad.FindAll(r => (r.curve.AttouchPoint(p) - p).magnitude <= ApproxLimit);

        List<Road> onlyRoadCandidates = candidates.FindAll((obj) => !obj.virtualRoad);

        if (onlyRoadCandidates.Count > 0)
        {
            Road bestMatch = onlyRoadCandidates.OrderBy(r => (r.curve.AttouchPoint(p) - p).magnitude).First();
            match = bestMatch;
            foreach (Road others in candidates)
            {
                if (others != bestMatch)
                {
                    List<Vector3> interPoints = Geometry.curveIntersect(bestMatch.curve, others.curve);
                    foreach(Vector3 point in interPoints){
                        if ((point - p).magnitude <= ApproxLimit){
                            return point;
                        }
                    }
                }
            }
            return bestMatch.curve.AttouchPoint(p);

        }

        if (candidates.Count > 0)
        {
            match = null;
            return candidates[0].curve.AttouchPoint(p);
        }

        match = null;
        return p;
    }

    public void updateMargin(Road r)
    {
        Destroy(r.roadObject);
        GameObject roadInstance = Instantiate(road, transform);
        RoadRenderer roadConfigure = roadInstance.GetComponent<RoadRenderer>();
        r.roadObject = roadInstance;
        Node n0, n1, startNode, endNode;
        findNodeAt(r.curve.at(0f), out n0);
        findNodeAt(r.curve.at(1f), out n1);

        if (n0.startof(r.curve)){
            startNode = n0;
            endNode = n1;
        }
        else{
            startNode = n1;
            endNode = n0;
        }

        Debug.Log(r.curve + " 0L= " + startNode.getMargin(r).First + " 0R= " + startNode.getMargin(r).Second + "\n1L="
                  + endNode.getMargin(r).First + " 1R= " + endNode.getMargin(r).Second);


        roadConfigure.generate(r.curve, r.laneconfigure,
                               startNode.getMargin(r).First, startNode.getMargin(r).Second,
                               endNode.getMargin(r).First, endNode.getMargin(r).Second);
    }

    public void deleteRoad(Road r){
        removeRoadWithoutChangingNodes(r);
        Node nstart, nend;
        findNodeAt(r.curve.at(0f), out nstart);
        findNodeAt(r.curve.at(1f), out nend);

        Node[] affectedNides = { nstart, nend };
        foreach (Node n in affectedNides.ToList()){
            if (n.connection.Count == 0){
                Destroy(allnodes[Algebra.approximate(n.position)].gameObject);
                allnodes.Remove(Algebra.approximate(n.position));
            }
            else{
                if (n.connection.Count == 2){
                    if (Geometry.sameMotherCurveUponIntersect(n.connection[0].First.curve, n.connection[1].First.curve)){
                        Road r1 = n.connection[0].First;
                        Road r2 = n.connection[1].First;
                        removeRoadWithoutChangingNodes(r1);
                        removeRoadWithoutChangingNodes(r2);
                        Destroy(allnodes[Algebra.approximate(n.position)].gameObject);
                        allnodes.Remove(Algebra.approximate(n.position));
                        Curve c2 = r1.curve.concat(r2.curve);
                        addPureRoad(c2, r1.laneconfigure); //TODO: deal with different lane configure
                    }
                }
            }
        }
    }
}