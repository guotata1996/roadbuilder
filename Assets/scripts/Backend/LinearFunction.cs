using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LinearFunction : Function
{
    public float y0, y1;

    public LinearFunction(float y_0 = float.NegativeInfinity, float y_1 = float.NegativeInfinity)
    {
        y0 = y_0;
        y1 = y_1;
    }

    public override bool IsValid => !float.IsInfinity(y0) && !float.IsInfinity(y1);

    public override float GradientAt(float t)
    {
        return y1 - y0;
    }

    public override float ValueAt(float t)
    {
        return Mathf.Lerp(y0, y1, t);
    }

    public override string ToString()
    {
        return "Linear func y0= " + y0 + " y1= " + y1;
    }

    public override void Crop(float start_t, float end_t)
    {
        float y_old_start = ValueAt(start_t);
        float y_old_end = ValueAt(end_t);

        y0 = y_old_start;
        y1 = y_old_end;
    }

    public override Function Clone()
    {
        return new LinearFunction(y0, y1);
    }

    public override List<float> ControlPoints
    {
        get
        {
            return (new float[] { y0, y1 }).ToList();
        }
        set
        {
            bool changed = (y0 != value[0] || y1 != value[1]);

            y0 = value[0];
            y1 = value[1];

            if (changed)
            {
                NotifyValueChanged();
            }
        }
    }
}
