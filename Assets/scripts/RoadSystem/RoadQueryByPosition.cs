using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MoreLinq;


/// <summary>
/// A naive implementation
/// </summary>
public static partial class RoadPositionRecords
{
    static List<Lane> allLanes = new List<Lane>();

    public static List<Lane> QueryAllCP(Vector3 position, float radius = 5f)
    {
        return allLanes.FindAll(delegate (Lane input)
        {
            return input.ControlPoints.Any(cp => (cp - position).sqrMagnitude < radius * radius);
        });
    }

    public static Curve3DSampler QueryClosestCPs3DCurve(Vector3 position, float tolerance = 3f)
    {
        if (allLanes.Count == 0)
        {
            return null;
        }

        Curve3DSampler candidate = allLanes.MinBy(delegate (Curve3DSampler input)
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

    public static Curve3DSampler QueryNodeOr3DCurve(Vector3 position, out Vector3 out_position, float radius = 1f)
    {
        // Check if any intersection or end is close
        // 1. Find (maybe more than one) lane(s) with ending close to position
        List<Curve3DSampler> endingCloseToPosition = new List<Curve3DSampler>();
        foreach(Curve3DSampler l in allLanes)
        {
            if ((l.GetThreedPos(0) - position).sqrMagnitude < radius)
            {
                endingCloseToPosition.Add(l);
            }
            if ((l.GetThreedPos(1) - position).sqrMagnitude < radius)
            {
                endingCloseToPosition.Add(l);
            }
        }

        // 2. Find the one with least approximation error
        if (endingCloseToPosition.Count > 0)
        {
            Curve3DSampler leastApproxError = endingCloseToPosition.MinBy(
            (arg) => (arg.GetAttractedPoint(position, radius * 1.01f) - position).magnitude);

            out_position = leastApproxError.GetAttractedPoint(position, radius * 1.01f);
            return leastApproxError;
        }

        // If not found, Find the one with least approximation error from whole set
        if (allLanes.Count == 0)
        {
            out_position = position;
            return null;
        }

        Curve3DSampler candidate = allLanes.MinBy(delegate (Curve3DSampler arg)
        {
            var attracted = arg.GetAttractedPoint(position, radius);
            return (attracted == position) ? float.PositiveInfinity : (attracted - position).sqrMagnitude;
        });

        var attractedMin = candidate.GetAttractedPoint(position, radius);
        if (attractedMin == position || (attractedMin - position).sqrMagnitude > radius * radius)
        {
            out_position = position;
            return null;
        }

        out_position = attractedMin;
        return candidate;
    }


}
