using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//object that is rendered in a continuous manner
class Linear3DObject{
	public Material linearMaterial, crossMaterial;
    public float dashLength, dashInterval;
    public Polygon cross_section;
    public float offset;
    public Linear3DObject(string name, float param = 0f){
        //TODO: read config dynamically
        //TODO: non-symmetry case
        switch (name)
        {
            case "fence":
                crossMaterial = Resources.Load<Material>("Materials/roadBarrier1");
                linearMaterial = Resources.Load<Material>("Materials/roadBarrier2");
                Vector2 P0 = new Vector2(0.1f, 0f);
                Vector2 P1 = new Vector2(0.1f, 1.0f);
                Vector2 P2 = new Vector2(0f, 1.0f);
                Vector2 P3 = new Vector2(-0.1f, 1.0f);
                Vector2 P4 = new Vector2(-0.1f, 0f);
                cross_section = new Polygon(new List<Curve> { new Line(P0, P1), new Arc(P2, P1, Mathf.PI), new Line(P3, P4), new Line(P4, P0) });
                dashLength = 0.2f;
                dashInterval = 2f;
                break;
            case "squarecolumn":
                linearMaterial = crossMaterial = Resources.Load<Material>("Materials/concrete");
                cross_section = new Polygon(new List<Curve>{
                    new Line(new Vector2(0.5f, 0f), new Vector2(-0.5f, 0f)),
                    new Line(new Vector2(-0.5f, 0f), new Vector2(-0.5f, -1f)),
                    new Line(new Vector2(-0.5f, -1f), new Vector2(0.5f, -1f)),
                    new Line(new Vector2(0.5f, -1f), new Vector2(0.5f, 0f))
                },
                                            new List<float>{
                    0f, 
                    0f,
                    -1.0f,
                    -1.0f
                }
                );

                dashLength = 1f;
                dashInterval = 8f;
                break;
            case "crossbeam":
                Debug.Assert(param > 0);
                linearMaterial = crossMaterial = Resources.Load<Material>("Materials/concrete");
                cross_section = new Polygon(new List<Curve>{
                    new Line(new Vector2(param/2, 0f), new Vector2(-param/2, 0f)),
                    new Line(new Vector2(-param/2, 0f), new Vector2(-param/2, -1f)),
                    new Line(new Vector2(-param/2, -1f), new Vector2(-1f, -1.3f)),
                    new Line(new Vector2(-1f, -1.3f), new Vector2(1f, -1.3f)),
                    new Line(new Vector2(1f, -1.3f), new Vector2(param/2, -1f)),
                    new Line(new Vector2(param/2, -1f), new Vector2(param/2, 0f))
                });
                dashLength = 1f;
                dashInterval = 8f;
                break;
            case "bridgepanel":
                Debug.Assert(param > 0);
                linearMaterial = Resources.Load<Material>("Materials/roadsurface");
                crossMaterial = Resources.Load<Material>("Materials/concrete");
                cross_section = new Polygon(new List<Curve>
                {
                    new Line(new Vector2(param/2, 0f), new Vector2(-param/2, 0f)),
                    new Line(new Vector2(-param/2, 0f), new Vector2(-param/2, -0.2f)),
                    new Line(new Vector2(-param/2, -0.2f), new Vector2(param/2, -0.2f)),
                    new Line(new Vector2(param/2, -0.2f), new Vector2(param/2, 0f))
                });
                dashInterval = 0f;
                break;
            default:
                break;
        }
    }
}

//objects rendered in discontinues manner
class NonLinear3DObject{
    public GameObject obj;
    public float interval;
}

class Separator
{
    public Texture texture;
    public bool dashed;
    public float offset;
}

/*should support:
lane 
interval
surface_{width}
removal_{width}

yellow/white/blueindi_dash/solid

should also support:
barrier
*/
public class RoadRenderer : MonoBehaviour
{

    public GameObject rend;
    public static float laneWidth = 2.8f;
    public static float separatorWidth = 0.2f;
    public static float separatorInterval = 0.2f;
    public static float fenceWidth = 0.2f;

    public float dashLength = 4f;
    public float dashInterval = 6f;

    public float dashIndicatorLength = 1f;
    public float dashIndicatorWidth = 2f;

    public void generate(Curve curve, List<string> laneConfig, 
                         float indicatorMargin_0 = 0f, float indicatorMargin_1 = 0f, float surfaceMargin_0 = 0f, float surfaceMargin_1 = 0f,
                         bool indicator = false)
    {
        List<Separator> separators = new List<Separator>();
        List<Linear3DObject> linear3DObjects = new List<Linear3DObject>();
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
                if (configs[0] == "fence")
                {
                    Linear3DObject fence = new Linear3DObject("fence");
                    fence.offset = offset;
                    linear3DObjects.Add(fence);

                    offset += fenceWidth;
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
                            sep.texture = Resources.Load<Texture>("Textures/yellow");
                            break;
                        case "white":
                            sep.texture = Resources.Load<Texture>("Textures/white");
                            break;
                        case "blueindi":
                            sep.texture = Resources.Load<Texture>("Textures/blue");
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
        }

        //adjust center
        for (int i = 0; i != separators.Count; i++)
        {
            separators[i].offset -= offset / 2;
            drawSeparator(curve, separators[i], indicatorMargin_0, indicatorMargin_1);
        }
        for (int i = 0; i != linear3DObjects.Count; i++){
            linear3DObjects[i].offset -= offset / 2;
            drawLinear3DObject(curve, linear3DObjects[i], indicatorMargin_0, indicatorMargin_1);
        }


        if (curve.z_start > 0 || curve.z_offset > 0){
            drawLinear3DObject(curve, new Linear3DObject("squarecolumn"), indicatorMargin_0, indicatorMargin_1);
            drawLinear3DObject(curve, new Linear3DObject("crossbeam", offset), indicatorMargin_0, indicatorMargin_1);
            drawLinear3DObject(curve, new Linear3DObject("bridgepanel", offset), indicatorMargin_0, indicatorMargin_1);
        }
        else{
            drawRoadSurface(curve, offset, surfaceMargin_0, surfaceMargin_1, indicator);
        }

    }

