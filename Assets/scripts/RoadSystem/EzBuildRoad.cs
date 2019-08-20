using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EzBuildRoad : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    InputHandler inputHandler;

    public float forwardStep = 10.0f;
    public float turnStep = Mathf.PI / 6;

    public float shiftStep = 3.0f;

    bool appendAtFront = true;
    bool turnRight = true;

    Type spawnType = typeof(Line);

    private Lane _baseLane;
    private Lane BaseLane
    {
        set
        {
            if (_baseLane != null)
            {
                _baseLane.SetHighlighted(false);
            }
            _baseLane = value;

            if (_baseLane != null)
            {
                _baseLane.SetHighlighted(true);
            }

        }
        get
        {
            return _baseLane;
        }
    }

    void OnEnable()
    {
        inputHandler.OnClick += delegate (object sender, (Vector3, Curve3DSampler) pos_parent) {
            if (pos_parent.Item2 == null)
            {
                BaseLane = null;
            }
            else
            {
                BaseLane = (Lane)pos_parent.Item2;
            }
        };

        inputHandler.OnForwardKeyPressed += delegate {
            if (BaseLane == null)
            {
                return;
            }

            Function currentFunc = new LinearFunction(0f, 0f); // TODO: Create more
            Curve currentCurve = null;

            if (spawnType == typeof(Line))
            {
                var curveParams = appendAtFront ?
                new List<Vector3> { BaseLane.xz_curve.GetTwodPos(1f), BaseLane.xz_curve.GetTwodPos(1f) + BaseLane.xz_curve.GetFrontDir(1f) * forwardStep } :
                new List<Vector3> { BaseLane.xz_curve.GetTwodPos(0f) - BaseLane.xz_curve.GetFrontDir(0f) * forwardStep, BaseLane.xz_curve.GetTwodPos(0f)};
                currentCurve = new Line(curveParams[0], curveParams[1]);
            }
            else
            {
                if (spawnType == typeof(Arc))
                {
                    float radius = forwardStep / turnStep;
                    if (BaseLane.xz_curve is Arc)
                    {
                        radius = ((Arc)(BaseLane.xz_curve)).Radius;
                    }

                    if (appendAtFront)
                    {
                        if (turnRight)
                        {
                            currentCurve = new Arc(
                                BaseLane.xz_curve.GetTwodPos(1f) + BaseLane.xz_curve.GetRightDir(1f) * radius,
                                BaseLane.xz_curve.GetTwodPos(1f),
                                - turnStep);
                        }
                        else
                        {
                            currentCurve = new Arc(
                                BaseLane.xz_curve.GetTwodPos(1f) - BaseLane.xz_curve.GetRightDir(1f) * radius,
                                BaseLane.xz_curve.GetTwodPos(1f),
                                turnStep);
                        }
                    }
                    else
                    {
                        if (turnRight)
                        {
                            currentCurve = new Arc(
                                BaseLane.xz_curve.GetTwodPos(0f) - BaseLane.xz_curve.GetRightDir(0f) * radius,
                                BaseLane.xz_curve.GetTwodPos(0f),
                                -turnStep);
                            currentCurve.Crop(1f, 0f);
                        }
                        else
                        {
                            currentCurve = new Arc(
                                BaseLane.xz_curve.GetTwodPos(0f) + BaseLane.xz_curve.GetRightDir(0f) * radius,
                                BaseLane.xz_curve.GetTwodPos(0f),
                                turnStep);
                            currentCurve.Crop(1f, 0f);
                        }
                    }
                }
                else
                {
                    // Bezeir mode not supported
                    return;
                }
            }


            BaseLane = new Lane(currentCurve, currentFunc);
            var placeCmd = new PlaceLaneCommand();
            inputHandler.commandSequence.Push(placeCmd);
            placeCmd.Execute(BaseLane);

            BaseLane.SetGameobjVisible(false);

            // Set new BaseLane
            foreach(Lane added in placeCmd.added)
            {
                if ((added.GetThreedPos(1f) - BaseLane.GetThreedPos(1f)).sqrMagnitude < 0.001f)
                {
                    BaseLane = added;
                    break;
                }
            }

            BuildRoad.UseDefaultStickyMouseForRoad(inputHandler.stickyMouse);

        };

        inputHandler.OnLeftKeyPressed += delegate {
            PlaceShiftedLane(false);
        };

        inputHandler.OnRightKeyPressed += delegate {
            PlaceShiftedLane(true);
        };
    }

    void PlaceShiftedLane(bool right)
    {
        if (BaseLane == null)
        {
            return;
        }

        Curve c1 = BaseLane.xz_curve.Clone();
        c1.ShiftRight(right ? shiftStep : -shiftStep);
        BaseLane = new Lane(c1, BaseLane.y_func);

        var placeCmd = new PlaceLaneCommand();
        inputHandler.commandSequence.Push(placeCmd);
        placeCmd.Execute(BaseLane);
        BaseLane.SetGameobjVisible(false);

        // Set new BaseLane
        foreach (Lane added in placeCmd.added)
        {
            if ((added.GetThreedPos(1f) - BaseLane.GetThreedPos(1f)).sqrMagnitude < 0.001f)
            {
                BaseLane = added;
                break;
            }
        }

        BuildRoad.UseDefaultStickyMouseForRoad(inputHandler.stickyMouse);
    }

    void OnDisable()
    {
        BaseLane = null;
        inputHandler.Reset();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Debug.Log("EZ Line mode");
            spawnType = typeof(Line);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Debug.Log("EZ Arc mode");
            spawnType = typeof(Arc);
        }
    }

    public void SwitchLR()
    {
        turnRight = !turnRight;
        Debug.Log("switched LR to " + appendAtFront);
    }

    public void SwitchFrontBack()
    {
        appendAtFront = !appendAtFront;
        Debug.Log("switched FB to " + appendAtFront);
    }
}
