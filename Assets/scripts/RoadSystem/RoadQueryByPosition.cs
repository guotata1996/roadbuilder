using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MoreLinq;


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

    public static List<Lane> Query(Vector3 position, float radius = 5f)
    {
        return allLanes.FindAll(delegate (Lane input)
        {
            return input.ControlPoints.Any(cp => (cp - position).sqrMagnitude < radius * radius);
        });
    }

    public static Lane QueryClosest(Vector3 position, float tolerance = 3f)
    {
        if (allLanes.Count == 0)
        {
            return null;
        }

        Lane candidate = allLanes.MinBy(delegate (Lane input)
        {
            return input.ControlPoints.Min(cp => (cp - position).sqrMagnitude);
        });

        if (candidate != null && candidate.ControlPoints.Any(
            cp => (cp - position).sqrMagnitude < tolerance * tolerance))
            {
                return candidate;
            }
        return null;
    }
}
