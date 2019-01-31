using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using MoreLinq;
using Newtonsoft.Json;
using System.IO;

public class RoadManager : MonoBehaviour
{
    public GameObject road;

    public GameObject intersectIndicator;

    public GameObject nodePrefab;

    public List<Road> allroads = new List<Road>();

    static Dictionary<Vector3, Node> allnodes = new Dictionary<Vector3, Node>();

    public const float ApproxLimit = 3f;

    public void addRoad(Curve curve, List<string> laneConfigure)
    {
        if (laneConfigure.Count == 0){
            GameObject.FindWithTag("Logger").GetComponent<MessageLogger>().LogMessage("Please configure lanes first!");
            return;
        }

        List<Road> allroadsBackup = allroads.ConvertAll(input => input);

        List<Vector3> newIntersectPoints = new List<Vector3>();

        List<Vector3> affectedPoints = new List<Vector3>();

        foreach (Road oldroad in allroads.ToList())
        {
            List<Vector3> intersectPoints = Geometry.curveIntersect(curve, oldroad.curve);

            if (intersectPoints.Count > 0)
            {
                newIntersectPoints.AddRange(intersectPoints);
                //remove oldroad
                removeRoad(oldroad);
                //Add new fragments
                List<float> intersectParams = interSectPoints2Fragments(intersectPoints, oldroad.curve);
                addAllFragments(intersectParams, oldroad.curve, oldroad.laneconfigure);

                affectedPoints.AddRange(intersectPoints);
                affectedPoints.Add(oldroad.curve.at(0f));
                affectedPoints.Add(oldroad.curve.at(1f));
            }
        }

        newIntersectPoints = newIntersectPoints.Distinct(new IntersectPointComparator()).ToList();
        List<float> intersectParamsWithBeginAndEnd1 = interSectPoints2Fragments(newIntersectPoints, curve);

        addAllFragments(intersectParamsWithBeginAndEnd1, curve, laneConfigure);

        affectedPoints.Add(curve.at(0f));
        affectedPoints.Add(curve.at(1f));
        affectedPoints = affectedPoints.Distinct(new IntersectPointComparator()).ToList();

        List<Node> affectedNodes = affectedPoints.ConvertAll(delegate (Vector3 pos)
        {
            Node n;
            findNodeAt(pos, out n);
            return n;
        });

        try
        {
            foreach(Node n in affectedNodes){
                n.updateMargins();
            }

            foreach (Node n in affectedNodes){
                foreach (var r in n.connection){
                    createRoadObject(r);
                }
            }
        }
        catch{
            GameObject.FindWithTag("Logger").GetComponent<MessageLogger>().LogMessage("Intersections too close!");

            allroads = allroadsBackup;

            foreach(Node n in affectedNodes){
                n.reEstablishConnections(allroads);
            }

            foreach (var item in affectedNodes.Where(n => n.connection.Count == 0).ToList())
            {
                affectedNodes.Remove(item);
                var nodepos = allnodes.First(kvp => kvp.Value == item).Key;
                Debug.Assert(allnodes.Remove(nodepos));
            }

            foreach (Node n in affectedNodes)
            {
                foreach (var r in n.connection)
                {
                    createRoadObject(r);
                }
            }
        }

        foreach(Road r in allroads){
            Debug.Assert(r.marginedOutCurve != null);
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
            addPureRoad(curve.cutByParam(intersectParams[i], intersectParams[i + 1]), laneConfigure);
        }
    }

    private Road addPureRoad(Curve curve, List<string> laneConfigure)
    {
        Road newRoad = new Road(curve, laneConfigure);

        allroads.Add(newRoad);
        createOrAddtoNode(newRoad);
        //Debug.Log(curve + " added");
        return newRoad;
    }

