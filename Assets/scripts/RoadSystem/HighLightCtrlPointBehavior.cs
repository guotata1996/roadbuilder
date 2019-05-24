using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreLinq;

public class HighLightCtrlPointBehavior : MonoBehaviour
{
    [SerializeField]
    InputHandler input;

    /// <summary>
    /// The radius of heighlighted area. 
    /// If set to 0, only the closest would be highlighted.
    /// </summary>
    [HideInInspector]
    public float radius;

    private void Update()
    {
        foreach (var g in GameObject.FindGameObjectsWithTag("Road/curveIndicator"))
        {
            Destroy(g);
        }

        if (radius == 0f)
        {
            Lane l = RoadQueryByPosition.QueryClosest(input.MousePosition);
            if (l == null)
            {
                return;
            }
            Vector3 p = l.ControlPoints.MinBy((Vector3 cp) => (cp - input.MousePosition).magnitude);

            var indi = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indi.transform.position = p;
            indi.transform.localScale = Vector3.one * 0.3f;
            indi.tag = "Road/curveIndicator";
        }
        else
        {
            var ls = RoadQueryByPosition.Query(input.MousePosition, radius);
            foreach(Lane l in ls)
            {
                foreach(var cp in l.ControlPoints)
                {
                    var indi = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    indi.transform.position = cp;
                    indi.transform.localScale = Vector3.one * 0.3f;
                    indi.tag = "Road/curveIndicator";
                }
            }
        }

    }


}
