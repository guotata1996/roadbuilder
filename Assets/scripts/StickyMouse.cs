using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreLinq;

public class StickyMouse {
    List<Curve3DSampler> _source;
    List<Lane> _lane_source;
    Dictionary<Vector3, Lane> _points;

    public void SetVirtualCurve(List<Curve3DSampler> cs)
    {
        _source = cs;
    }

    public void SetLane(List<Lane> lcs)
    {
        _lane_source = lcs;
    }

    public void SetPoint (Dictionary<Vector3, Lane> points)
    {
        _points = points;
    }

    public Curve3DSampler StickTo3DCurve(Vector3 position, out Vector3 out_position, float radius = Lane.laneWidth * 0.6f)
    {
        List<Curve3DSampler> curves = new List<Curve3DSampler>();

        if (_source != null)
            curves.AddRange(_source);

        if (_lane_source != null)
        {
            foreach (var lc in _lane_source)
            {
                curves.Add((Curve3DSampler)lc);
            }
        }

        Debug.Assert(curves.TrueForAll(input => input.IsValid));

        // Check if any intersection or end is close
        // 1. Find (maybe more than one) lane(s) with ending close to position
        if (_lane_source != null)
        {
            List<Curve3DSampler> close_lane_endings = new List<Curve3DSampler>();
            foreach (Curve3DSampler l in _lane_source)
            {
                if ((l.GetThreedPos(0) - position).sqrMagnitude < radius * radius)
                {
                    close_lane_endings.Add(l);
                }
                if ((l.GetThreedPos(1) - position).sqrMagnitude < radius * radius)
                {
                    close_lane_endings.Add(l);
                }
            }

            // 1.2. Find the one with least approximation error
            if (close_lane_endings.Count > 0)
            {
                Curve3DSampler leastApproxError = close_lane_endings.MinBy(
                (arg) => (arg.GetAttractedPoint(position, radius * 1.01f) - position).sqrMagnitude);

                out_position = new List<Vector3> { leastApproxError.GetThreedPos(0), leastApproxError.GetThreedPos(1) }.MinBy(
                        input => (input - position).sqrMagnitude);
                return leastApproxError;
            }
        }

        // 2. Attract to _points
        if (_points != null)
        {
            KeyValuePair<Vector3, Lane> close_point = new KeyValuePair<Vector3, Lane>();
            float minsqrMagnitude = float.PositiveInfinity;
            foreach (var point_lane in _points)
            {
                if ((point_lane.Key - position).sqrMagnitude < minsqrMagnitude)
                {
                    close_point = point_lane;
                    minsqrMagnitude = (point_lane.Key - position).sqrMagnitude;
                }

            }
            if (minsqrMagnitude < radius * radius)
            {
                out_position = close_point.Key;
                return close_point.Value;
            }
        }

        // 3. Find a point on single curve with least approximation error from whole set
        if (curves.Count == 0)
        {
            out_position = position;
            return null;
        }

        Curve3DSampler candidate = curves.MinBy(delegate (Curve3DSampler arg)
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
