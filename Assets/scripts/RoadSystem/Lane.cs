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
    private bool _laneObjectVisibleStatus = true;
    
    public void SetGameobjVisible(bool visible)
    {
        _laneObjectVisibleStatus = visible;
        if (!visible)
        {
            Debug.Assert(laneObject != null);
            GameObject.Destroy(laneObject);
        }
        else
        {
            if (laneObject == null)
            {
                laneObject = SolidCurve.Generate(this, spriteSampler);
            }
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

    public Lane(Curve xz_source, Function y_source):base(xz_source.Clone(), y_source.Clone(), maxAngleDiff)
    {
        spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>("Sectors/SimpleRoad"), 1f, 0.1f);
        
        if (this.IsValid)
        {
            laneObject = SolidCurve.Generate(this, spriteSampler);
        }

        void OnShapeOrValueChanged(object sender, int e)
        {
            GameObject.Destroy(laneObject);
            if (this.IsValid && _laneObjectVisibleStatus == true)
            {
                //Debug.Log("repaint: " + xz_source + " " + y_source + " @step=" + StepSize);
                laneObject = SolidCurve.Generate(this, spriteSampler);
            }
        }

        xz_curve.OnShapeChanged += OnShapeOrValueChanged;
        y_func.OnValueChanged += OnShapeOrValueChanged;

        PrimaryLane = this;
    }

    public Lane (Curve3DSampler sampler):base(sampler.xz_curve.Clone(), sampler.y_func.Clone(), maxAngleDiff)
    {
        spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>("Sectors/SimpleRoad"), 1f, 0.1f);

        if (this.IsValid)
        {
            laneObject = SolidCurve.Generate(this, spriteSampler);
        }

        void OnShapeOrValueChanged(object sender, int e)
        {
            GameObject.Destroy(laneObject);
            if (this.IsValid && _laneObjectVisibleStatus == true)
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
