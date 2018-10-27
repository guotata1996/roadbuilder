using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Pair<T, U>
{
    public Pair()
    {
    }

    public Pair(T first, U second)
    {
        this.First = first;
        this.Second = second;
    }

    public T First { get; set; }
    public U Second { get; set; }
};

public class Node : MonoBehaviour
{
    public List<Pair<Road, Pair<float, float>>> connection = new List<Pair<Road, Pair<float, float>>>();

    public Vector3 position;

    public GameObject roadCornerIndicator;
    
    RoadDrawing indicatorInst;
    
    public float crossingSmoothScale;

    public float laneChangeSmoothScale;

    List<GameObject> smoothInstances = new List<GameObject>();

    public void Awake()
    {
        indicatorInst = GameObject.FindWithTag("Road/curveIndicator").GetComponent<RoadDrawing>();
    }

    public Vector2 twodPosition{
        get{
            return new Vector2(position.x, position.z);
        }
    }

    public bool containsRoad(Road r){
        foreach(var rmPair in connection){
            if (rmPair.First == r){
                return true;
            }
        }
        return false;
    }

    public Pair<float, float> getMargin(Road r){
        for (int i = 0; i != connection.Count; ++i){
            if (connection[i].First == r){
                return connection[i].Second;
            }
        }
        Debug.Assert(false);
        return new Pair<float, float>(0f, 0f);
    }

    public void addRoad(Road road)
    {
        connection.Add(new Pair<Road, Pair<float, float>>(road, new Pair<float, float>(0f, 0f)));

        updateCrossroads();
    }

    public void removeRoad(Road road)
    {
        Pair<Road, Pair<float, float>> target = null;
        foreach (Pair<Road, Pair<float, float>> pair in connection){
            if (pair.First == road){
                target = pair;
                break;
            }
        }

        if (target != null)
        {
            connection.Remove(target);
        }
        else{
            Debug.Assert(false);
        }
        if (connection.Count > 0)
        {
            updateCrossroads();
        }
    }

    //returns one directional line for each of the road connecting to this node.
    public List<Curve> directionalLines(float length, bool reverse = false){
        List<Curve> rtn = new List<Curve>();
        foreach(Pair<Road, Pair<float, float>> connect in connection){
            Vector2 direction = connect.First.curve.direction_ending_2d(startof(connect.First.curve));
            if (reverse){
                direction = -direction;
            }
            rtn.Add(new Line(twodPosition, twodPosition + direction * Algebra.InfLength, 0f, 0f));
        }
        return rtn;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        Node n = obj as Node;
        return n.position == this.position;
    }

    void updateCrossroads(){
        foreach (GameObject smoothinst in smoothInstances){
            Destroy(smoothinst);
        }
        smoothInstances.Clear();

        connection = connection.OrderBy(r =>
                                        startof(r.First.curve) ?
                                        r.First.curve.angle_ending(true) : r.First.curve.angle_ending(false)).ToList();

        foreach (var rmPair in connection){
            rmPair.Second = new Pair<float, float>(0f, 0f);
        }

        if (connection.Count > 1)
        {
            for (int i = 0; i != connection.Count; ++i)
            {
                int i_hat = (i == connection.Count - 1) ? 0 : i + 1;
                var margins = smoothenCrossing(connection[i].First, connection[i_hat].First);
                connection[i].Second.First = Mathf.Max(connection[i].Second.First, Mathf.Abs(margins.First));
                connection[i_hat].Second.First = Mathf.Max(connection[i_hat].Second.First, Mathf.Abs(margins.Second));
                if (margins.First < 0)
                {
                    //>=180 degrees intersection
                    connection[i].Second.Second = Mathf.Abs(margins.First);
                    connection[i_hat].Second.Second = Mathf.Abs(margins.Second);
                    if (connection.Count == 2)
                    {
                        break;
                    }
                }

            }
        }
    }


    /*
    float minCrossingRadius(Road r1, Road r2){
        float r1_angle = startof(r1.curve) ? r1.curve.angle_ending(true) : r1.curve.angle_ending(false);
        float r2_angle = startof(r2.curve) ? r2.curve.angle_ending(true) : r2.curve.angle_ending(false);
        float delta_angle = r1_angle - r2_angle;
        if (Algebra.isclose(Mathf.Sin(delta_angle), 0f)){
            return 0f;
        }
        else{
            float w1 = r2.width / 2;
            float w2 = r2.width / 2;
            return Mathf.Sqrt(w1 * w1 / (Mathf.Sin(delta_angle) * Mathf.Sin(delta_angle)) +
                              2 * w1 * w2 * Mathf.Cos(delta_angle) / (Mathf.Sin(delta_angle) * Mathf.Sin(delta_angle))
                              + w2 * w2 / (Mathf.Sin(delta_angle) * Mathf.Sin(delta_angle)));
        }
    }
    */

