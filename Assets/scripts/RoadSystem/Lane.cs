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

    SpriteVerticeSampler spriteSampler;

    public LaneGroup laneGroup;

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


    public Lane(Curve xz_source, Function y_source):base(xz_source.Clone(), y_source.Clone(), maxAngleDiff)
    {
        spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>("Sectors/SimpleRoad"), 1f, 0.1f);

        Repaint();

        OnShapeChanged += (arg1, arg2) => Repaint();
    }

    public Lane (Curve3DSampler sampler):base(sampler.xz_curve.Clone(), sampler.y_func.Clone(), maxAngleDiff)
    {
        spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>("Sectors/SimpleRoad"), 1f, 0.1f);

        Repaint();

        OnShapeChanged += (arg1, arg2) => Repaint();
    }

    public void Repaint()
    {
        if (laneObject != null) {
            GameObject.Destroy(laneObject); 
        }

        if (this.IsValid && _laneObjectVisibleStatus == true)
            laneObject = SolidCurve.Generate(this, spriteSampler);
    }

}
