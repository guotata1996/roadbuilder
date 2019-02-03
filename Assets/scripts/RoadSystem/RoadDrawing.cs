using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IndicatorType { line, arc, bezeir, none, delete };

public class RoadDrawing : MonoBehaviour
{
    public List<string> laneConfig;

    public GameObject nodeIndicatorPrefab;

    public GameObject roadIndicatorPrefab;
    
    public GameObject roadManagerPrefab;

    protected GameObject nodeIndicator, roadIndicator;

    public RoadManager roadManager;

    public GameObject CurveRendererPrefab;

    public GameObject degreeTextPrefab;

    List<GameObject> degreeTextInstance, neighborIndicatorInstance;

    public float textDistance;

    public Vector3[] controlPoint;

    Road targetRoad;

    public int pointer;

    public IndicatorType indicatorType;
    
    List<Curve> interestedApproxLines;

    Dictionary<Pair<Curve, float>, GameObject> highLighters;

    public void fixControlPoint(Vector3 cp)
    {
        //call after setControlPoint is called
        pointer++;
    }

    public void setControlPoint(Vector3 cp3)
    {
        cp3 = roadManager.approxNodeToExistingRoad(cp3, out targetRoad, interestedApproxLines);

        if (pointer <= 3)
        {
            controlPoint[pointer] = cp3;
        }
    }

    public void Awake()
    {
        GameObject manager = Instantiate(roadManagerPrefab, Vector3.zero, Quaternion.identity);
        roadManager = manager.GetComponent<RoadManager>();
        indicatorType = IndicatorType.none;
        controlPoint = new Vector3[4];
        highLighters = new Dictionary<Pair<Curve, float>, GameObject>();

        interestedApproxLines = new List<Curve>();
        degreeTextInstance = new List<GameObject>();
        neighborIndicatorInstance = new List<GameObject>();

    }


    public void reset()
    {
        for (int i = 0; i != 4; ++i){
            controlPoint[i] = Vector3.negativeInfinity;
        }
        pointer = 0;
        nodeIndicator.transform.localScale = Vector3.zero;
        Destroy(roadIndicator);
    }

    private void Start()
    {
        nodeIndicator = Instantiate(nodeIndicatorPrefab, transform);
        reset();
    }