    bool startof(Curve c){
        return (c.at(0f) - position).magnitude < (c.at(1f) - position).magnitude;
    }

    /*if retured value is negative, surface margin is needed*/
    Pair<float, float> smoothenCrossing(Road r1, Road r2)
    {
        float r1_angle = startof(r1.curve) ? r1.curve.angle_ending(true) : r1.curve.angle_ending(false);
        float r2_angle = startof(r2.curve) ? r2.curve.angle_ending(true) : r2.curve.angle_ending(false);
        float delta_angle = r1_angle < r2_angle ? r2_angle - r1_angle : r2_angle + 2 * Mathf.PI - r1_angle;
        if (Algebra.isclose(delta_angle, Mathf.PI)){
            //smoothen with Beizier Line
            if (r1.width == r2.width){
                //TODO Combine two frags
                return new Pair<float, float>(0f, 0f);
            }
            Road wideRoad = r1.width > r2.width ? r1 : r2;
            Road narrowRoad = r1.width > r2.width ? r2 : r1;
            float halfsmoothenLength = (wideRoad.width - narrowRoad.width) * laneChangeSmoothScale / 2f;
            int num_smootheners = Mathf.CeilToInt(wideRoad.width / narrowRoad.width);
            float Awide = wideRoad.curve.angle_ending(startof(wideRoad.curve), halfsmoothenLength);
            float Anarrow = narrowRoad.curve.angle_ending(startof(narrowRoad.curve), halfsmoothenLength);
            Vector2 Pwide_0 = wideRoad.curve.at_ending_2d(startof(wideRoad.curve), halfsmoothenLength) + Algebra.angle2dir(Awide + Mathf.PI / 2) * wideRoad.width / 2;
            Vector2 Pnarrow = narrowRoad.curve.at_ending_2d(startof(narrowRoad.curve), halfsmoothenLength);                                 
            
            for (int i = 0; i != num_smootheners - 1; ++i){
                Vector2 Pwide = Pwide_0 + Algebra.angle2dir(Awide - Mathf.PI / 2) * narrowRoad.width * (i + 0.5f);
                Curve smoothener1;
                Curve smoothener2;
                if (Geometry.Parallel(Pwide - Pnarrow, Algebra.angle2dir(Awide)))
                {
                    smoothener1 = new Line(Pwide, 0.5f * (Pwide + Pnarrow), 0f, 0f);
                    smoothener2 = new Line(0.5f * (Pwide + Pnarrow), Pnarrow, 0f, 0f);
                }
                else
                {
                    smoothener1 = new Bezeir(Pwide, Pwide - Algebra.angle2dir(Awide) * halfsmoothenLength / 2, 0.5f * (Pwide + Pnarrow), 0f, 0f);
                    smoothener2 = new Bezeir(0.5f * (Pwide + Pnarrow), Pnarrow - Algebra.angle2dir(Anarrow) * halfsmoothenLength / 2, Pnarrow, 0f, 0f);
                }
                GameObject smoothObj = Instantiate(indicatorInst.roadIndicatorPrefab, transform);
                smoothInstances.Add(smoothObj);
                RoadRenderer smoothObjConfig = smoothObj.GetComponent<RoadRenderer>();
                smoothObjConfig.generate(smoothener1, new List<string> { string.Format("surface_{0}", narrowRoad.width) });
                smoothObjConfig.generate(smoothener2, new List<string> { string.Format("surface_{0}", narrowRoad.width) });
            }

            Vector2 Pwide_1 = wideRoad.curve.at_ending_2d(startof(wideRoad.curve), halfsmoothenLength) + Algebra.angle2dir(Awide - Mathf.PI / 2) * 
                                      (wideRoad.width / 2 - narrowRoad.width / 2);
            Curve smoothener1_last = new Bezeir(Pwide_1, Pwide_1 - Algebra.angle2dir(Awide) * halfsmoothenLength / 2, 0.5f * (Pwide_1 + Pnarrow), 0f, 0f);
            Curve smoothener2_last = new Bezeir(0.5f * (Pwide_1 + Pnarrow), Pnarrow - Algebra.angle2dir(Anarrow) * halfsmoothenLength / 2, Pnarrow, 0f, 0f);
            GameObject smoothObj_last = Instantiate(indicatorInst.roadIndicatorPrefab, transform);
            smoothInstances.Add(smoothObj_last);
            RoadRenderer smoothObjConfig_last = smoothObj_last.GetComponent<RoadRenderer>();
            smoothObjConfig_last.generate(smoothener1_last, new List<string> { string.Format("surface_{0}", narrowRoad.width) });
            smoothObjConfig_last.generate(smoothener2_last, new List<string> { string.Format("surface_{0}", narrowRoad.width) });
            return new Pair<float, float>(-halfsmoothenLength, -halfsmoothenLength);
        }
        else{
            //smoothen with Arc

            if (delta_angle > Mathf.PI && !Algebra.isclose(delta_angle, 0f) && !Algebra.isclose(delta_angle, Mathf.PI * 2)){
                Road widerRoad = r1.width > r2.width ? r1 : r2;
                Road narrowerRoad = r1.width > r2.width ? r2 : r1;
                // the width could be same
                this.r1 = r1;
                this.r2 = r2;
                Vector2 streetCorner = approxStreetCorner();

                float smoothLength = (streetCorner - twodPosition).magnitude * this.crossingSmoothScale + Mathf.Max(Mathf.Max(-c1_offset, 0), Mathf.Max(-c2_offset, 0));

                float wide_curveIntersectAngle = widerRoad.curve.angle_ending(startof(widerRoad.curve), smoothLength);
                float narrow_curveIntersectAngle = narrowerRoad.curve.angle_ending(startof(narrowerRoad.curve), smoothLength);

                Vector2 wide_curveIntersect = widerRoad.curve.at_ending_2d(startof(widerRoad.curve), smoothLength) +
                                                       Algebra.angle2dir(wide_curveIntersectAngle + Mathf.PI / 2) * widerRoad.width / 2;

                int smoothener_count = Mathf.CeilToInt(widerRoad.width / narrowerRoad.width);
                for (int i = 0; i != smoothener_count; ++i)
                {
                    Vector2 wideP = wide_curveIntersect + Algebra.angle2dir(wide_curveIntersectAngle - Mathf.PI / 2) *
                                                                 ((i == smoothener_count - 1) ? widerRoad.width - narrowerRoad.width * 0.5f : (narrowerRoad.width * (i + 0.5f)));
                    Vector2 narrowP = narrowerRoad.curve.at_ending_2d(startof(narrowerRoad.curve), smoothLength);
                    Vector2 wideP_endPoint = wideP - Algebra.angle2dir(wide_curveIntersectAngle) * Algebra.InfLength;
                    Vector2 narrowP_endPoint = narrowP - Algebra.angle2dir(narrow_curveIntersectAngle) * Algebra.InfLength;
                    Vector2 bezier_midPoint = Geometry.curveIntersect(new Line(wideP, wideP_endPoint, 0f, 0f), new Line(narrowP, narrowP_endPoint, 0f, 0f))[0];

                    Curve smoothener = new Bezeir(wideP, bezier_midPoint, narrowP, 0f, 0f);
                    GameObject smoothObj = Instantiate(indicatorInst.roadIndicatorPrefab, transform);
                    smoothInstances.Add(smoothObj);
                    RoadRenderer smoothObjConfig = smoothObj.GetComponent<RoadRenderer>();
                    smoothObjConfig.generate(smoothener, new List<string> { string.Format("surface_{0}", narrowerRoad.width) });
                }
                return new Pair<float, float>(-smoothLength, -smoothLength);
            }
            else{
                this.r1 = r1;
                this.r2 = r2;
                Vector2 streetcorner = approxStreetCorner();
                float smoothLength = Mathf.Min((streetcorner - twodPosition).magnitude * this.crossingSmoothScale, 2 * Mathf.Min(r1.width, r2.width));

                float r1_curveIntersectAngle = r1.curve.angle_ending(startof(r1.curve), c1_offset);
                float r2_curveIntersectAngle = r2.curve.angle_ending(startof(r2.curve), c2_offset);
                Vector2 r1_curveIntersect = r1.curve.at_ending_2d(startof(r1.curve), c1_offset + smoothLength) +
                                              new Vector2(Mathf.Cos(r1_curveIntersectAngle + Mathf.PI / 2), Mathf.Sin(r1_curveIntersectAngle + Mathf.PI / 2)) * r1.width / 2;
                Vector2 r2_curveIntersect = r2.curve.at_ending_2d(startof(r2.curve), c2_offset + smoothLength) +
                                              new Vector2(Mathf.Cos(r2_curveIntersectAngle - Mathf.PI / 2), Mathf.Sin(r2_curveIntersectAngle - Mathf.PI / 2)) * r2.width / 2;
                float roadcornerHalfAngle = 0.5f * (r1_curveIntersectAngle < r2_curveIntersectAngle ? r2_curveIntersectAngle - r1_curveIntersectAngle :
                                             r2_curveIntersectAngle + 2 * Mathf.PI - r1_curveIntersectAngle);
                Vector2 smoothArcCenter = streetcorner + (r1_curveIntersect + r2_curveIntersect - 2 * streetcorner).normalized * smoothLength / Mathf.Cos(roadcornerHalfAngle);
                float smoothInnerRadius = smoothLength * Mathf.Tan(roadcornerHalfAngle);
                float smoothOuterRadius = smoothLength / Mathf.Cos(roadcornerHalfAngle);

                Road narrowerRoad = r1.width > r2.width ? r2 : r1;
                if (smoothOuterRadius <= smoothLength * Mathf.Tan(roadcornerHalfAngle) + narrowerRoad.width)
                {
                    Arc ac_without_width = new Arc(r1_curveIntersect, Mathf.PI - 2 * roadcornerHalfAngle, r2_curveIntersect, 0f, 0f);
                    float smoothwidth = (ac_without_width.center - streetcorner).magnitude - ac_without_width.radius;

                    Vector2 r1_curveIntersect_pluswidth = r1_curveIntersect + new Vector2(Mathf.Cos(r1_curveIntersectAngle - Mathf.PI / 2), Mathf.Sin(r1_curveIntersectAngle - Mathf.PI / 2)) * smoothwidth / 2;
                    Vector2 r2_curveIntersect_pluswidth = r2_curveIntersect + new Vector2(Mathf.Cos(r2_curveIntersectAngle + Mathf.PI / 2), Mathf.Sin(r2_curveIntersectAngle + Mathf.PI / 2)) * smoothwidth / 2;

                    GameObject smoothObj = Instantiate(indicatorInst.roadIndicatorPrefab, transform);
                    smoothInstances.Add(smoothObj);
                    RoadRenderer smoothObjConfig = smoothObj.GetComponent<RoadRenderer>();
                    smoothObjConfig.generate(new Arc(r1_curveIntersect_pluswidth, Mathf.PI - 2 * roadcornerHalfAngle, r2_curveIntersect_pluswidth, 0f, 0f),
                                             new List<string> { string.Format("surface_{0}", smoothwidth) });
                }
                else
                {
                    Arc curve1, curve2;
                    float smoothWidth1, smoothWidth2;
                    if (narrowerRoad == r1)
                    {
                        smoothWidth1 = r1.width;
                        smoothWidth2 = smoothOuterRadius - smoothInnerRadius;
                        float angle1 = Mathf.Acos((smoothInnerRadius + r1.width) / smoothOuterRadius);
                        curve1 = new Arc(smoothArcCenter, r1_curveIntersect + (r1_curveIntersect - smoothArcCenter).normalized * smoothWidth1 / 2, -angle1, 0f, 0f);
                        curve2 = new Arc(smoothArcCenter, r2_curveIntersect + (r2_curveIntersect - smoothArcCenter).normalized * smoothWidth2 / 2, (Mathf.PI - 2 * roadcornerHalfAngle - angle1), 0f, 0f);
                    }
                    else
                    {
                        smoothWidth1 = smoothOuterRadius - smoothInnerRadius;
                        smoothWidth2 = r2.width;
                        float angle2 = Mathf.Acos((smoothInnerRadius + r2.width) / smoothOuterRadius);
                        curve1 = new Arc(smoothArcCenter, r1_curveIntersect + (r1_curveIntersect - smoothArcCenter).normalized * smoothWidth1 / 2, angle2, 0f, 0f);
                        curve2 = new Arc(smoothArcCenter, r2_curveIntersect + (r2_curveIntersect - smoothArcCenter).normalized * smoothWidth2 / 2, -(Mathf.PI - 2 * roadcornerHalfAngle - angle2), 0f, 0f);
                    }
                    GameObject smoothObj = Instantiate(indicatorInst.roadIndicatorPrefab, transform);
                    smoothInstances.Add(smoothObj);
                    RoadRenderer smoothObjConfig = smoothObj.GetComponent<RoadRenderer>();
                    smoothObjConfig.generate(curve1, new List<string> { string.Format("surface_{0}", smoothWidth1) });
                    smoothObjConfig.generate(curve2, new List<string> { string.Format("surface_{0}", smoothWidth2) });
                }
                return new Pair<float, float>(c1_offset + smoothLength, c2_offset + smoothLength);
            }
        }


    }

