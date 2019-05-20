using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearFunction : Function
{
    float y0, y1;

    public LinearFunction(float y_0, float y_1)
    {
        y0 = y_0;
        y1 = y_1;
    }

    public override float GradientAt(float t)
    {
        return y1 - y0;
    }

    public override float ValueAt(float t)
    {
        return Mathf.Lerp(y0, y1, t);
    }
}
