using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public event System.EventHandler<Vector3> OnClick;
    public event System.EventHandler<Vector3> OnDragStart;
    public event System.EventHandler<Vector3> OnDragEnd;
    
    float pressingTime;
    bool dragging = false;
    float y = 0f;

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
            OnDragStart(this, MousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (dragging)
            {
                dragging = false;
                OnDragEnd(this, MousePosition);
            }
            else
            {
                OnClick(this, MousePosition);
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

    public void SwitchDragListenerTo(System.EventHandler<Vector3> handler)
    {
        if (dragging)
        {
            OnDragEnd(this, MousePosition);
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
