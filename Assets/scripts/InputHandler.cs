using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField]
    GameObject cursor;

    public event System.EventHandler<Vector3> OnClick;
    public event System.EventHandler<Vector3> OnDragStart;
    public event System.EventHandler<Vector3> OnDragEnd;

    public event System.EventHandler OnUndoPressed;
    public event System.EventHandler OnIncPressed;
    public event System.EventHandler OnDecPressed;
    
    float pressingTime;
    bool dragging = false;
    float y = 0f;

    StickyMouse stickyMouse;

    //For debug: all shifted curves
    List<Lane> shift_indicators;

    void Start()
    {
        stickyMouse = new StickyMouse();

        shift_indicators = new List<Lane>();

        RoadPositionRecords.OnMapChanged += (sender, e) =>
        {
            shift_indicators.ForEach(lane => lane.SetGameobjVisible(false));
            shift_indicators.Clear();

            stickyMouse.SetLane(RoadPositionRecords.allLanes);

            var rightShiftCurves = RoadPositionRecords.allLanes.ConvertAll((input) =>
            {
                var cloned_3d_curve = input.Clone();
                cloned_3d_curve.xz_curve.ShiftRight(Lane.laneWidth);
                return cloned_3d_curve;
            }).FindAll(input => input.IsValid);

            var leftShiftCurves = RoadPositionRecords.allLanes.ConvertAll((input) =>
            {
                var cloned_3d_curve = input.Clone();
                cloned_3d_curve.xz_curve.ShiftRight(- Lane.laneWidth);
                return cloned_3d_curve;
            }).FindAll(input => input.IsValid);

            leftShiftCurves.AddRange(rightShiftCurves);

            //For debug: show all shifted curves
            leftShiftCurves.ForEach(input => shift_indicators.Add(new Lane(input, _indicate: true)));

            stickyMouse.SetVirtualCurve(leftShiftCurves);

            // Intersect shiftcurves with allLanes
            var intersection_points = new Dictionary<Vector3, Lane>();
            foreach(Curve3DSampler c in leftShiftCurves)
            {
                foreach (Lane l in RoadPositionRecords.allLanes)
                {
                    foreach (var inter_position in c.IntersectWith(l, filter_self: false, filter_other: true))
                    {
                        if (!intersection_points.ContainsKey(inter_position))
                        {
                            intersection_points.Add(inter_position, l);
                        }
                    }
                }
            }
            stickyMouse.SetPoint(intersection_points);
        };
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            pressingTime += Time.deltaTime;
        }
        else
        {
            pressingTime = 0f;
        }
        if (pressingTime > 0.8f && !dragging)
        {
            dragging = true;
            OnDragStart(this, MagnetMousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (dragging)
            {
                dragging = false;
                OnDragEnd(this, MagnetMousePosition);
            }
            else
            {
                OnClick(this, MagnetMousePosition);
            }
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            y = y + 1.0f;
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            y = y - 1.0f;
        }

        if (Input.GetKey(KeyCode.LeftApple) && Input.GetKeyDown(KeyCode.Z))
        {
            OnUndoPressed(this, null);
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            OnIncPressed(this, null);
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            OnDecPressed(this, null);
        }

        cursor.transform.position = MagnetMousePosition;
    }

    public Vector3 MousePosition
    {
        get
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane y_0 = new Plane(Vector3.up, Vector3.up * y);
            y_0.Raycast(ray, out float enter);
            return ray.GetPoint(enter);
        }
    }

    public Vector3 MagnetMousePosition
    {
        get
        {
            stickyMouse.StickTo3DCurve(MousePosition, out Vector3 position);
            return position;
        }
    }

    public void SwitchDragListenerTo(System.EventHandler<Vector3> handler)
    {
        if (dragging)
        {
            OnDragEnd(this, MagnetMousePosition);
            pressingTime = 0f;
            dragging = false;
        }
        OnDragStart = handler;
        OnDragEnd = handler;
    }

    public void SwitchClickListenerTo(System.EventHandler<Vector3> handler)
    {
        OnClick = handler;
    }
}
