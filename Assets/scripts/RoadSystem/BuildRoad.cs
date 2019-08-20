using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum CurveMode { Line, Arc, Bezier }

public class BuildRoad : MonoBehaviour
{
    [SerializeField]
    InputHandler inputHandler;

    Type spawnType = typeof(Line);

    Lane currentLane;

    float highlightRadius = 2f;


    void OnEnable()
    {
        if (inputHandler.stickyMouse != null)
        {
            UseDefaultStickyMouseForRoad(inputHandler.stickyMouse);
        }

        // Init behavior
        inputHandler.OnClick += delegate (object sender, (Vector3, Curve3DSampler) pos_parent) {
            if (currentLane == null)
            {
                Debug.Log("add new");
                Curve currentCurve = null;
                if (spawnType == typeof(Line))
                {
                    currentCurve = Line.GetDefault();
                }
                if (spawnType == typeof(Arc))
                {
                    currentCurve = Arc.GetDefault();
                }
                if (spawnType == typeof(Bezier))
                {
                    currentCurve = Bezier.GetDefault();
                }

                Function currentFunc = new LinearFunction(); // TODO: Create more
                currentLane = new Lane(currentCurve, currentFunc);
            }

            Vector3 position;
            Curve3DSampler parentCurve; // which virtual curve does the road under construction belong to?
            (position, parentCurve) = pos_parent;


            new PlaceEndingCommand(position).Execute(currentLane);

            if (currentLane.IsValid)
            {
                // Place
                GetComponent<FollowMouseBehavior>().enabled = false;
                var placeCmd = new PlaceLaneCommand();
                inputHandler.commandSequence.Push(placeCmd);
                placeCmd.Execute(currentLane);
                currentLane.SetGameobjVisible(false);
                currentLane = null;
                GetComponent<HighLightCtrlPointBehavior>().radius = highlightRadius;

                UseDefaultStickyMouseForRoad(inputHandler.stickyMouse);
            }
            else
            {
                // Next ctrl point Pending
                if (parentCurve != null)
                {
                    UseSingleVirtualCurveForRoad(parentCurve);
                }
                else
                {
                    UseDefaultStickyMouseForRoad(inputHandler.stickyMouse);
                }

                GetComponent<FollowMouseBehavior>().enabled = true;
                GetComponent<FollowMouseBehavior>().SetTarget(currentLane);
                GetComponent<HighLightCtrlPointBehavior>().radius = 0f;
            }

        };

        // Adjust behavior
        inputHandler.OnDragStart += delegate (object sender, Vector3 position)
        {
            Lane targetLane = RoadPositionRecords.QueryClosestCPs3DCurve(position);
            if (targetLane != null)
            {
                GetComponent<FollowMouseBehavior>().enabled = true;
                currentLane = new Lane(targetLane);
                GetComponent<FollowMouseBehavior>().SetTarget(currentLane);
                
                //replace targetLane with a temporary object (currentLane)
                var removeCmd = new RemoveLaneCommand();
                inputHandler.commandSequence.Push(removeCmd);
                removeCmd.Execute(targetLane);
                
                GetComponent<HighLightCtrlPointBehavior>().radius = 0f;
            }
            
        };

        inputHandler.OnDragEnd += delegate (object sender, Vector3 position)
        {
            if (currentLane != null)
            {
                GetComponent<FollowMouseBehavior>().enabled = false;
                GetComponent<HighLightCtrlPointBehavior>().radius = highlightRadius;

                // add actual lane to network
                var placeCmd = new PlaceLaneCommand();
                inputHandler.commandSequence.Push(placeCmd);
                placeCmd.Execute(currentLane);

                currentLane.SetGameobjVisible(false);
                currentLane = null;
            }
        };
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1)){
            Debug.Log("Line mode");
            spawnType = typeof(Line);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Debug.Log("Arc mode");
            spawnType = typeof(Arc);
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            Debug.Log("Bezier mode");
            spawnType = typeof(Bezier);
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            Debug.Log("Quit");
            currentLane = null;
        }
    }

    static public void UseDefaultStickyMouseForRoad(StickyMouse mouse)
    {
        mouse.Reset();

        mouse.AddLane(RoadPositionRecords.allLanes);

        // Intersect shiftcurves with allLanes
        // ctrl points added at same time as interest
        var ctrl_points = new Dictionary<Vector3, Lane>();

        var rightShiftCurves = RoadPositionRecords.allLanes.ConvertAll((input) =>
        {
            var cloned_3d_curve = input.Clone();
            cloned_3d_curve.xz_curve.ShiftRight(Lane.laneWidth);
            foreach (var cp in cloned_3d_curve.ControlPoints)
            {
                if (!ctrl_points.ContainsKey(cp))
                {
                    ctrl_points.Add(cp, input);
                }
            }
            return cloned_3d_curve;
        }).FindAll(input => input.IsValid);

        var leftShiftCurves = RoadPositionRecords.allLanes.ConvertAll((input) =>
        {
            var cloned_3d_curve = input.Clone();
            cloned_3d_curve.xz_curve.ShiftRight(-Lane.laneWidth);
            foreach (var cp in cloned_3d_curve.ControlPoints)
            {
                if (!ctrl_points.ContainsKey(cp))
                {
                    ctrl_points.Add(cp, input);
                }
            }
            return cloned_3d_curve;
        }).FindAll(input => input.IsValid);

        leftShiftCurves.AddRange(rightShiftCurves);
        mouse.AddVirtualCurve(leftShiftCurves);

        //For debug: show all shifted curves
        //leftShiftCurves.ForEach(input => shift_indicators.Add(new Lane(input, _indicate: true)));

        // Add Intersection points as interest
        var intersection_points = new Dictionary<Vector3, Lane>();
        foreach (Curve3DSampler c in leftShiftCurves)
        {
            foreach (Lane l in RoadPositionRecords.allLanes)
            {
                foreach (var inter_position in c.IntersectWith(l, filter_self: false, filter_other: true))
                {
                    if (((inter_position - c.GetThreedPos(0)).sqrMagnitude < 40f || (inter_position - c.GetThreedPos(1)).sqrMagnitude < 40f)
                    && !intersection_points.ContainsKey(inter_position))
                    {
                        intersection_points.Add(inter_position, l);
                    }
                }
            }
        }

        // Merge ctrl_points with priority lower than inter_points
        foreach (var cp in ctrl_points)
        {
            if (!intersection_points.Keys.ToList().Any(input => (input - cp.Key).sqrMagnitude < Lane.laneWidth * Lane.laneWidth))
            {
                intersection_points.Add(cp.Key, cp.Value);
            }
        }
        mouse.AddPoint(intersection_points);
    }

    private void UseSingleVirtualCurveForRoad(Curve3DSampler vCurve)
    {
        inputHandler.stickyMouse.Reset();

        inputHandler.stickyMouse.AddLane(RoadPositionRecords.allLanes);

        inputHandler.stickyMouse.AddVirtualCurve(new List<Curve3DSampler>(){vCurve});

        
    }

    private void OnDisable()
    {
        inputHandler.Reset();
    }
}
