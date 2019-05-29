using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MoreLinq;
using UnityEditor;

/// <summary>
/// Controls the last possible (1.unset or 2.close) ControlPoint (in natural order)
/// </summary>
public class FollowMouseBehavior : MonoBehaviour
{
    [SerializeField]
    InputHandler input;

    Curve3DSampler curvesampler;
    int ctrlPointIndex;
    Vector3 lastFrameMousePosition = Vector3.zero;

    public void SetTarget(object data)
    {
        if (data == null)
        {
            curvesampler = null;
        }
        else
        {
            curvesampler = (Lane)data;
            
            Vector3 closestCtrlPoint = curvesampler.ControlPoints.MinBy((Vector3 arg) => float.IsInfinity(arg.x) ? -1 : (input.MagnetMousePosition - arg).sqrMagnitude);
            ctrlPointIndex = curvesampler.ControlPoints.LastIndexOf(closestCtrlPoint);

            lastFrameMousePosition = input.MagnetMousePosition;
        }
    }

    public void Update()
    {
        if (curvesampler == null)
        {
            return;
        }

        var thisFrameMousePosition = input.MagnetMousePosition;
        var deltaMousePosition = thisFrameMousePosition - lastFrameMousePosition;

        var ctrl = curvesampler.ControlPoints;

        if (!float.IsInfinity(ctrl[ctrlPointIndex].x))
        {
            ctrl[ctrlPointIndex] += deltaMousePosition;
        }
        else
        {
            ctrl[ctrlPointIndex] = thisFrameMousePosition;
        }

        curvesampler.ControlPoints = ctrl;

        lastFrameMousePosition = thisFrameMousePosition;
    }

    public void Undo()
    {
        throw new System.NotImplementedException();
    }

}
