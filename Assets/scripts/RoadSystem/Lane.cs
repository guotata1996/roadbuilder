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

    public const float laneWidth = 1.0f;

    SpriteVerticeSampler spriteSampler;

    public LaneGroup laneGroup;

    public Material normal_material, highlight_material;

    GameObject laneObject, highLightMask;
    private bool _laneObjectVisibleStatus = true;
    
    public void SetGameobjVisible(bool visible)
    {
        _laneObjectVisibleStatus = visible;
        if (!visible)
        {
            GameObject.Destroy(laneObject);
            GameObject.Destroy(highLightMask);
        }
        else
        {
            if (laneObject == null)
            {
                laneObject = SolidCurve.Generate(this, spriteSampler, normal_material);
            }
        }
    }

    public void SetHighlighted(bool highlight)
    {
        if (highlight)
        {

            if (_laneObjectVisibleStatus && highLightMask == null)
            {
                highLightMask = SolidCurve.Generate(this, spriteSampler, highlight_material);
                GameObject.Destroy(laneObject);
            }
        }
        else
        {
            if (highLightMask != null)
            {
                GameObject.Destroy(highLightMask);
            }
            if (_laneObjectVisibleStatus && laneObject == null)
            {
                laneObject = SolidCurve.Generate(this, spriteSampler, normal_material);
            }
        }
    }


    public Lane(Curve xz_source, Function y_source, bool _indicate = false):base(xz_source.Clone(), y_source.Clone(), maxAngleDiff)
    {
        spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>(_indicate ? "Sectors/Indicator" : "Sectors/SimpleRoad"), 1f, 0.1f);

        normal_material = Resources.Load<Material>("Materials/concrete");

        Repaint();
        
        OnShapeChanged += (arg1, arg2) => Repaint();
    }

    public Lane (Curve3DSampler sampler, bool _indicate = false) :base(sampler.xz_curve.Clone(), sampler.y_func.Clone(), maxAngleDiff)
    {
        spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>(_indicate ? "Sectors/Indicator" : "Sectors/SimpleRoad"), 1f, 0.1f);

        normal_material = Resources.Load<Material>("Materials/concrete");

        Repaint();

        OnShapeChanged += (arg1, arg2) => Repaint();
    }

    public void Repaint()
    {
        if (laneObject != null) {
            GameObject.Destroy(laneObject); 
        }

        if (this.IsValid && _laneObjectVisibleStatus == true)
            laneObject = SolidCurve.Generate(this, spriteSampler, normal_material);
    }

    /*Connection info*/

    public Lane leftNeighbor, rightNeighbor;
    public HashSet<Lane> frontNeighbors;
    public HashSet<Lane> backNeighbors;

    public Lane dynamicInConnection, dynamicOutConnection;

}
