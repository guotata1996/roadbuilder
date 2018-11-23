using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//object that is rendered in a continuous manner
class Linear3DObject{
	public Material linearMaterial, crossMaterial;
    public float dashLength, dashInterval;
    public Polygon cross_section;
    public float offset;
    public float margin_0, margin_1;
    public string tag;
    public Linear3DObject(string name, float param = 0f){
        //TODO: read config dynamically
        //TODO: non-symmetry case
        tag = name;
        margin_0 = margin_1 = offset = 0f;
        switch (name)
        {
            case "fence":
                {
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
                }
                break;
            case "lowbar":
                {
                    crossMaterial = linearMaterial = Resources.Load<Material>("Materials/white");
                    Vector2 P0 = new Vector2(0f, 0.4f);
                    Vector2 P1 = new Vector2(0.1f, 0.4f);
                    Vector2 P2 = new Vector2(-0.1f, 0.4f);
                    cross_section = new Polygon(new List<Curve> { new Arc(P0, P1, Mathf.PI), new Arc(P0, P2, Mathf.PI) });
                    dashInterval = 0f;
                }
                break;
            case "highbar":
                {
                    crossMaterial = linearMaterial = Resources.Load<Material>("Materials/white");
                    Vector2 P0 = new Vector2(0f, 0.9f);
                    Vector2 P1 = new Vector2(0.1f, 0.9f);
                    Vector2 P2 = new Vector2(-0.1f, 0.9f);
                    cross_section = new Polygon(new List<Curve> { new Arc(P0, P1, Mathf.PI), new Arc(P0, P2, Mathf.PI) });
                    dashInterval = 0f;
                }
                break;

            case "squarecolumn":
                linearMaterial = crossMaterial = Resources.Load<Material>("Materials/concrete");
                cross_section = new Polygon(new List<Curve>{
                    new Line(new Vector2(0.5f, -0.2f), new Vector2(-0.5f, -0.2f)),
                    new Line(new Vector2(-0.5f, -0.2f), new Vector2(-0.5f, -1f)),
                    new Line(new Vector2(-0.5f, -1f), new Vector2(0.5f, -1f)),
                    new Line(new Vector2(0.5f, -1f), new Vector2(0.5f, -0.2f))
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
                linearMaterial = crossMaterial = Resources.Load<Material>("Materials/concrete");
                if (param > 0f)
                {
                    setParam(param);
                }
                dashLength = 1f;
                dashInterval = 8f;
                break;
            case "bridgepanel":
                linearMaterial = Resources.Load<Material>("Materials/roadsurface");
                crossMaterial = Resources.Load<Material>("Materials/concrete");
                if (param > 0f){
                    setParam(param);
                }
                dashInterval = 0f;
                break;
            default:
                break;
        }
    }

    public void setParam(float param){
        switch(tag){
            case "crossbeam":
                cross_section = new Polygon(new List<Curve>{
                        new Line(new Vector2(param/2, -0.2f), new Vector2(-param/2, -0.2f)),
                        new Line(new Vector2(-param/2, -0.2f), new Vector2(-param/2, -1f)),
                        new Line(new Vector2(-param/2, -1f), new Vector2(-1f, -1.3f)),
                        new Line(new Vector2(-1f, -1.3f), new Vector2(1f, -1.3f)),
                        new Line(new Vector2(1f, -1.3f), new Vector2(param/2, -1f)),
                        new Line(new Vector2(param/2, -1f), new Vector2(param/2, -0.2f))
                    });
                break;
            case "bridgepanel":
                cross_section = new Polygon(new List<Curve>
                {
                    new Line(new Vector2(param/2, 0f), new Vector2(-param/2, 0f)),
                    new Line(new Vector2(-param/2, 0f), new Vector2(-param/2, -0.2f)),
                    new Line(new Vector2(-param/2, -0.2f), new Vector2(param/2, -0.2f)),
                    new Line(new Vector2(param/2, -0.2f), new Vector2(param/2, 0f))
                });
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
    public float offset = 0f;
    public float margin_0 = 0f, margin_1 = 0f;
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

    static List<string> commonTypes = new List<string> { "lane", "interval", "surface", "removal", "fence", "column" };

    public void generate(Curve curve, List<string> laneConfig,
                         float indicatorMargin_0L = 0f, float indicatorMargin_0R = 0f, float indicatorMargin_1L = 0f, float indicatorMargin_1R = 0f){

        Debug.Assert(indicatorMargin_0L < curve.length - indicatorMargin_1L);
        Debug.Assert(indicatorMargin_0R < curve.length - indicatorMargin_1R);
        float indicatorMargin_0Bound = Mathf.Max(indicatorMargin_0L, indicatorMargin_0R);
        float indicatorMargin_1Bound = Mathf.Max(indicatorMargin_1L, indicatorMargin_1R);

        if (Algebra.isclose(curve.z_offset, 0f) || (indicatorMargin_1Bound == 0f && indicatorMargin_1Bound == 0f)){
            //Debug.Log("generating single in the first place with 0= " + indicatorMargin_0 + " 1= " + indicatorMargin_1);
            generateSingle(curve, laneConfig, indicatorMargin_0L, indicatorMargin_0R, indicatorMargin_1L, indicatorMargin_1R);
            return;
        }
        else{
            Debug.Log(curve + " 0L= " + indicatorMargin_0L + " 0R= " + indicatorMargin_0R + "\n1L="
                      + indicatorMargin_1L + " 1R= " + indicatorMargin_1R);
            if (indicatorMargin_0Bound > 0){
                Curve margin0Curve = curve.cut(0f, indicatorMargin_0Bound / curve.length);
                margin0Curve.z_start = curve.at(0f).y;
                margin0Curve.z_offset = 0f;
                generateSingle(margin0Curve, laneConfig, indicatorMargin_0L, indicatorMargin_0R, 0f, 0f);
            }
            Curve middleCurve = curve.cut(indicatorMargin_0Bound / curve.length, 1f - indicatorMargin_1Bound / curve.length);
            middleCurve.z_start = curve.at(0f).y;
            middleCurve.z_offset = curve.at(1f).y - curve.at(0f).y;
            generateSingle(middleCurve, laneConfig, 0f, 0f, 0f, 0f);
            //Debug.Log("generating single in the 2nd place");

            if (indicatorMargin_1Bound > 0){
                Curve margin1Curve = curve.cut(1f - indicatorMargin_1Bound / curve.length, 1f);
                margin1Curve.z_start = curve.at(1f).y;
                margin1Curve.z_offset = 0f;
                generateSingle(margin1Curve, laneConfig, 0f, 0f,indicatorMargin_1L, indicatorMargin_1R);
            }
        }
    }

    void generateSingle(Curve curve, List<string> laneConfig, 
                        float indicatorMargin_0L , float indicatorMargin_0R, float indicatorMargin_1L, float indicatorMargin_1R)
    {
        List<Separator> separators = new List<Separator>();
        List<Linear3DObject> linear3DObjects = new List<Linear3DObject>();
        float width = getConfigureWidth(laneConfig);

        for (int i = 0; i != laneConfig.Count; ++i)
        {
            string l = laneConfig[i];
            float partialWidth = getConfigureWidth(laneConfig.GetRange(0, i + 1));
            string[] configs = l.Split('_');
            if (commonTypes.Contains(configs[0]))
            {
                switch (configs[0])
                {
                    case "lane":
                    case "interval":
                        break;
                    case "surface":
                        Debug.Assert(configs.Length == 1);
                        Linear3DObject roadBlock = new Linear3DObject("bridgepanel");
                        linear3DObjects.Add(roadBlock);
                        break;
                    case "column":
                        Linear3DObject squarecolumn = new Linear3DObject("squarecolumn");
                        Linear3DObject crossbeam = new Linear3DObject("crossbeam");
                        squarecolumn.margin_0 = crossbeam.margin_0 =
                            Algebra.Lerp(indicatorMargin_0L, indicatorMargin_0R, partialWidth / width);
                        squarecolumn.margin_1 = crossbeam.margin_1 =
                            Algebra.Lerp(indicatorMargin_1L, indicatorMargin_1R, partialWidth / width);

                        linear3DObjects.Add(squarecolumn);
                        linear3DObjects.Add(crossbeam);
                        break;
                    case "fence":
                        Linear3DObject fence = new Linear3DObject("fence");
                        Linear3DObject lowbar = new Linear3DObject("lowbar");
                        Linear3DObject highbar = new Linear3DObject("highbar");

                        fence.offset = lowbar.offset = highbar.offset = partialWidth - fenceWidth / 2;
                        fence.margin_0 = lowbar.margin_0 = highbar.margin_0 =
                            Algebra.Lerp(indicatorMargin_0L, indicatorMargin_0R, partialWidth / width);
                        fence.margin_1 = lowbar.margin_1 = highbar.margin_1 =
                            Algebra.Lerp(indicatorMargin_1L, indicatorMargin_1R, partialWidth / width);

                        linear3DObjects.Add(fence);
                        linear3DObjects.Add(lowbar);
                        linear3DObjects.Add(highbar);
                        break;
                    case "removal":
                        Debug.Assert(laneConfig.Count == 1);
                        drawRemovalMark(curve, float.Parse(configs[1]));
                        return;
                }
            }
            else
            {
                string septype, sepcolor;
                septype = configs[0];
                sepcolor = configs[1];
                
                Separator sep = new Separator();
                sep.margin_0 = Algebra.Lerp(indicatorMargin_0L, indicatorMargin_0R, partialWidth / width);
                sep.margin_1 = Algebra.Lerp(indicatorMargin_1L, indicatorMargin_1R, partialWidth / width);

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

                sep.offset = partialWidth - separatorWidth;

                separators.Add(sep);
            }

        }

        //adjust center

        for (int i = 0; i != separators.Count; i++)
        {
            separators[i].offset -= width / 2;
            drawLinear2DObject(curve, separators[i]);
        }


        for (int i = 0; i != linear3DObjects.Count; i++)
        {
            {
                Linear3DObject obj = linear3DObjects[i];
                switch(obj.tag){
                    case "crossbeam":
                        if ((curve.z_start > 0 || curve.z_offset > 0)) {
                            linear3DObjects[i].setParam(width);
                            drawLinear3DObject(curve, linear3DObjects[i]);
                        }
                        break;
                    case "squarecolumn":
                        if ((curve.z_start > 0 || curve.z_offset > 0)){
                            drawLinear3DObject(curve, linear3DObjects[i]);
                        }
                        break;
                    case "bridgepanel":
                        linear3DObjects[i].setParam(width);
                        drawLinear3DObject(curve, linear3DObjects[i]);

                        break;
                    default:
                        linear3DObjects[i].offset -= width / 2;

                        if ((curve.z_start > 0 || curve.z_offset > 0))
                        {
                            drawLinear3DObject(curve, linear3DObjects[i]);
                        }
                        break;
                }
            }
        }


    }

	void drawLinear2DObject(Curve curve, Separator sep){
        float margin_0 = sep.margin_0;
        float margin_1 = sep.margin_1;
        if (Algebra.isclose(margin_0 + margin_1, curve.length)){
            return;
        }
        Debug.Assert(margin_0 + margin_1 < curve.length);

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

    void drawLinear3DObject(Curve curve, Linear3DObject obj){
        float margin_0 = obj.margin_0;
        float margin_1 = obj.margin_1;
        if (Algebra.isclose(margin_0 + margin_1, curve.length))
        {
            return;
        }
        Debug.Assert(margin_0 >= 0 && margin_1 >= 0);
        Debug.Assert(margin_0 + margin_1 < curve.length);

        if (curve.length > 0 && (margin_0 > 0 || margin_1 > 0)){
            curve = curve.cut(margin_0 / curve.length, 1f - margin_1 / curve.length);
        }
        if (obj.dashInterval == 0f)
        {
            GameObject rendins = Instantiate(rend, transform);
            rendins.transform.parent = this.transform;
            CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
            decomp.CreateMesh(curve, obj.offset, obj.linearMaterial, obj.crossMaterial, obj.cross_section);
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
                    decomp.CreateMesh(vacant_and_dashed[1], obj.offset, obj.linearMaterial, obj.crossMaterial, obj.cross_section);
                }
            }
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

    public static float getConfigureWidth(List<string> laneconfigure){
        var ans = 0f;
        for (int i = 0; i != laneconfigure.Count; ++i)
        {
            if (commonTypes.Contains(laneconfigure[i].Split('_')[0]))
                switch (laneconfigure[i].Split('_')[0])
            {
                case "lane":
                    ans += laneWidth;
                    break;
                case "interval":
                    ans += separatorInterval;
                    break;
                case "fence":
                    ans += fenceWidth;
                    break;
            }
            else{
                ans += separatorWidth;
            }
        }
        return ans;
    }

    public void generate(Polygon polygon, float H, string materialconfig)
    {
        GameObject rendins = Instantiate(rend, transform);
        rendins.transform.parent = this.transform;
        CurveRenderer decomp = rendins.GetComponent<CurveRenderer>();
        switch (materialconfig)
        {
            default:
                decomp.CreateMesh(polygon, H, Resources.Load<Material>("Materials/roadsurface"));
                break;
        }   
    }
}