    private void removeRoad(Road road)
    {
        allroads.Remove(road);
        Node startNode, endNode;
        findNodeAt(road.curve.at(0f), out startNode);
        startNode.removeRoad(road);

        findNodeAt(road.curve.at(1f), out endNode);

        endNode.removeRoad(road);

        Destroy(road.roadObject);
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

    /* For non-virtual(existing road), should consider height diff
    *  For virtual road, always flatten
    */
    public Vector3 approxNodeToExistingRoad(Vector3 p, out Road match, List<Curve> additionalInterestedLines = null){
        List<Road> allInterestedRoad;
        if (additionalInterestedLines != null){
            allInterestedRoad = new List<Road>();
            allInterestedRoad.AddRange(allroads);
            allInterestedRoad.AddRange(additionalInterestedLines.ConvertAll<Road>((Curve input) => new Road(input, null)));
        }
        else{
            allInterestedRoad = allroads;
        }
        //Debug.Assert(additionalInterestedLines == null || additionalInterestedLines.All((arg1) => arg1 is Line));

        List<Road> candidates = allInterestedRoad.FindAll(r => r.virtualRoad ? 
                                                          (Algebra.toVector2(r.curve.AttouchPoint(p)) - Algebra.toVector2(p)).magnitude <= ApproxLimit:
                                                          (r.curve.AttouchPoint(p) - p).magnitude <= ApproxLimit);

        List<Road> onlyRoadCandidates = candidates.FindAll((obj) => !obj.virtualRoad);

        if (onlyRoadCandidates.Count > 0)
        {
            Road bestMatch = onlyRoadCandidates.OrderBy(r => (r.curve.AttouchPoint(p) - p).magnitude).First();
            match = bestMatch;
            foreach (Road others in candidates)
            {
                if (others != bestMatch)
                {
                    List<Vector3> interPoints;
                    if (others.virtualRoad){
                        Curve othersCurve = others.curve.deepCopy().flattened();
                        Curve bestmatchCurve = bestMatch.curve.deepCopy().flattened();
                        interPoints = Geometry.curveIntersect(othersCurve, bestmatchCurve);

                        foreach (Vector3 point in interPoints)
                        {
                            if (Algebra.toVector2(point - p).magnitude <= ApproxLimit)
                            {
                                return bestMatch.curve.at((float)bestMatch.curve.paramOf(point));
                            }
                        }
                    }
                    else{
                        interPoints = Geometry.curveIntersect(bestMatch.curve, others.curve);
                        foreach (Vector3 point in interPoints)
                        {
                            if ((point - p).magnitude <= ApproxLimit)
                            {
                                return point;
                            }
                        }
                    }


                }
            }
            return bestMatch.curve.AttouchPoint(p);

        }

        if (candidates.Count > 0)
        {
            match = null;
            Vector3 flattened = candidates[0].curve.AttouchPoint(p);
            return new Vector3(flattened.x, p.y, flattened.z);
        }

        match = null;
        return p;
    }

    public void createRoadObject(Road r)
    {
        Destroy(r.roadObject);

        GameObject roadInstance = Instantiate(road, transform);
        RoadRenderer roadConfigure = roadInstance.GetComponent<RoadRenderer>();
        r.roadObject = roadInstance;
        roadConfigure.generate(r);

        /*
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
        */
        /*
        roadConfigure.generate(r.curve, r.laneconfigure,
                               startNode.getMargin(r).First, startNode.getMargin(r).Second,
                               endNode.getMargin(r).First, endNode.getMargin(r).Second);
        r.setMargins(startNode.getMargin(r).First, startNode.getMargin(r).Second, endNode.getMargin(r).First, endNode.getMargin(r).Second);
        */
    }

    public void deleteRoad(Road r){
        removeRoad(r);
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
                /*
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
                */
                n.updateMargins();

                foreach (var r1 in n.connection)
                {
                    createRoadObject(r1);
                }
            }
        }
    }

    /*TODO: improve efficiency*/
    public Path findPath(Road r1, float param1, Road r2, float param2){
        if (r1 == r2){
            /*Special trivial case*/
            return new Path(r1, param1, param2);
        }

        List<Node> possibleStartNodes = new List<Node>();
        List<Node> possibleEndNodes = new List<Node>();
        if (r1.validLaneCount(false) > 0){
            Node r10;
            findNodeAt(r1.curve.at(0f), out r10);
            possibleStartNodes.Add(r10);
        }

        if (r1.validLaneCount(true) > 0){
            Node r11;
            findNodeAt(r1.curve.at(1f), out r11);
            possibleStartNodes.Add(r11);
        }

        if (r2.validLaneCount(true) > 0){
            Node r20;
            findNodeAt(r2.curve.at(0f), out r20);
            possibleEndNodes.Add(r20);
        }

        if (r2.validLaneCount(false) > 0)
        {
            Node r21;
            findNodeAt(r2.curve.at(1f), out r21);
            possibleEndNodes.Add(r21);
        }

        if (possibleStartNodes.Count * possibleEndNodes.Count == 0){
            return null;
        }

        List<Path> candicatePaths = new List<Path>();
        foreach(Node nstart in possibleStartNodes){
            foreach(Node nend in possibleEndNodes){
                Path p = Node2NodeSP(nstart, nend);

                if (p != null){
                    p.insertAtStart(r1, param1);
                    p.insertAtEnd(r2, param2);
                    candicatePaths.Add(p);
                }
            }
        }

        if (candicatePaths.Count > 0)
        {
            return candicatePaths.MinBy((arg) => arg.length);
        }
        else{
            return null;
        }
    }

    Path Node2NodeSP(Node source, Node destination){

        Dictionary<Node, float> distances = new Dictionary<Node, float>();
        Dictionary<Node, Pair<Road, Node>> parentness = new Dictionary<Node, Pair<Road, Node>>();
        foreach(Node n in allnodes.Values){
            distances[n] = Mathf.Infinity;
        }
        distances[source] = 0f;
        
        while (distances.Count > 0){
            Node closestNode = distances.MinBy((KeyValuePair<Node, float> arg1) => arg1.Value).Key;

            foreach(Road r1 in closestNode.connection){
                Node neighbor = null;
                if (Algebra.isclose(r1.curve.at(0f), closestNode.position) && r1.validLaneCount(true) > 0){
                    findNodeAt(r1.curve.at(1f), out neighbor);
                }
                else{
                    if (Algebra.isclose(r1.curve.at(1f), closestNode.position) && r1.validLaneCount(false) > 0)
                    {
                        findNodeAt(r1.curve.at(0f), out neighbor);
                    }
                }
                float w1 = r1.SPWeight;

                if (neighbor != null && distances.ContainsKey(neighbor) && distances[neighbor] > distances[closestNode] + w1){
                    distances[neighbor] = distances[closestNode] + w1;
                    parentness[neighbor] = new Pair<Road, Node>(r1, closestNode);
                }
            }
            distances.Remove(closestNode);
        }

        if (parentness.ContainsKey(destination) || source.Equals(destination)){
            List<Road> sp = new List<Road>();
            List<Node> AllPassingNodes = new List<Node>();
            Node currentNode = destination;
            AllPassingNodes.Add(currentNode);
            while (parentness.ContainsKey(currentNode)){
                sp.Add(parentness[currentNode].First);
                currentNode = parentness[currentNode].Second;
                AllPassingNodes.Add(currentNode);
            }
            sp.Reverse();
            AllPassingNodes.Reverse();
            return new Path(AllPassingNodes, sp);
        }
        else{
            return null;
        }
    }

    private void FixedUpdate()
    {
        foreach(Road r in allroads){
            r.forwardVehicleController.updateLanes();
            r.backwardVehicleController.updateLanes();

            r.forwardVehicleController.updateAccs();
            r.backwardVehicleController.updateAccs();
        }
        foreach(Node n in allnodes.Values){
            foreach(Road vr in n.AllVirtualRoads){
                vr.forwardVehicleController.updateLanes();
                vr.backwardVehicleController.updateLanes();

                vr.forwardVehicleController.updateAccs();
                vr.backwardVehicleController.updateAccs();
            }
        }
    }

    public void serializeRoad(){
        List<Pair<string, List<string>>> allSerializedRoads = new List<Pair<string, List<string>>>();

        foreach(Road r in allroads){
            string serializedCurve = JsonUtility.ToJson(r.curve);
            List<string> laneConfigurePlusCurveType = r.laneconfigure.ConvertAll((input) => input);
            laneConfigurePlusCurveType.Add(r.curve.GetType().ToString());

            Pair<string, List<string>> serializedRoad = 
                new Pair<string, List<string>>(serializedCurve, laneConfigurePlusCurveType);
            allSerializedRoads.Add(serializedRoad);
        }

        using (StreamWriter streamWriter = File.CreateText("roadbuildersave.txt"))
        {
            streamWriter.Write(JsonConvert.SerializeObject(allSerializedRoads));
        }
    }

    /*clear everything on board*/
    private void Reset()
    {
        foreach(GameObject vehicleCtrl in GameObject.FindGameObjectsWithTag("Traffic/RouteController")){
            vehicleCtrl.GetComponent<RouteButton>().Reset();
        }

        foreach(Road r in allroads){
            Destroy(r.roadObject);
            r.forwardVehicleController = r.backwardVehicleController = null;
        }
        foreach(Node n in allnodes.Values){
            Destroy(n.gameObject);
        }
        allroads.Clear();
        allnodes.Clear();
    }

    public void deserializeRoad(){
        try
        {
            using (StreamReader streamReader = File.OpenText("roadbuildersave.txt"))
            {
                Reset();
                string allSerializedRoads = streamReader.ReadToEnd();
                List<Pair<string, List<string>>> deserializedRoads = JsonConvert.DeserializeObject<List<Pair<string, List<string>>>>(allSerializedRoads);
                foreach (var pair in deserializedRoads)
                {
                    string curveType = pair.Second[pair.Second.Count - 1];
                    Curve c;
                    switch (curveType)
                    {
                        case "Line":
                            c = JsonUtility.FromJson<Line>(pair.First);
                            break;
                        case "Bezeir":
                            c = JsonUtility.FromJson<Bezeir>(pair.First);
                            break;
                        case "Arc":
                            c = JsonUtility.FromJson<Arc>(pair.First);
                            break;
                        default:
                            throw new Exception();
                    }

                    addRoad(c, pair.Second.GetRange(0, pair.Second.Count - 1));
                }
            }
        }
        catch(Exception){
            GameObject.FindWithTag("Logger").GetComponent<MessageLogger>().LogMessage("No game save found!");
        }
    }
}