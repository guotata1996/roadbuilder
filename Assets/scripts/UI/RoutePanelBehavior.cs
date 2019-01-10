using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoutePanelBehavior : MonoBehaviour
{

    // Use this for initialization
    RoadDrawing drawing;
    Vector3? start;
    Vector3? end;
    public GameObject carModelPrefab;

    enum mode { listenStart, listenEnd, none };
    mode workingMode;

    public GameObject indicatorPrefab;
    GameObject indicator;

    int _hourflow;
    int HourFlow
    {
        get
        {
            return _hourflow;
        }
        set
        {
            _hourflow = value;
            updateFlowDisplay();
            CancelInvoke("generateVehicle");
            InvokeRepeating("generateVehicle", 0f, 3600f / _hourflow);

        }
    }

    void updateFlowDisplay()
    {
        transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = _hourflow.ToString();
    }

    List<GameObject> vehiclesOfRoute;

    void Start()
    {
        workingMode = mode.none;
        setStartFlag();
        start = end = null;
        indicator = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity);
        drawing = GameObject.Find("curveIndicator").GetComponent<RoadDrawing>();

        vehiclesOfRoute = new List<GameObject>();
        HourFlow = 500;
    }

    // Update is called once per frame
    void Update()
    {
        indicator.transform.position = GameObject.FindWithTag("MainCamera").GetComponent<RayHit>().hitpoint3;
        Vector3 position = GameObject.FindWithTag("MainCamera").GetComponent<RayHit>().hitpoint3;
        Road r;
        position = drawing.roadManager.approxNodeToExistingRoad(position, out r);
        indicator.transform.position = position;

        switch (workingMode)
        {
            case mode.listenStart:
                indicator.transform.localScale = Vector3.one;
                if (Input.GetMouseButtonDown(1))
                {
                    if (r != null)
                    {
                        workingMode = mode.none;
                        setFinishFlag();
                        start = position;
                    }
                    else
                    {
                        start = null; ;
                    }
                }
                break;
            case mode.listenEnd:
                indicator.transform.localScale = Vector3.one;
                if (Input.GetMouseButtonDown(1))
                {
                    if (r != null)
                    {
                        workingMode = mode.none;
                        setStartFlag();
                        end = position;
                    }
                    else
                    {
                        end = null;
                    }
                }
                break;
            case mode.none:
                indicator.transform.localScale = Vector3.zero;
                break;
        }

        if (Input.GetKey(KeyCode.T))
        {
            if (vehiclesOfRoute.Count > 0)
            {
                vehiclesOfRoute[0].GetComponent<Vehicle>().speed = 0f;
            }
        }


    }

    void setStartFlag()
    {
        transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Textures/startflag");
    }

    void setFinishFlag()
    {
        transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Textures/finishflag");
    }

    public void enterListenMode()
    {
        GameObject.Find("Canvas").GetComponent<UIController>().ResetDrawing();

        if (start == null)
        {
            workingMode = mode.listenStart;
        }
        else
        {
            if (end == null)
            {
                workingMode = mode.listenEnd;
            }
        }
    }

    public void addVehicleflow()
    {
        HourFlow += 100;
    }

    public void decreaseVehicleflow()
    {
        HourFlow = Mathf.Max(0, HourFlow - 100);
    }

    public void toggleRouteView()
    {
        vehiclesOfRoute[0].GetComponent<Vehicle>().toggleRouteView();
    }

    void generateVehicle(){
        if (end != null)
        {
            GameObject vehicleObj = Instantiate(carModelPrefab);
            Vehicle vehicle = vehicleObj.GetComponent<Vehicle>();
            vehicle.SetStart(start.Value);
            vehicle.SetDest(end.Value, randomizeLane: true, initialSpeed: 10f);
            if (vehicle.pathOn != null)
            {
                float leadingS = vehicle.VhCtrlOfCurrentSeg.GetIDMInfo(vehicle.LaneOn, 0).leadingS;
                if (leadingS < vehicle.bodyLength + new DriverBehavior().s0)
                {
                    //Abort if there's no enough space to place
                    vehicle.Abort();
                    Destroy(vehicleObj);
                }
                else
                {
                    vehicle.stopEvent += delegate
                    {
                        Destroy(vehicleObj);
                        vehiclesOfRoute.Remove(vehicleObj);
                    };
                    vehiclesOfRoute.Add(vehicleObj);
                }
            }
        }
    }



}
