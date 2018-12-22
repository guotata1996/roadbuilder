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
    }
    //To add: enterable for, walkable for
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

    public float SPWeight{
        get
        {
            return curve.length;
        }
    }
    
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
        if (Algebra.isclose(param, 1f)){
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

    public bool getHeadingOfCurrentSeg(int segnum){
        return components[segnum].Second;
    }

}