    internal List<Vector2> getNeighborDirections(Vector2 direction)
    {

        if (connection.Count <= 2){
            return connection.ConvertAll((input) => input.First.curve.direction_ending_2d(startof(input.First.curve)));
        }

        List<float> anglesFromDirection =
            connection.ConvertAll((input) => Algebra.signedAngleToPositive(Vector2.SignedAngle(direction, input.First.curve.direction_ending_2d(startof(input.First.curve)))));

        return connection.FindAll((Pair<Road, Pair<float, float>> arg1) =>
                                  Algebra.signedAngleToPositive(Vector2.SignedAngle(direction, arg1.First.curve.direction_ending_2d(startof(arg1.First.curve)))) 
                                  == anglesFromDirection.Max() 
                                 ||
                                  Algebra.signedAngleToPositive(Vector2.SignedAngle(direction, arg1.First.curve.direction_ending_2d(startof(arg1.First.curve))))
                                  == anglesFromDirection.Min()).
                         ConvertAll((input) => input.First.curve.direction_ending_2d(startof(input.First.curve)));
                       

        return new List<Vector2>() { Algebra.angle2dir(anglesFromDirection.Max()), Algebra.angle2dir(anglesFromDirection.Min())};
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(streetcorner.x, 2f, streetcorner.y), 0.1f);
    }
    */

    Road r1, r2;
    float c1_offset, c2_offset;

    float C1sidepointToC2Sidepoint(float l1)
    {
        float c2_tan_angle = r2.curve.angle_ending(startof(r2.curve), offset: c2_offset);
        Vector2 c2_normDir = new Vector2(Mathf.Cos(c2_tan_angle - Mathf.PI / 2), Mathf.Sin(c2_tan_angle - Mathf.PI / 2));
        float c1_tan_angle = r1.curve.angle_ending(startof(r1.curve), offset: l1);
        Vector2 c1_normDir = new Vector2(Mathf.Cos(c1_tan_angle + Mathf.PI / 2), Mathf.Sin(c1_tan_angle + Mathf.PI / 2));
        return ((r1.curve.at_ending_2d(startof(r1.curve), l1) + c1_normDir * r1.width / 2f) -
                (r2.curve.at_ending_2d(startof(r2.curve), c2_offset) + c2_normDir * r2.width / 2f)).magnitude;
    }

    float C2sidepointToC1Sidepoint(float l2)
    {
        float c2_tan_angle = r2.curve.angle_ending(startof(r2.curve), offset: l2);
        Vector2 c2_normDir = new Vector2(Mathf.Cos(c2_tan_angle - Mathf.PI / 2), Mathf.Sin(c2_tan_angle - Mathf.PI / 2));
        float c1_tan_angle = r1.curve.angle_ending(startof(r1.curve), offset: c1_offset);
        Vector2 c1_normDir = new Vector2(Mathf.Cos(c1_tan_angle + Mathf.PI / 2), Mathf.Sin(c1_tan_angle + Mathf.PI / 2));
        return ((r1.curve.at_ending_2d(startof(r1.curve), c1_offset) + c1_normDir * r1.width / 2f) -
                (r2.curve.at_ending_2d(startof(r2.curve), l2) + c2_normDir * r2.width / 2f)).magnitude;
    }

    Vector2 approxStreetCorner(){
        c1_offset = 0f;
        c2_offset = 0f;

        while (true){
            float c1_new_offset = Algebra.minArg(C1sidepointToC2Sidepoint, c1_offset);
            float c1_diff = Mathf.Abs(c1_offset - c1_new_offset);
            c1_offset = c1_new_offset;

            float c2_new_offset = Algebra.minArg(C2sidepointToC1Sidepoint, c2_offset);
            float c2_diff = Mathf.Abs(c2_offset - c2_new_offset);
            c2_offset = c2_new_offset;
            if (c1_diff + c2_diff < 1e-3)
                break;
        }
        float c2_tan_angle = r2.curve.angle_ending(startof(r2.curve), offset: c2_offset);
        Vector2 c2_normDir = new Vector2(Mathf.Cos(c2_tan_angle - Mathf.PI / 2), Mathf.Sin(c2_tan_angle - Mathf.PI / 2));

        return r2.curve.at_ending_2d(startof(r2.curve), c2_offset) + c2_normDir * r2.width / 2f;

    }
}
