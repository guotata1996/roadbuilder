using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Function
{
    public abstract float ValueAt(float t);

    public abstract float GradientAt(float t);
}
