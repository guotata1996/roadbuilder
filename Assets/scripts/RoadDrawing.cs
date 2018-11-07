using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum IndicatorType { line, arc, bezeir, none, delete };

public class RoadDrawing : MonoBehaviour
{
    public List<string> laneConfig;

    public GameObject nodeIndicatorPrefab;

    public GameObject roadIndicatorPrefab;

    public GameObject roadManagerPrefab;

    protected GameObject nodeIndicator, roadIndicator;

    public RoadManager roadManager;

    public GameObject degreeTextPrefab;

    List<GameObject> degreeTextInstance, neighborIndicatorInstance;

    public float textDistance;

    public Pair<Vector2, float>[] controlPoint;

    Road targetRoad;

    public int pointer;

    public IndicatorType indicatorType;

    public GameObject preview;

    List<Curve> interestedApproxLines;

    float height;

    GameObject heightTextField;

    public void fixControlPoint(Vector2 cp)
    {
        //call after setControlPoint is called
        pointer++;
    }

    public void setControlPoint(Vector2 cp)
    {
        cp = roadManager.approxNodeToExistingRoad(cp, out targetRoad, interestedApproxLines);

        if (pointer <= 3)
        {
            controlPoint[pointer].First = cp;
            controlPoint[pointer].Second = height;
        }
    }

    public void Awake()
    {
        GameObject manager = Instantiate(roadManagerPrefab, Vector3.zero, Quaternion.identity);
        roadManager = manager.GetComponent<RoadManager>();
        indicatorType = IndicatorType.none;
        controlPoint = new Pair<Vector2, float>[4];
        for (int i = 0; i != 4; ++i){
            controlPoint[i] = new Pair<Vector2, float>();
        }

        interestedApproxLines = new List<Curve>();
        degreeTextInstance = new List<GameObject>();
        neighborIndicatorInstance = new List<GameObject>();
        height = 0f;
        heightTextField = GameObject.Find("Canvas/Height");
        heightTextField.GetComponent<Text>().text = "0";

        reset();
    }


    public void reset()
    {
        for (int i = 0; i != 4; ++i){
            controlPoint[i].First = Vector2.negativeInfinity;
            controlPoint[i].Second = float.NegativeInfinity;
        }
        pointer = 0;
        Destroy(nodeIndicator);
        Destroy(roadIndicator);
        interestedApproxLines.Clear();
        clearAngleDrawing();
    }