	void drawSeparator(Curve curve, Separator sep, float margin_0 = 0f, float margin_1 = 0f){
        if (curve.length > 0 && (margin_0 > 0 || margin_1 > 0))
        {
            curve = curve.cut(margin_0 / curve.length, 1f - margin_1 / curve.length);
        }
		if (!sep.dashed) {
			GameObject rendins = Instantiate (rend, transform);
            rendins.transform.parent = this.transform;
			CurveRenderer decomp = rendins.GetComponent<CurveRenderer> ();
            Material normalMaterial = new Material(Shader.Find("Standard"));
            normalMaterial.mainTexture = sep.texture;
            decomp.CreateMesh (curve, separatorWidth, normalMaterial, offset: sep.offset + separatorWidth / 2, z_offset:0.01f);
		}
		else {
            List<Curve> dashed = curve.segmentation (dashLength + dashInterval);
			foreach (Curve singledash in dashed) {
                List<Curve> vacant_and_dashed = singledash.split(dashInterval / (dashLength + dashInterval), byLength:true);

                if (vacant_and_dashed.Count == 2) {
					GameObject rendins = Instantiate (rend, transform);
					CurveRenderer decomp = rendins.GetComponent<CurveRenderer> ();
                    Material normalMaterial = new Material(Shader.Find("Standard"));
                    normalMaterial.mainTexture = sep.texture;
                    decomp.CreateMesh (vacant_and_dashed [1], separatorWidth, normalMaterial, offset:sep.offset + separatorWidth / 2, z_offset:0.01f);
				}

			}

		}
	}

    void drawLinear3DObject(Curve curve, Linear3DObject obj, float margin_0 = 0f, float margin_1 = 0f){

        if (curve.length > 0 && (margin_0 > 0 || margin_1 > 0)){
            curve = curve.cut(margin_0 / curve.length, 1f - margin_1 / curve.length);
        }
        if (obj.dashInterval == 0f)
        {
            GameObject rendins = Instantiate(rend, transform);
            rendins.transform.parent = this.transform;
            CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
            decomp.CreateMesh(curve, obj.offset + fenceWidth / 2, obj.linearMaterial, obj.crossMaterial, obj.cross_section);
        }
        else
        {
            Debug.Assert(obj.dashLength > 0f);
            List<Curve> dashed = curve.segmentation(obj.dashLength + obj.dashInterval);
            foreach (Curve singledash in dashed)
            {
                List<Curve> vacant_and_dashed = singledash.split(obj.dashInterval / (obj.dashLength + obj.dashInterval), byLength: true);
                if (vacant_and_dashed.Count == 2)
                {
                    GameObject rendins = Instantiate(rend, transform);
                    rendins.transform.parent = this.transform;
                    CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
                    decomp.CreateMesh(vacant_and_dashed[1], obj.offset + fenceWidth / 2, obj.linearMaterial, obj.crossMaterial, obj.cross_section);
                }
            }
        }
    }

    void drawRoadSurface(Curve curve, float width, float surfacemargin_0 = 0f, float surfacemargin_1 = 0f, bool indicator = false){
        if (curve.length > 0 && (surfacemargin_0 > 0 || surfacemargin_1 > 0))
        {
            curve = curve.cut(surfacemargin_0 / curve.length, 1f - surfacemargin_1 / curve.length);
        }
		GameObject rendins = Instantiate (rend, transform);
        rendins.transform.parent = this.transform;
		CurveRenderer decomp = rendins.GetComponent<CurveRenderer> ();
        if (indicator)
        {
            Material transMaterial = new Material(Shader.Find("Transparent/Diffuse"));
            transMaterial.color = new Color(0.25f, 0.75f, 0.75f, 0.6f);
            decomp.CreateMesh(curve, width, transMaterial);
        }
        else
        {
            Material normalMaterial = Resources.Load<Material>("Materials/roadsurface");
            decomp.CreateMesh(curve, width, normalMaterial);
        }
    }

    void drawRemovalMark(Curve curve, float width){
        GameObject rendins = Instantiate(rend, transform);
        rendins.transform.parent = this.transform;
        CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
        Material normalMaterial = new Material(Shader.Find("Standard"));
        normalMaterial.mainTexture = Resources.Load<Texture>("Textures/orange");
        decomp.CreateMesh(curve, width, normalMaterial, z_offset:0.02f);
    }

}
