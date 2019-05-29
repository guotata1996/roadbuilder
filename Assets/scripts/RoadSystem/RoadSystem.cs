using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static partial class RoadPositionRecords
{
    public static void AddLane(Curve3DSampler l, 
    out List<Lane> added, out List<Lane> deleted)
    {
        // Variable has not been actually calculated until "ToList()" is called
        var lane_intersections = from lane in allLanes
                                 let intersections = lane.IntersectWith(l)
                    where intersections.Count > 0
                    select (lane, intersections);


        added = new List<Lane>();
        deleted = new List<Lane>();
        List<float> l_intersect_params = new List<float>();

        // replacements
        foreach (var lane_intersection in lane_intersections.ToList())
        {
            Lane old_lane;
            List<Vector3> intersection;
            (old_lane, intersection) = lane_intersection;
            List<float> intersection_params = intersection.ConvertAll(input => old_lane.xz_curve.ParamOf(Algebra.toVector2(input)).Value);
            List<Curve3DSampler> splitted = old_lane.MultiCut(intersection_params, old_lane.xz_curve.Length);
            //remove old lanes & destroy objects.
            old_lane.SetGameobjVisible(false);
            allLanes.Remove(old_lane);
            deleted.Add(old_lane);

            //create replacements
            var replaced_lanes = splitted.ConvertAll(sampler => new Lane(sampler));
            added.AddRange(replaced_lanes);

            //store intersection points on l
            intersection.ForEach(input => 
            l_intersect_params.Add(l.xz_curve.ParamOf(Algebra.toVector2(input)).Value));
        }

        // create new lane
        List<Curve3DSampler> l_splitted = l.MultiCut(l_intersect_params, l.xz_curve.Length);
        var new_lanes = l_splitted.ConvertAll(sampler => new Lane(sampler));
        added.AddRange(new_lanes);
        Debug.Log(new_lanes.Count + "newly added");

        // Update record
        allLanes.AddRange(added);
    }

    public static void DeleteLanes(List<Lane> tobedeleted)
    {
        tobedeleted.ForEach(lane => { lane.SetGameobjVisible(false); allLanes.Remove(lane); });
    }

    public static void RestoreLanes(List<Lane> toberestored)
    {
        toberestored.ForEach(lane => { lane.SetGameobjVisible(true); allLanes.Add(lane); });
    }
}
