using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Function : LinearFragmentable<Function>
{
    public abstract float ValueAt(float t);

    public abstract float GradientAt(float t);

    public abstract bool IsValid { get; }

    public abstract List<float> ControlPoints { get; set; }

    public event System.EventHandler<int> OnValueChanged;

    protected void NotifyValueChanged()
    {
        OnValueChanged(this, 0);
    }
}
