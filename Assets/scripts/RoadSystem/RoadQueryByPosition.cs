using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary>
/// A naive implementation
/// </summary>
public static class RoadQueryByPosition
{
    static List<Lane> allLanes = new List<Lane>();

    public static void AddLane(Lane l)
    {
        allLanes.Add(l);
    }

    public static List<Lane> Query(Vector3 position)
    {
        return allLanes.FindAll(delegate (Lane input)
        {
            return input.ControlPoints.Any(cp => Algebra.isRoadNodeClose(cp, position));
        });
    }
}