    public void Update()
    {
        clearAngleDrawing();
        laneConfig = GameObject.FindWithTag("UI/laneconfig").GetComponent<LaneConfigPanelBehavior>().laneconfigresult;
        interestedApproxLines.Clear();
        if (Input.GetKeyDown(KeyCode.P)){
            height += 1f;
            heightTextField.GetComponent<Text>().text = height.ToString();
        }
        if (Input.GetKeyDown(KeyCode.L)){
            height = Mathf.Max(0f, height - 1f); //TODO: support height < 0
            heightTextField.GetComponent<Text>().text = height.ToString();
        }

        if (pointer >= 1)
        {
            interestedApproxLines.Add(new Line(controlPoint[pointer - 1].First + Vector2.down * Algebra.InfLength, controlPoint[pointer - 1].First + Vector2.up * Algebra.InfLength, 0f, 0f));
            interestedApproxLines.Add(new Line(controlPoint[pointer - 1].First + Vector2.left * Algebra.InfLength, controlPoint[pointer - 1].First + Vector2.right * Algebra.InfLength, 0f, 0f));
            if (targetRoad != null)
            {
                interestedApproxLines.Add(new Line(controlPoint[pointer - 1].First, targetRoad.curve.AttouchPoint(controlPoint[pointer - 1].First), 0f, 0f));
            }
        }

        if (controlPoint[pointer].First.x != Vector3.negativeInfinity.x && indicatorType != IndicatorType.none)
        {
            Destroy(nodeIndicator);
            nodeIndicator = Instantiate(nodeIndicatorPrefab, new Vector3(controlPoint[pointer].First.x, height, controlPoint[pointer].First.y), Quaternion.identity);
            nodeIndicator.transform.SetParent(transform);

            if (indicatorType == IndicatorType.line)
            {

                if (pointer == 1)
                {
                    Destroy(roadIndicator);
                    addAngleDrawing(controlPoint[1].First, controlPoint[0].First);
                    addAngleDrawing(controlPoint[0].First, controlPoint[1].First);

                    Road cp0_targetRoad;
                    roadManager.approxNodeToExistingRoad(controlPoint[0].First, out cp0_targetRoad);
                    if (cp0_targetRoad != null){
                        //perpendicular
                        interestedApproxLines.Add(new Line(controlPoint[0].First, controlPoint[0].First + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0].First)) + Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        interestedApproxLines.Add(new Line(controlPoint[0].First, controlPoint[0].First + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0].First)) - Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        //extension
                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending_2d(true), controlPoint[0].First)){
                            Node crossingRoad;
                            roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(true), out crossingRoad);
                            Debug.Assert(crossingRoad != null);
                            interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse:true));
                        }
                        else{
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending_2d(false),controlPoint[0].First)){
                                Node crossingRoad;
                                roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(false), out crossingRoad);
                                Debug.Assert(crossingRoad != null);
                                interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                            }
                        }

                    }

                    if (!Algebra.isclose((controlPoint[0].First - controlPoint[1].First).magnitude, 0f))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Line(controlPoint[0].First, controlPoint[1].First, controlPoint[0].Second, controlPoint[1].Second), laneConfig, indicator: true);
                    }
                }

                if (pointer == 2)
                {
                    roadManager.addRoad(new Line(controlPoint[0].First, controlPoint[1].First, controlPoint[0].Second, controlPoint[1].Second), laneConfig);
                    reset();
                }

            }
            if (indicatorType == IndicatorType.bezeir)
            {
                if (pointer == 1){
                    Destroy(roadIndicator);
                    addAngleDrawing(controlPoint[1].First, controlPoint[0].First);
                    addAngleDrawing(controlPoint[0].First, controlPoint[1].First);
                    interestedApproxLines.Add(new Line(controlPoint[0].First + Vector2.down * Algebra.InfLength, controlPoint[0].First + Vector2.up * Algebra.InfLength, 0f, 0f));
                    interestedApproxLines.Add(new Line(controlPoint[0].First + Vector2.left * Algebra.InfLength, controlPoint[0].First + Vector2.right * Algebra.InfLength, 0f, 0f));
                    
                    Road cp0_targetRoad;
                    roadManager.approxNodeToExistingRoad(controlPoint[0].First, out cp0_targetRoad);
                    if (cp0_targetRoad != null)
                    {
                        interestedApproxLines.Add(new Line(controlPoint[0].First, controlPoint[0].First + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0].First)) + Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        interestedApproxLines.Add(new Line(controlPoint[0].First, controlPoint[0].First + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0].First)) - Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending_2d(true), controlPoint[0].First))
                        {
                            Node crossingRoad;
                            roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(true), out crossingRoad);
                            Debug.Assert(crossingRoad != null);
                            interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                        }
                        else
                        {
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending_2d(false), controlPoint[0].First))
                            {
                                Node crossingRoad;
                                roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(false), out crossingRoad);
                                Debug.Assert(crossingRoad != null);
                                interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                            }
                        }
                    }

                    if (!Algebra.isclose(controlPoint[0].First, controlPoint[1].First))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Line(controlPoint[0].First, controlPoint[1].First, controlPoint[0].Second, controlPoint[1].Second), laneConfig, indicator: true);
                    }
                }

                if (pointer == 2){

                    if (!Geometry.Parallel(controlPoint[1].First - controlPoint[0].First, controlPoint[2].First - controlPoint[1].First)
                        && !Algebra.isRoadNodeClose(controlPoint[2].First, controlPoint[1].First))
                    {
                        Destroy(roadIndicator);
                        addAngleDrawing(controlPoint[2].First, controlPoint[1].First);

                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Bezeir(controlPoint[0].First, controlPoint[1].First, controlPoint[2].First, 
                                                          controlPoint[0].Second, controlPoint[1].Second), laneConfig, indicator:true);
                    }

                }

                if (pointer == 3){
                    
                    if (!Geometry.Parallel(controlPoint[1].First - controlPoint[0].First, controlPoint[2].First - controlPoint[1].First)
                        && !Algebra.isRoadNodeClose(controlPoint[2].First, controlPoint[1].First))
                    {
                        roadManager.addRoad(new Bezeir(controlPoint[0].First, controlPoint[1].First, controlPoint[2].First,
                                                       controlPoint[0].Second, controlPoint[2].Second), laneConfig);
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
                    roadManager.approxNodeToExistingRoad(controlPoint[0].First, out cp0_targetRoad);
                    if (cp0_targetRoad != null)
                    {
                        addAngleDrawing(controlPoint[0].First, controlPoint[1].First);

                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending_2d(true), controlPoint[0].First))
                        {
                            interestedApproxLines.Add(new Line(controlPoint[0].First, controlPoint[0].First + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(0f) + Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                            interestedApproxLines.Add(new Line(controlPoint[0].First, controlPoint[0].First + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(0f) - Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                            Node crossingRoad;
                            roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(true), out crossingRoad);
                            Debug.Assert(crossingRoad != null);
                            interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                        }
                        else
                        {
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending_2d(false), controlPoint[0].First))
                            {
                                interestedApproxLines.Add(new Line(controlPoint[0].First, controlPoint[0].First + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(1f) + Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                                interestedApproxLines.Add(new Line(controlPoint[0].First, controlPoint[0].First + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(1f) - Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                                Node crossingRoad;
                                roadManager.findNodeAt(cp0_targetRoad.curve.at_ending(false), out crossingRoad);
                                Debug.Assert(crossingRoad != null);
                                interestedApproxLines.AddRange(crossingRoad.directionalLines(Algebra.InfLength, reverse: true));
                            }
                        }
                    }

                    /*ind[0] is start, ind[1] isorigin*/
                    Destroy(roadIndicator);
                    if (!Algebra.isclose((controlPoint[0].First - controlPoint[1].First).magnitude, 0f))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Line(controlPoint[1].First, controlPoint[0].First, controlPoint[1].Second, controlPoint[1].Second), laneConfig);
                        if (!Algebra.isclose((controlPoint[1].First - controlPoint[0].First).magnitude, 0))
                            roadConfigure.generate(new Arc(controlPoint[1].First, controlPoint[0].First, 1.999f * Mathf.PI, 
                                                           controlPoint[0].Second, controlPoint[0].Second), laneConfig, indicator: true);
                    }
                }

                if (pointer == 2){
                    Vector2 basedir = controlPoint[0].First - controlPoint[1].First;
                    Vector2 towardsdir = controlPoint[2].First - controlPoint[1].First;
                    interestedApproxLines.Add(new Arc(controlPoint[1].First, controlPoint[0].First, Mathf.PI * 1.999f, 0f, 0f));
                    if (!Algebra.isclose(0, towardsdir.magnitude) && !Algebra.isclose(controlPoint[1].First, controlPoint[0].First) && !Geometry.Parallel(basedir, towardsdir))
                    {
                        Destroy(roadIndicator);
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Arc(controlPoint[1].First, controlPoint[0].First, Mathf.Deg2Rad * Vector2.SignedAngle(basedir, towardsdir), 
                                                       controlPoint[0].Second, controlPoint[2].Second), laneConfig, indicator:true);
                        roadConfigure.generate(new Arc(controlPoint[1].First, controlPoint[1].First + Vector2.right , 1.999f * Mathf.PI, controlPoint[1].Second, controlPoint[1].Second), laneConfig, indicator:true);
                    }
                }

                if (pointer == 3){
                    Vector2 basedir = controlPoint[0].First - controlPoint[1].First;
                    Vector2 towardsdir = controlPoint[2].First - controlPoint[1].First;
                    if (Algebra.isclose(0, towardsdir.magnitude)){
                        pointer = 2;
                    }
                    else
                    {
                        roadManager.addRoad(new Arc(controlPoint[1].First, controlPoint[0].First, Mathf.Deg2Rad * Vector2.SignedAngle(basedir, towardsdir),
                                                    controlPoint[0].Second, controlPoint[2].Second), laneConfig);
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
                        roadConfigure.generate(targetRoad.curve, new List<string> { "removal_" + targetRoad.width });
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

    void addAngleDrawing(Vector2 positionMaybeOnRoad, Vector2 anotherPosition){
        Node n;
        List<Vector2> neighborDirs;
        if (roadManager.findNodeAt(new Vector3(positionMaybeOnRoad.x, 0f, positionMaybeOnRoad.y), out n))
        {
            neighborDirs = n.getNeighborDirections(anotherPosition - positionMaybeOnRoad);
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
            //Debug.Log("angle: " + Vector2.Angle(anotherPosition - positionMaybeOnRoad, dir));
            Vector2 text2dPosition = positionMaybeOnRoad + ((anotherPosition - positionMaybeOnRoad).normalized + dir.normalized).normalized * textDistance;
            //Debug.Log(text2dPosition);
            GameObject textObj = Instantiate(degreeTextPrefab, new Vector3(text2dPosition.x, 0f, text2dPosition.y), Quaternion.Euler(90f, 0f, 0f));
            textObj.transform.SetParent(transform);
            textObj.GetComponent<TextMesh>().text = Mathf.RoundToInt(Mathf.Abs(Vector2.Angle(anotherPosition - positionMaybeOnRoad, dir))).ToString();
            degreeTextInstance.Add(textObj);

            GameObject indicatorObj = Instantiate(roadIndicatorPrefab, transform);
            RoadRenderer indicatorConfigure = indicatorObj.GetComponent<RoadRenderer>();
            indicatorConfigure.generate(new Line(positionMaybeOnRoad, positionMaybeOnRoad + dir * textDistance * 2, 0f, 0f), new List<string> { "dash_blueindi"});
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

}

