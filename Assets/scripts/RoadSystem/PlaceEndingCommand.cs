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

    public override void Execute(object obj)
    {
        object[] obj_arr = (object[])obj;
        Curve c = (Curve)obj_arr[0];
        Function f = (Function)obj_arr[1];
        
        List<Vector2> ctrl = c.ControlPoints;
        if (c is Line)
        {
            if (float.IsInfinity(ctrl[0].x))
            {
                ctrl[0] = Algebra.toVector2(position);
            }
            else
            {
                ctrl[1] = Algebra.toVector2(position);
            }
        }
        if (c is Arc)
        {
            if (float.IsInfinity(ctrl[0].x))
            {
                ctrl[0] = Algebra.toVector2(position);
            }
            else
            {
                ctrl[2] = Algebra.toVector2(position);
            }
        }
        if (c is Bezier)
        {
            if (float.IsInfinity(ctrl[0].x))
            {
                ctrl[0] = Algebra.toVector2(position);
            }
            else
            {
                ctrl[2] = Algebra.toVector2(position);
            }

        }

        if (f is LinearFunction)
        {
            if (float.IsInfinity(((LinearFunction)f).y0))
            {
                ((LinearFunction)f).y0 = position.y;
            }
            else
            {
                ((LinearFunction)f).y1 = position.y;
            }
        }

        c.ControlPoints = ctrl;

    }

    public override void Undo()
    {

    }
}
