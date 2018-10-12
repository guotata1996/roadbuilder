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
        foreach(string l in _lane){
            laneconfigure.Add(l);
        }
        roadObject = _roadObj;
    }
    //To add: enterable for, walkable for
    public Curve curve;
    public List<string> laneconfigure;
    public GameObject roadObject;
    public float width{
        get{
            var ans = 0f;
            for (int i = 0; i != laneconfigure.Count; ++i){
                if (laneconfigure[i] == "lane")
                    ans += RoadRenderer.laneWidth;
                else{
                    if (laneconfigure[i] == "interval"){
                        ans += RoadRenderer.separatorInterval;
                    }
                    else{
                        ans += RoadRenderer.separatorWidth;
                    }
                }
            }
            return ans;
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
        List<Vector2> allNewIntersectPoints = new List<Vector2>();

        foreach (Road oldroad in allroads.ToList())
        {
            List<Vector2> intersectPoints = Geometry.curveIntersect(curve, oldroad.curve);

            foreach (Vector2 point in intersectPoints){
                Debug.Log("Interset points btw curve " + curve + " \nand "+ oldroad.curve + " is " + point);
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

        allNewIntersectPoints = allNewIntersectPoints.Distinct(new Vector2Comparator()).ToList();
        List<float> intersectParamsWithBeginAndEnd1 = interSectPoints2Fragments(allNewIntersectPoints, curve);
        addAllFragments(intersectParamsWithBeginAndEnd1, curve, laneConfigure);

        /*TODO revise*/
        foreach(Road r in allroads.ToList()){
            updateMargin(r);
        }
    }

    private List<float> interSectPoints2Fragments(List<Vector2> intersectPoints, Curve originalCurve)
    {
        var intersectParams = from intersectNode in intersectPoints
            select (float)originalCurve.paramOf(intersectNode);
        List<float> intersectParams1 = intersectParams.ToList();

        intersectParams1.Sort();
        if (intersectParams1.Count == 0 || !Mathf.Approximately(0f, intersectParams1.First()))
        {
            intersectParams1.Insert(0, 0f);
        }
        if (!Mathf.Approximately(1f, intersectParams1.Last()))
        {
            intersectParams1.Add(1f);
        }
        return intersectParams1;
    }

    private void addAllFragments(List<float> intersectParams, Curve curve, List<string> laneConfigure)
    {
        for (int i = 0; i != intersectParams.Count - 1; ++i)
        {
            addPureRoad(curve.cut(intersectParams[i], intersectParams[i + 1]), laneConfigure);
        }
    }

    private Road addPureRoad(Curve curve, List<string> laneConfigure)
    {
        GameObject roadInstance = Instantiate(road, transform);
        RoadRenderer roadConfigure = roadInstance.GetComponent<RoadRenderer>();
        roadConfigure.generate(curve, laneConfigure, indicator:false);
        Road newRoad = new Road(curve, laneConfigure, roadInstance);
        allroads.Add(newRoad);
        createOrAddtoNode(newRoad);
        return newRoad;
    }

    /* only destroys road obj, and remove it from record/nodes
     * not changing other properties of node
     */ 
    private void removeRoadWithoutChangingNodes(Road road)
    {
        allroads.Remove(road);
        Node startNode, endNode;
        bool startNodeV = findNodeAt(road.curve.at(0f), out startNode);
        startNode.removeRoad(road);

        bool endNodeV = findNodeAt(road.curve.at(1f), out endNode);

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

    bool findNodeAt(Vector3 position, out Node rtn)
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

    public Vector2 approNodeToExistingRoad(Vector2 p, out Road match){
        List<Road> candidates = allroads.FindAll(r => (r.curve.AttouchPoint(p) - p).magnitude <= ApproxLimit);
        if (candidates.Count > 1){
            Road bestMatch = candidates.OrderBy(r => (r.curve.AttouchPoint(p) - p).magnitude).First();
            match = bestMatch;
            foreach(Road others in candidates){
                if (others != bestMatch)
                {
                    Node n0;
                    findNodeAt(bestMatch.curve.at(0f), out n0);
                    if (n0.containsRoad(others))
                        return bestMatch.curve.at_2d(0f);

                    Node n1;
                    findNodeAt(bestMatch.curve.at(1f), out n1);
                    if (n1.containsRoad(others))
                        return bestMatch.curve.at_2d(1f);
                }

            }
        }

        if (candidates.Count == 1)
        {
            match = candidates[0];
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
        Node n0, n1;
        findNodeAt(r.curve.at(0f), out n0);
        findNodeAt(r.curve.at(1f), out n1);
        roadConfigure.generate(r.curve, r.laneconfigure, 
                               n0.getMargin(r).First, n1.getMargin(r).First, n0.getMargin(r).Second, n1.getMargin(r).Second,
                               indicator:false);
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