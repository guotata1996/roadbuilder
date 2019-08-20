using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField]
    GameObject cursor;

    [SerializeField]
    GameObject roadDrawing;

    [SerializeField]
    Canvas ezDrawingCanvas;

    public event System.EventHandler<(Vector3, Curve3DSampler)> OnClick;
    public event System.EventHandler<Vector3> OnDragStart;
    public event System.EventHandler<Vector3> OnDragEnd;

    public event System.EventHandler OnUndoPressed;
    public event System.EventHandler OnIncPressed;
    public event System.EventHandler OnDecPressed;

    public event System.EventHandler OnRightKeyPressed;
    public event System.EventHandler OnLeftKeyPressed;
    public event System.EventHandler OnForwardKeyPressed;

    float pressingTime;
    bool dragging = false;
    float y = 0f;

    public StickyMouse stickyMouse;

    public Stack<Command> commandSequence = new Stack<Command>();

    void Start()
    {
        stickyMouse = new StickyMouse();

        OnUndoPressed += delegate {
            var latestCmd = commandSequence.Pop();
            latestCmd.Undo();
            BuildRoad.UseDefaultStickyMouseForRoad(stickyMouse);
        };

        ezDrawingCanvas.enabled = false;
    }

    void Update()
    {

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            roadDrawing.GetComponent<EzBuildRoad>().enabled = false;
            roadDrawing.GetComponent<BuildRoad>().enabled = true;
            Debug.Log("Build Road Mode");
            ezDrawingCanvas.enabled = false;

        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            roadDrawing.GetComponent<BuildRoad>().enabled = false;
            roadDrawing.GetComponent<EzBuildRoad>().enabled = true;
            Debug.Log("EZ Build Road Mode");
            ezDrawingCanvas.enabled = true;
        }


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
            Vector3 pos1;
            Curve3DSampler curve1;
            (pos1, curve1) = MagnetMousePosition;
            OnDragStart(this, pos1);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (dragging)
            {
                dragging = false;
                Vector3 pos2;
                Curve3DSampler curve2;
                (pos2, curve2) = MagnetMousePosition;

                OnDragEnd(this, pos2);
            }
            else
            {
                //stickyMouse.SetLane(RoadPositionRecords.allLanes);
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

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnLeftKeyPressed(this, null);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnRightKeyPressed(this, null);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnForwardKeyPressed(this, null);
        }

        Vector3 pos;
        Curve3DSampler curve;
        (pos, curve) = MagnetMousePosition;
        cursor.transform.position = pos;
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

    public (Vector3, Curve3DSampler) MagnetMousePosition
    {
        get
        {
            var curve = stickyMouse.StickTo3DCurve(MousePosition, out Vector3 position);
            return (position, curve);
        }
    }

    public void Reset()
    {
        if (dragging)
        {
            Vector3 pos;
            Curve3DSampler curve;
            (pos, curve) = MagnetMousePosition;
            OnDragEnd(this, pos);
            pressingTime = 0f;
            dragging = false;
        }
        OnClick = null;
        OnDragEnd = null;
        OnDragStart = null;
        OnRightKeyPressed = null;
        OnLeftKeyPressed = null;
        OnForwardKeyPressed = null;

    }
}
