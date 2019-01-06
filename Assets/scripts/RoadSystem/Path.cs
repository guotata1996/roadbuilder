using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Path
{
    List<Pair<Road, bool>> components;
    float startParam, endParam;
    Node sourceNode, destNode;

    public Path(List<Node> passingNodes, List<Road> comp)
    {
        Debug.Assert(passingNodes.Count == comp.Count + 1 || passingNodes.Count == 0);

        components = new List<Pair<Road, bool>>();
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

            if (i != comp.Count - 1)
            {
                Road virtualNodePath = passingNodes[i + 1].getVirtualRoad(comp[i], comp[i + 1]);
                components.Add(new Pair<Road, bool>(virtualNodePath, true));
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
        if (components.Count > 1 && road != components[1].First)
        {
            //otherwise, must not be SP
            Road virtualNodePath = sourceNode.getVirtualRoad(road, components[1].First);
            components.Insert(1, new Pair<Road, bool>(virtualNodePath, true));
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
            components.Insert(components.Count - 1, new Pair<Road, bool>(virtualNodePath, true));
        }
        endParam = param;
    }

    public float length
    {
        get
        {
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
            else
            {
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
        else
        {
            if (components[0].First != components[1].First)
            {
                str += components[0].First.curve.at_ending(!components[0].Second);
                str += "==>";
            }
            str += components[1].First.curve.at(endParam);
        }
        return str;
    }


    public Pair<Road, float> travelAlong(int segnum, float param, float distToTravel, out int nextseg, out bool termination)
    {
        //check whether to jump at the very beginning
        //Debug.Log(segnum + " , " + param + " , " + components[segnum].Second + " , " + components[segnum].First.margin1End);

        if (components[segnum].Second && (param > components[segnum].First.margin1End) ||
            (!components[segnum].Second) && (param < components[segnum].First.margin0End) ||
            (segnum == components.Count - 1 && components[segnum].Second && param > endParam) ||
            (segnum == components.Count - 1 && !components[segnum].Second && param < endParam))
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
                param = components[segnum].Second ? components[segnum].First.margin0End : components[segnum].First.margin1End;
            }
        }

        //Do not jump to second road
        var roadOn = components[segnum];
        float newParam = roadOn.First.curve.TravelAlong(param, distToTravel, roadOn.Second);
        termination = false;
        nextseg = segnum;
        return new Pair<Road, float>(roadOn.First, newParam);
    }

    public Road getRoadOfSeg(int segnum)
    {
        return components[segnum].First;
    }

    public bool getHeadingOfCurrentSeg(int segnum)
    {
        return components[segnum].Second;
    }

    /* derived property */
    List<Curve> curveRepresentation;

    public List<Curve> getCurveRepresentation()
    {

        if (curveRepresentation == null)
        {

            if (components.Count == 1)
            {
                Curve p = components.First().First.curve.cutByParam(Mathf.Min(startParam, endParam), Mathf.Max(startParam, endParam));
                curveRepresentation = new List<Curve> { p };
            }
            else
            {
                List<Curve> allcurves = new List<Curve>();
                Curve ps = components.First().Second ?
                                     components.First().First.curve.cutByParam(startParam, components.First().First.margin1End) :
                                     components.First().First.curve.cutByParam(components.First().First.margin0End, startParam);
                allcurves.Add(ps);

                foreach (var c in components.GetRange(1, components.Count - 2))
                {
                    allcurves.Add(c.First.curve.cut(c.First.margin0End, c.First.margin1End));
                }

                Curve pe = components.Last().Second ?
                                     components.Last().First.curve.cutByParam(components.Last().First.margin0End, endParam) :
                                     components.Last().First.curve.cutByParam(endParam, components.Last().First.margin1End);
                allcurves.Add(pe);

                curveRepresentation = allcurves;
            }
        }

        return curveRepresentation;

    }
}
