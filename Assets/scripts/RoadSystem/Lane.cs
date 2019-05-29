using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// It makes no sense to clone a lane: 
/// how to set reference to pointers in _left/next?
/// </summary>
public class Lane : Curve3DSampler
{
    const float maxAngleDiff = 0.1f;

    public Lane PrimaryLane
    {
        get;
        private set;
    }

    SpriteVerticeSampler spriteSampler;

    Lane[] next;

    Lane _left;

    GameObject laneObject;
    public void SetGameobjVisible(bool visible)
    {
        if (!visible)
        {
            GameObject.Destroy(laneObject);
        }
        else
        {
            laneObject = SolidCurve.Generate(this, spriteSampler);
        }
    }

    public Lane Left
    {
        set
        {
            _left = value;
            for (Lane current = this; current != null; current = current.Right)
            {
                current.PrimaryLane = value.PrimaryLane;
            }

            value.Right = this;
        }
        get
        {
            return _left;
        }
    }

    public Lane Right
    {
        get;
        private set;
    }

    public Lane(Curve xz_source, Function y_source):base(xz_source, y_source, maxAngleDiff)
    {
        spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>("Sectors/SimpleRoad"), 1f, 0.1f);
        
        if (this.IsValid)
        {
            laneObject = SolidCurve.Generate(this, spriteSampler);
        }

        void OnShapeOrValueChanged(object sender, int e)
        {
            GameObject.Destroy(laneObject);
            if (this.IsValid)
            {
                //Debug.Log("repaint: " + xz_source + " " + y_source + " @step=" + StepSize);
                laneObject = SolidCurve.Generate(this, spriteSampler);
            }
        }

        xz_curve.OnShapeChanged += OnShapeOrValueChanged;
        y_func.OnValueChanged += OnShapeOrValueChanged;

        PrimaryLane = this;
    }

    public Lane (Curve3DSampler sampler):base(sampler.xz_curve, sampler.y_func, maxAngleDiff)
    {
        spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>("Sectors/SimpleRoad"), 1f, 0.1f);

        Debug.Log("valid? " + sampler.xz_curve);
        if (this.IsValid)
        {
            laneObject = SolidCurve.Generate(this, spriteSampler);
        }

        void OnShapeOrValueChanged(object sender, int e)
        {
            GameObject.Destroy(laneObject);
            if (this.IsValid)
            {
                //Debug.Log("repaint: " + xz_source + " " + y_source + " @step=" + StepSize);
                laneObject = SolidCurve.Generate(this, spriteSampler);
            }
        }

        xz_curve.OnShapeChanged += OnShapeOrValueChanged;
        y_func.OnValueChanged += OnShapeOrValueChanged;

        PrimaryLane = this;
    }

}
