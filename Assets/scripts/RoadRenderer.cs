using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Separator{
	public Texture texture;
	public bool dashed;
	public float offset;
}

public class RoadRenderer : MonoBehaviour
{

    public GameObject rend;
    public static float laneWidth = 2.8f;
    public static float separatorWidth = 0.2f;
    public static float separatorInterval = 0.2f;

    public float dashLength = 4f;
    public float dashInterval = 6f;

    public Texture yellow, white, road, delete;

    public void generate(Curve curve, List<string> laneConfig, float margin_0 = 0f, float margin_1 = 0f)
    {
        List<Separator> separators = new List<Separator>();
        float offset = 0f;

        foreach (string l in laneConfig)
        {
            string[] configs = l.Split('_');
            if (configs[0] == "lane" || configs[0] == "interval" || configs[0] == "surface" || configs[0] == "removal")
            {

                switch (configs[0])
                {
                    case "lane":
                        offset += laneWidth;
                        break;
                    case "interval":
                        offset += separatorInterval;
                        break;
                    case "surface":
                        offset += float.Parse(configs[1]);
                        break;
                    case "removal":
                        offset += float.Parse(configs[1]);
                        drawRemovalMark(curve, offset);
                        return;
                }
            }
            else
            {
                string septype, sepcolor;
                septype = configs[0];
                sepcolor = configs[1];

                Separator sep = new Separator();

                switch (sepcolor)
                {
                    case "yellow":
                        sep.texture = yellow;
                        break;
                    case "white":
                        sep.texture = white;
                        break;
                }

                switch (septype)
                {
                    case "dash":
                        sep.dashed = true;
                        break;
                    case "solid":
                        sep.dashed = false;
                        break;
                }

                sep.offset = offset;

                separators.Add(sep);

                offset += separatorWidth;
            }
        }

        //adjust center
        for (int i = 0; i != separators.Count; i++)
        {
            separators[i].offset -= offset / 2;
            drawSeparator(curve, separators[i], margin_0, margin_1);
        }

        drawRoadSurface(curve, offset);

    }

    public void remove()
    {

    }

	void drawSeparator(Curve curve, Separator sep, float margin_0 = 0f, float margin_1 = 0f){
        if (curve.length > 0)
        {
            curve = curve.cut(margin_0 / curve.length, 1f - margin_1 / curve.length);
        }
		if (!sep.dashed) {
			GameObject rendins = Instantiate (rend, transform);
            rendins.transform.parent = this.transform;
			CurveRenderer decomp = rendins.GetComponent<CurveRenderer> ();
			decomp.CreateMesh (curve, separatorWidth, sep.texture, sep.offset + separatorWidth / 2, z_offset:0.01f);
		}
		else {
            List<Curve> dashed = curve.segmentation (dashLength + dashInterval);
			foreach (Curve singledash in dashed) {
                List<Curve> vacant_and_dashed = singledash.segmentation (dashInterval);
                Debug.Assert (vacant_and_dashed.Count <= 2);


                if (vacant_and_dashed.Count == 2) {
					GameObject rendins = Instantiate (rend, transform);
					CurveRenderer decomp = rendins.GetComponent<CurveRenderer> ();
					decomp.CreateMesh (vacant_and_dashed [1], separatorWidth, sep.texture, sep.offset + separatorWidth / 2, z_offset:0.01f);
				}

			}

		}
	}

	void drawRoadSurface(Curve curve, float width){
		GameObject rendins = Instantiate (rend, transform);
        rendins.transform.parent = this.transform;
		CurveRenderer decomp = rendins.GetComponent<CurveRenderer> ();
		decomp.CreateMesh (curve, width, road);
	}

    void drawRemovalMark(Curve curve, float width){
        GameObject rendins = Instantiate(rend, transform);
        rendins.transform.parent = this.transform;
        CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
        decomp.CreateMesh(curve, width, delete, z_offset:0.02f);
    }
}
