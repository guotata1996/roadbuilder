using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceEndingCommand : Command
{
    Vector3 position;
    public PlaceEndingCommand(Vector3 mousePosition)
    {
        position = mousePosition;
    }

    public void Execute(object obj)
    {
        Curve3DSampler c = (Lane)obj;
        
        List<Vector3> ctrl = c.ControlPoints;
        if (float.IsInfinity(ctrl[0].x))
        {
            ctrl[0] = position;
        }
        else
        {
            ctrl[ctrl.Count - 1] = position;
        }

        c.ControlPoints = ctrl;

    }

    public void Undo()
    {

    }
}
