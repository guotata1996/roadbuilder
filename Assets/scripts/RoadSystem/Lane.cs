using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : Curve3DSampler
{
    const float maxAngleDiff = 0.1f;

    public Lane PrimaryLane
    {
        get;
        private set;
    }

    Lane[] next;

    Lane _left;

    GameObject laneObject;

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
        SpriteVerticeSampler spriteSampler = new SpriteVerticeSampler(Resources.Load<Sprite>("Sectors/SimpleRoad"), 1f, 0.1f);

        if (this.IsValid)
        {
            laneObject = SolidCurve.Generate(this, spriteSampler);
        }

        void OnShapePrValueChanged(object sender, int e)
        {
            GameObject.Destroy(laneObject);
            if (this.IsValid)
            {
                Debug.Log("repaint: " + xz_source + " " + y_source + " @step=" + StepSize);
                laneObject = SolidCurve.Generate(this, spriteSampler);
            }
        }

        xz_source.OnShapeChanged += OnShapePrValueChanged;
        y_source.OnValueChanged += OnShapePrValueChanged;

        PrimaryLane = this;
    }


}
