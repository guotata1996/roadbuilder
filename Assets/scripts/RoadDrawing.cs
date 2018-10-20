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

    public Vector2[] controlPoint;

    Road targetRoad;

    public int pointer;

    public IndicatorType indicatorType;

    public GameObject preview;

    List<Curve> interestedApproxLines;

    public void fixControlPoint(Vector2 cp)
    {
        //call after setControlPoint is called
        pointer++;
    }

    public void setControlPoint(Vector2 cp)
    {
        cp = roadManager.approxNodeToExistingRoad(cp, out targetRoad, interestedApproxLines);
        if (pointer <= 3)
            controlPoint[pointer] = cp;
    }

    public void Awake()
    {
        GameObject manager = Instantiate(roadManagerPrefab, Vector3.zero, Quaternion.identity);
        roadManager = manager.GetComponent<RoadManager>();
        indicatorType = IndicatorType.none;
        controlPoint = new Vector2[4];
        interestedApproxLines = new List<Curve>();

        reset();
    }


    public void reset()
    {
        for (int i = 0; i != 4; ++i){
            controlPoint[i] = Vector2.negativeInfinity;
        }
        pointer = 0;
        Destroy(nodeIndicator);
        Destroy(roadIndicator);
        interestedApproxLines.Clear();
    }

    public void Update()
    {
        laneConfig = GameObject.FindWithTag("UI/laneconfig").GetComponent<LaneConfigPanelBehavior>().laneconfigresult;
        interestedApproxLines.Clear();


        if (pointer >= 1)
        {
            interestedApproxLines.Add(new Line(controlPoint[pointer - 1] + Vector2.down * Algebra.InfLength, controlPoint[pointer - 1] + Vector2.up * Algebra.InfLength, 0f, 0f));
            interestedApproxLines.Add(new Line(controlPoint[pointer - 1] + Vector2.left * Algebra.InfLength, controlPoint[pointer - 1] + Vector2.right * Algebra.InfLength, 0f, 0f));
            if (targetRoad != null)
            {
                interestedApproxLines.Add(new Line(controlPoint[pointer - 1], targetRoad.curve.AttouchPoint(controlPoint[pointer - 1]), 0f, 0f));
            }
        }

        if (controlPoint[pointer].x != Vector3.negativeInfinity.x && indicatorType != IndicatorType.none)
        {
            Destroy(nodeIndicator);
            nodeIndicator = Instantiate(nodeIndicatorPrefab, new Vector3(controlPoint[pointer].x, 0f, controlPoint[pointer].y), Quaternion.identity);

            if (indicatorType == IndicatorType.line)
            {

                if (pointer == 1)
                {
                    Destroy(roadIndicator);


                    Road cp0_targetRoad;
                    roadManager.approxNodeToExistingRoad(controlPoint[0], out cp0_targetRoad);
                    if (cp0_targetRoad != null){
                        //perpendicular

                        interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0])) + Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0])) - Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        //extension
                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending(true), controlPoint[0])){
                            interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(0f) + Mathf.PI) * Algebra.InfLength, 0f, 0f));
                        }
                        else{
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending(false),controlPoint[0])){
                                interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(1f)) * Algebra.InfLength, 0f, 0f));
                            }
                        }

                    }

                    if (!Algebra.isclose((controlPoint[0] - controlPoint[1]).magnitude, 0f))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Line(controlPoint[0], controlPoint[1], 0f, 0f), laneConfig, indicator: true);
                    }
                }

                if (pointer == 2)
                {
                    roadManager.addRoad(new Line(controlPoint[0], controlPoint[1], 0f, 0f), laneConfig);
                    reset();
                }

            }
            if (indicatorType == IndicatorType.bezeir)
            {
                if (pointer == 1){
                    Destroy(roadIndicator);
                    interestedApproxLines.Add(new Line(controlPoint[0] + Vector2.down * Algebra.InfLength, controlPoint[0] + Vector2.up * Algebra.InfLength, 0f, 0f));
                    interestedApproxLines.Add(new Line(controlPoint[0] + Vector2.left * Algebra.InfLength, controlPoint[0] + Vector2.right * Algebra.InfLength, 0f, 0f));
                    
                    Road cp0_targetRoad;
                    roadManager.approxNodeToExistingRoad(controlPoint[0], out cp0_targetRoad);
                    if (cp0_targetRoad != null)
                    {
                        interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0])) + Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d((float)cp0_targetRoad.curve.paramOf(controlPoint[0])) - Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending(true), controlPoint[0]))
                        {
                            interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(0f) + Mathf.PI) * Algebra.InfLength, 0f, 0f));
                        }
                        else
                        {
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending(false), controlPoint[0]))
                            {
                                interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(1f)) * Algebra.InfLength, 0f, 0f));
                            }
                        }
                    }

                    if (!Algebra.isclose((controlPoint[0] - controlPoint[1]).magnitude, 0f))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Line(controlPoint[0], controlPoint[1], 0f, 0f), laneConfig, indicator: true);
                    }
                }

                if (pointer == 2){

                    if (!Geometry.Parallel(controlPoint[1] - controlPoint[0], controlPoint[2] - controlPoint[1]))
                    {
                        Destroy(roadIndicator);
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Bezeir(controlPoint[0], controlPoint[1], controlPoint[2], 0f, 0f), laneConfig, indicator:true);
                    }

                }

                if (pointer == 3){
                    
                    if (!Geometry.Parallel(controlPoint[1] - controlPoint[0], controlPoint[2] - controlPoint[1]))
                    {
                        roadManager.addRoad(new Bezeir(controlPoint[0], controlPoint[1], controlPoint[2], 0f, 0f), laneConfig);
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
                        if (Algebra.isclose(cp0_targetRoad.curve.at_ending(true), controlPoint[0]))
                        {
                            interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(0f) + Mathf.PI) * Algebra.InfLength, 0f, 0f));
                            interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(0f) + Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                            interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(0f) - Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                        }
                        else
                        {
                            if (Algebra.isclose(cp0_targetRoad.curve.at_ending(false), controlPoint[0]))
                            {
                                interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(1f)) * Algebra.InfLength, 0f, 0f));
                                interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(1f) + Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                                interestedApproxLines.Add(new Line(controlPoint[0], controlPoint[0] + Algebra.angle2dir(cp0_targetRoad.curve.angle_2d(1f) - Mathf.PI / 2) * Algebra.InfLength, 0f, 0f));
                            }
                        }
                    }

                    /*ind[0] is start, ind[1] isorigin*/
                    Destroy(roadIndicator);
                    if (!Algebra.isclose((controlPoint[0] - controlPoint[1]).magnitude, 0f))
                    {
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Line(controlPoint[1], controlPoint[0], 0f, 0f), laneConfig);
                        if (!Algebra.isclose((controlPoint[1] - controlPoint[0]).magnitude, 0))
                            roadConfigure.generate(new Arc(controlPoint[1], controlPoint[0], 1.999f * Mathf.PI, 0f, 0f), laneConfig, indicator: true);
                    }
                }

                if (pointer == 2){
                    //interestedApproxLines.Clear();
                    Vector2 basedir = controlPoint[0] - controlPoint[1];
                    Vector2 towardsdir = controlPoint[2] - controlPoint[1];
                    if (!Algebra.isclose(0, towardsdir.magnitude) && !Algebra.isclose(controlPoint[1], controlPoint[0]) && !Geometry.Parallel(basedir, towardsdir))
                    {
                        Destroy(roadIndicator);
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Arc(controlPoint[1], controlPoint[0], Mathf.Deg2Rad * Vector2.SignedAngle(basedir, towardsdir), 0f, 0f), laneConfig, indicator:true);
                        roadConfigure.generate(new Arc(controlPoint[1], controlPoint[1] + Vector2.right , 1.999f * Mathf.PI, 0f, 0f), laneConfig, indicator:true);
                    }
                }

                if (pointer == 3){
                    Vector2 basedir = controlPoint[0] - controlPoint[1];
                    Vector2 towardsdir = controlPoint[2] - controlPoint[1];
                    if (Algebra.isclose(0, towardsdir.magnitude)){
                        pointer = 2;
                    }
                    else
                    {
                        roadManager.addRoad(new Arc(controlPoint[1], controlPoint[0], Mathf.Deg2Rad * Vector2.SignedAngle(basedir, towardsdir), 0f, 0f), laneConfig);
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
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
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

}

