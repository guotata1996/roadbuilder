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

    Function currentFunc = null;
    Curve currentCurve = null;
    Lane currentLane;

    private void Start()
    {
        inputHandler.OnClick += delegate (object sender, Vector3 position) {
            if (currentCurve == null)
            {
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

                currentFunc = new LinearFunction(); // TODO: Create more
                currentLane = new Lane(currentCurve, currentFunc);
            }

            new PlaceEndingCommand(position).Execute(new object[] { currentCurve, currentFunc });
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
            currentCurve = null;
        }
    }
}
