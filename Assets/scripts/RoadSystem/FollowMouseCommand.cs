using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MoreLinq;

public class FollowMouseCommand : MonoBehaviour, Command
{
    public InputHandler input;

    Curve3DSampler curvesampler;
    int ctrlPointIndex;
    Vector3 lastFrameMousePosition = Vector3.zero;

    public void Execute(object data)
    {
        if (data == null)
        {
            curvesampler = null;
        }
        else
        {
            curvesampler = (Lane)data;
            Vector3 closestCtrlPoint = curvesampler.ControlPoints.MinBy((Vector3 arg) => (input.MousePosition - arg).sqrMagnitude);
            ctrlPointIndex = curvesampler.ControlPoints.IndexOf(closestCtrlPoint);

            lastFrameMousePosition = input.MousePosition;
        }
    }

    public void Update()
    {
        if (curvesampler == null)
        {
            return;
        }

        var thisFrameMousePosition = input.MousePosition;
        var deltaMousePosition = thisFrameMousePosition - lastFrameMousePosition;

        var ctrl = curvesampler.ControlPoints;
        ctrl[ctrlPointIndex] += deltaMousePosition;
        curvesampler.ControlPoints = ctrl;


        lastFrameMousePosition = input.MousePosition;
    }

    public void Undo()
    {
        throw new System.NotImplementedException();
    }
}
