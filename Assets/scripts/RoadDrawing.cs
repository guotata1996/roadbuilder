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

    public void fixControlPoint(Vector2 cp)
    {
        cp = roadManager.approNodeToExistingRoad(cp, out targetRoad);
        controlPoint[pointer] = cp;
        pointer++;
    }

    public void setControlPoint(Vector2 cp)
    {
        cp = roadManager.approNodeToExistingRoad(cp, out targetRoad);
        controlPoint[pointer] = cp;
    }

    public void Awake()
    {
        GameObject manager = Instantiate(roadManagerPrefab, transform);
        roadManager = manager.GetComponent<RoadManager>();
        indicatorType = IndicatorType.none;
        reset();
    }


    public void reset()
    {
        controlPoint = new Vector2[4] { Vector2.negativeInfinity, Vector2.negativeInfinity, Vector2.negativeInfinity, Vector2.negativeInfinity };
        pointer = 0;
        Destroy(nodeIndicator);
        Destroy(roadIndicator);
    }

    public void Update()
    {
        laneConfig = GameObject.FindWithTag("UI/laneconfig").GetComponent<LaneConfigPanelBehavior>().laneconfigresult;

        if (controlPoint[pointer].x != Vector3.negativeInfinity.x && indicatorType != IndicatorType.none)
        {
            Destroy(nodeIndicator);
            nodeIndicator = Instantiate(nodeIndicatorPrefab, new Vector3(controlPoint[pointer].x, 0f, controlPoint[pointer].y), Quaternion.identity);

            if (indicatorType == IndicatorType.line)
            {
                if (pointer == 1)
                {
                    Destroy(roadIndicator);
                    roadIndicator = Instantiate(roadIndicatorPrefab, transform);
                    RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                    roadConfigure.generate(new Line(controlPoint[0], controlPoint[1], 0f, 0f), laneConfig);
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
                    roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
                    RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                    roadConfigure.generate(new Line(controlPoint[0], controlPoint[1], 0f, 0f), laneConfig);                
                }

                if (pointer == 2){
                    if (!Geometry.Parallel(controlPoint[1] - controlPoint[0], controlPoint[2] - controlPoint[1]))
                    {
                        Destroy(roadIndicator);
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Bezeir(controlPoint[0], controlPoint[1], controlPoint[2], 0f, 0f), laneConfig);
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
                    /*ind[0] is start, ind[1] isorigin*/
                    Destroy(roadIndicator);
                    roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
                    RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                    roadConfigure.generate(new Line(controlPoint[1], controlPoint[0], 0f, 0f), laneConfig);
                    if (!Algebra.isclose((controlPoint[1] - controlPoint[0]).magnitude, 0))
                        roadConfigure.generate(new Arc(controlPoint[1], controlPoint[0], 1.999f * Mathf.PI, 0f, 0f), laneConfig);
                }

                if (pointer == 2){
                    Vector2 basedir = controlPoint[0] - controlPoint[1];
                    Vector2 towardsdir = controlPoint[2] - controlPoint[1];
                    if (!Mathf.Approximately(0, towardsdir.magnitude))
                    {
                        Destroy(roadIndicator);
                        roadIndicator = Instantiate(roadIndicatorPrefab, transform.position, transform.rotation);
                        RoadRenderer roadConfigure = roadIndicator.GetComponent<RoadRenderer>();
                        roadConfigure.generate(new Arc(controlPoint[1], controlPoint[0], Mathf.Deg2Rad * Vector2.SignedAngle(basedir, towardsdir), 0f, 0f), laneConfig);
                        roadConfigure.generate(new Arc(controlPoint[1], controlPoint[1] + Vector2.right , 1.999f * Mathf.PI, 0f, 0f), laneConfig);
                    }
                }

                if (pointer == 3){
                    Vector2 basedir = controlPoint[0] - controlPoint[1];
                    Vector2 towardsdir = controlPoint[2] - controlPoint[1];
                    if (Mathf.Approximately(0, towardsdir.magnitude)){
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

