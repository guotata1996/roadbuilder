using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CurveMode { Line, Arc, Bezier }

public class BuildRoad : MonoBehaviour
{
    [SerializeField]
    InputHandler inputHandler;

    Type spawnType = typeof(Line);

    Lane currentLane;

    float highlightRadius = 2f;

    private void Start()
    {
        // Init behavior
        inputHandler.OnClick += delegate (object sender, Vector3 position) {
            if (currentLane == null)
            {
                Curve currentCurve = null;
                if (spawnType == typeof(Line))
                {
                    currentCurve = Line.GetDefault();
                }
                if (spawnType == typeof(Arc))
                {
                    currentCurve = Arc.GetDefault();
                }
                if (spawnType == typeof(Bezier))
                {
                    currentCurve = Bezier.GetDefault();
                }

                Function currentFunc = new LinearFunction(); // TODO: Create more
                currentLane = new Lane(currentCurve, currentFunc);
            }

            new PlaceEndingCommand(position).Execute(currentLane);

            if (currentLane.IsValid)
            {
                // Quit Init
                GetComponent<FollowMouseCommand>().enabled = false;
                RoadQueryByPosition.AddLane(currentLane);
                currentLane = null;
                GetComponent<HighLightCtrlPointBehavior>().radius = highlightRadius;
            }
            else
            {
                // Pending
                GetComponent<FollowMouseCommand>().enabled = true;
                GetComponent<FollowMouseCommand>().Execute(currentLane);
                GetComponent<HighLightCtrlPointBehavior>().radius = 0f;
            }

        };

        // Adjust behavior
        inputHandler.OnDragStart += delegate (object sender, Vector3 position)
        {
            GetComponent<FollowMouseCommand>().enabled = true;
            GetComponent<FollowMouseCommand>().Execute(RoadQueryByPosition.QueryClosest(inputHandler.MousePosition));

            GetComponent<HighLightCtrlPointBehavior>().radius = 0f;
        };

        inputHandler.OnDragEnd += delegate (object sender, Vector3 position)
        {
            GetComponent<FollowMouseCommand>().enabled = false;

            GetComponent<HighLightCtrlPointBehavior>().radius = highlightRadius;
        };


    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1)){
            Debug.Log("Line mode");
            spawnType = typeof(Line);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Debug.Log("Arc mode");
            spawnType = typeof(Arc);
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            Debug.Log("Bezier mode");
            spawnType = typeof(Bezier);
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            Debug.Log("Quit");
            currentLane = null;
        }


    }
}