    public void Update()
    {
        clearAngleDrawing();
        laneConfig = GameObject.FindWithTag("UI/laneconfig").GetComponent<LaneConfigPanelBehavior>().laneconfigresult;
        interestedApproxLines.Clear();

        if (pointer >= 1)
        {
            interestedApproxLines.Add(Line.TryInit(controlPoint[pointer - 1] + Vector3.back * Algebra.InfLength, controlPoint[pointer - 1] + Vector3.forward * Algebra.InfLength));
            interestedApproxLines.Add(Line.TryInit(controlPoint[pointer - 1] + Vector3.left * Algebra.InfLength, controlPoint[pointer - 1] + Vector3.right * Algebra.InfLength));
            if (targetRoad != null && !Algebra.isProjectionClose(controlPoint[pointer - 1], targetRoad.curve.AttouchPoint(controlPoint[pointer - 1])))
            {
                interestedApproxLines.Add(Line.TryInit(controlPoint[pointer - 1], targetRoad.curve.AttouchPoint(controlPoint[pointer - 1])));
            }
        }

        if (indicatorType == IndicatorType.none){
            nodeIndicator.transform.localScale = Vector3.zero;
        }

        if (controlPoint[pointer].x != Vector3.negativeInfinity.x && indicatorType != IndicatorType.none)
        {
            Vector3 adjustedAttach;
            if (targetRoad != null){
                adjustedAttach = targetRoad.at(targetRoad.curve.paramOf(targetRoad.curve.AttouchPoint(controlPoint[pointer])).Value);
            }
            else{
                adjustedAttach = controlPoint[pointer];
            }
            nodeIndicator.transform.position = new Vector3(adjustedAttach.x, adjustedAttach.y / 2 + 0.1f, adjustedAttach.z);
            nodeIndicator.transform.localScale = new Vector3(1.5f, Mathf.Max(1f, adjustedAttach.y/ 2), 1.5f);

            if (indicatorType == IndicatorType.line)
            {

                if (pointer == 1)
                {
                    Destroy(roadIndicator);
                    addAngleDrawing(controlPoint[1], controlPoint[0]);
                    addAngleDrawing(controlPoint[0], controlPoint[1]);

                    Road cp0_targetRoad;
                    roadManager.approxNodeToExistingRoad(controlPoint[0], out cp0_targetRoad);
                    if (cp0_targetRoad != null){
                        //perpendicular
                        interestedApproxLines.Add(Line.TryInit(controlPoint[0], controlPoint[0] + Algebra.angle2dir_3d(cp0_targetRoad.curve.Angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0])) + Mathf.PI / 2) * Algebra.InfLength));
                        interestedApproxLines.Add(Line.TryInit(controlPoint[0], controlPoint[0] + Algebra.angle2dir_3d(cp0_targetRoad.curve.Angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0])) - Mathf.PI / 2) * Algebra.InfLength));
                        //extension
                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending(true), controlPoint[0])){
                            Node crossingRoad;
                            roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(true), out crossingRoad);
                            Debug.Assert(crossingRoad != null);
                            interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse:true));
                        }
                        else{
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending(false),controlPoint[0])){
                                Node crossingRoad;
                                roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(false), out crossingRoad);
                                Debug.Assert(crossingRoad != null);
                                interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                            }
                        }

                    }

                    if (!Algebra.isProjectionClose(controlPoint[0] , controlPoint[1]))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(Line.TryInit(controlPoint[0], controlPoint[1]), laneConfig);
                    }
                }

                if (pointer == 2)
                {
                    roadManager.addRoad(Line.TryInit(controlPoint[0], controlPoint[1]), laneConfig);
                    reset();
                }

            }
            if (indicatorType == IndicatorType.bezeir)
            {
                if (pointer == 1){
                    Destroy(roadIndicator);
                    addAngleDrawing(controlPoint[1], controlPoint[0]);
                    addAngleDrawing(controlPoint[0], controlPoint[1]);
                    interestedApproxLines.Add(Line.TryInit(controlPoint[0] + Vector3.back * Algebra.InfLength, controlPoint[0] + Vector3.forward * Algebra.InfLength));
                    interestedApproxLines.Add(Line.TryInit(controlPoint[0] + Vector3.left * Algebra.InfLength, controlPoint[0] + Vector3.right * Algebra.InfLength));
                    
                    Road cp0_targetRoad;
                    roadManager.approxNodeToExistingRoad(controlPoint[0], out cp0_targetRoad);
                    if (cp0_targetRoad != null)
                    {
                        interestedApproxLines.Add(Line.TryInit(controlPoint[0], controlPoint[0] + Algebra.angle2dir_3d(cp0_targetRoad.curve.Angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0])) + Mathf.PI / 2) * Algebra.InfLength));
                        interestedApproxLines.Add(Line.TryInit(controlPoint[0], controlPoint[0] + Algebra.angle2dir_3d(cp0_targetRoad.curve.Angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0])) - Mathf.PI / 2) * Algebra.InfLength));
                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending(true), controlPoint[0]))
                        {
                            Node crossingRoad;
                            roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(true), out crossingRoad);
                            Debug.Assert(crossingRoad != null);
                            interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                        }
                        else
                        {
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending(false), controlPoint[0]))
                            {
                                Node crossingRoad;
                                roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(false), out crossingRoad);
                                Debug.Assert(crossingRoad != null);
                                interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                            }
                        }
                    }

                    if (!Algebra.isProjectionClose(controlPoint[0], controlPoint[1]))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(Line.TryInit(controlPoint[0], controlPoint[1]), laneConfig);
                    }
                }

                if (pointer == 2){

                    if (!Geometry.Parallel(controlPoint[1] - controlPoint[0], controlPoint[2] - controlPoint[1])
                        && !Algebra.isRoadNodeClose(controlPoint[2], controlPoint[1]))
                    {
                        Destroy(roadIndicator);
                        addAngleDrawing(controlPoint[2], controlPoint[1]);

                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(Bezeir.TryInit(controlPoint[0], controlPoint[1], controlPoint[2]), laneConfig);
                    }

                }

                if (pointer == 3){
                    
                    if (!Geometry.Parallel(controlPoint[1] - controlPoint[0], controlPoint[2] - controlPoint[1])
                        && !Algebra.isRoadNodeClose(controlPoint[2], controlPoint[1]))
                    {
                        roadManager.addRoad(Bezeir.TryInit(controlPoint[0], controlPoint[1], controlPoint[2]), laneConfig);
                        reset();
                    }
                    else{
                        pointer = 2;
                    }
                }
            }

            if (indicatorType == IndicatorType.arc){
                if (pointer == 1){

                    Road cp0_targetRoad;
                    roadManager.approxNodeToExistingRoad(controlPoint[0], out cp0_targetRoad);
                    if (cp0_targetRoad != null)
                    {
                        addAngleDrawing(controlPoint[0], controlPoint[1]);

                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending(true), controlPoint[0]))
                        {
                            interestedApproxLines.Add(Line.TryInit(controlPoint[0], controlPoint[0] + Algebra.angle2dir_3d(cp0_targetRoad.curve.Angle_2d(0f) + Mathf.PI / 2) * Algebra.InfLength));
                            interestedApproxLines.Add(Line.TryInit(controlPoint[0], controlPoint[0] + Algebra.angle2dir_3d(cp0_targetRoad.curve.Angle_2d(0f) - Mathf.PI / 2) * Algebra.InfLength));
                            Node crossingRoad;
                            roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(true), out crossingRoad);
                            Debug.Assert(crossingRoad != null);
                            interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                        }
                        else
                        {
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending(false), controlPoint[0]))
                            {
                                interestedApproxLines.Add(Line.TryInit(controlPoint[0], controlPoint[0] + Algebra.angle2dir_3d(cp0_targetRoad.curve.Angle_2d(1f) + Mathf.PI / 2) * Algebra.InfLength));
                                interestedApproxLines.Add(Line.TryInit(controlPoint[0], controlPoint[0] + Algebra.angle2dir_3d(cp0_targetRoad.curve.Angle_2d(1f) - Mathf.PI / 2) * Algebra.InfLength));
                                Node crossingRoad;
                                roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(false), out crossingRoad);
                                Debug.Assert(crossingRoad != null);
                                interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                            }
                        }
                    }

                    /*ind[0] is start, ind[1] isorigin*/
                    Destroy(roadIndicator);
                    if (!Algebra.isProjectionClose(controlPoint[0] , controlPoint[1]))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(Line.TryInit(controlPoint[1], controlPoint[0]), laneConfig);
                        if (!Algebra.isclose(controlPoint[1] , controlPoint[0]))
                            roadConfigure.generate(Arc.TryInit(controlPoint[1], controlPoint[0], 1.999f * Mathf.PI), laneConfig);
                    }
                }

                if (pointer == 2){
                    Vector3 basedir = controlPoint[0] - controlPoint[1];
                    Vector3 towardsdir = controlPoint[2] - controlPoint[1];
                    interestedApproxLines.Add(Arc.TryInit(controlPoint[1], controlPoint[0], Mathf.PI * 1.999f));
                    if (!Algebra.isProjectionClose(Vector3.zero, towardsdir) && !Algebra.isProjectionClose(controlPoint[1], controlPoint[0]) && !Geometry.Parallel(basedir, towardsdir))
                    {
                        Destroy(roadIndicator);
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(Arc.TryInit(Algebra.toVector2(controlPoint[1]), Algebra.toVector2(controlPoint[0]), 
                                                       Mathf.Deg2Rad * Vector2.SignedAngle(Algebra.toVector2(basedir), Algebra.toVector2(towardsdir)), 
                                                       controlPoint[0].y, controlPoint[2].y), laneConfig);
                        roadConfigure.generate(Arc.TryInit(controlPoint[1], controlPoint[1] + Vector3.right , 1.999f * Mathf.PI), laneConfig);
                    }
                }

                if (pointer == 3){
                    Vector3 basedir = controlPoint[0] - controlPoint[1];
                    Vector3 towardsdir = controlPoint[2] - controlPoint[1];
                    if (Algebra.isclose(0, towardsdir.magnitude)){
                        pointer = 2;
                    }
                    else
                    {
                        roadManager.addRoad(Arc.TryInit(Algebra.toVector2(controlPoint[1]), Algebra.toVector2(controlPoint[0]), Mathf.Deg2Rad * Vector2.SignedAngle(Algebra.toVector2(basedir), Algebra.toVector2(towardsdir)),
                                                    controlPoint[0].y, controlPoint[2].y), laneConfig);
                        reset();
                    }
                }
            }

            if (indicatorType == IndicatorType.delete){

                if (pointer == 0)
                {
                    Destroy(roadIndicator);

                    if (targetRoad != null)
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(targetRoad.marginedOutCurve, new List<string> { "removal_" + targetRoad.width });
                    }
                }
                else{
                    if (targetRoad != null)
                    {
                        roadManager.deleteRoad(targetRoad);
                        reset();
                    }
                    else{
                        pointer = 0;
                    }
                
                }
            }
        }

    }

    void addAngleDrawing(Vector3 positionMaybeOnRoad, Vector3 anotherPosition){
        Node n;
        List<Vector2> neighborDirs;
        if (roadManager.findNodeAt(positionMaybeOnRoad, out n))
        {
            neighborDirs = n.getNeighborDirections(Algebra.toVector2(anotherPosition - positionMaybeOnRoad));
        }
        else
        {
            Road tar;
            roadManager.approxNodeToExistingRoad(positionMaybeOnRoad, out tar);
            if (tar == null)
            {
                return;
            }
            float? param = tar.curve.paramOf(positionMaybeOnRoad);
            if (param == null){
                return;
            }
            neighborDirs = new List<Vector2>() { tar.curve.direction_2d((float)param), -tar.curve.direction_2d((float)param) };
        }
        foreach(Vector2 dir in neighborDirs){
            Debug.Assert(!Algebra.isclose(dir, Vector2.zero));

            Vector3 textPosition = positionMaybeOnRoad + ((anotherPosition - positionMaybeOnRoad).normalized + Algebra.toVector3(dir).normalized).normalized * textDistance;
            GameObject textObj = Instantiate(degreeTextPrefab, textPosition, Quaternion.Euler(90f, 0f, 0f));
            textObj.transform.SetParent(transform);
            textObj.GetComponent<TextMesh>().text = Mathf.RoundToInt(Mathf.Abs(Vector2.Angle(Algebra.toVector2(anotherPosition - positionMaybeOnRoad), dir))).ToString();
            degreeTextInstance.Add(textObj);

            GameObject indicatorObj = Instantiate(roadIndicatorPrefab, transform);
            RoadRenderer indicatorConfigure = indicatorObj.GetComponent<RoadRenderer>();
            indicatorConfigure.generate(Line.TryInit(positionMaybeOnRoad, positionMaybeOnRoad + Algebra.toVector3(dir) * textDistance * 2), new List<string> { "solid_blueindi"});
            neighborIndicatorInstance.Add(indicatorObj);

        }

    }

    void clearAngleDrawing(){
        foreach(GameObject text in degreeTextInstance){
            Destroy(text);
        }

        foreach(GameObject neighborIndicator in neighborIndicatorInstance){
            Destroy(neighborIndicator);
        }

    }

    public void highLightRoad(Pair<Curve, float> c_offset){
        GameObject highlighter = Instantiate(CurveRendererPrefab, transform);
        CurveRenderer renderer = highlighter.GetComponent<CurveRenderer>();
        Material normalMaterial = Resources.Load<Material>("Materials/Rolling");
        renderer.CreateMesh(c_offset.First, 0.3f, normalMaterial, offset:c_offset.Second, z_offset: 0.03f);
        highLighters.Add(c_offset, highlighter);
    }

    public void deHighLightRoad(Pair<Curve, float> c_offset){
        if (highLighters.ContainsKey(c_offset))
        {
            GameObject highLighter = highLighters[c_offset];
            Destroy(highLighter);
            highLighters.Remove(c_offset);
        }
    }

    public void SerializeRoad(){
        roadManager.serializeRoad();
    }

    public void DeSerializeRoad(){
        roadManager.deserializeRoad();
    }
}
