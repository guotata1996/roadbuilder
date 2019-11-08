using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrafficNetwork;
using System.Linq;

namespace TrafficParticipant
{
    [RequireComponent(typeof(VehicleLaneController))]
    public class ContinuousLaneController : MonoBehaviour
    {
        VehicleLaneController discreteController;
        /*should be in [-1,1]*/
        public float laneOffset; // vs GetLateralOffsetCenter()

        public float freeSpaceAhead;

        public float speed, lateralSpeed;

        public int laneChangingMove; // The current move. -1,0,1

        public float laneChangeCD = 0;

        public float leftscore, rightscore;

        const float maxSpeed = 10f;
        const float maxAcceleration = 4f;
        const float forceLaneChangeDistance = 10f;

        float[] vanishingLane
        {
            get
            {
                return discreteController.linkOn.sourceNode.vanishingLane;
            }    
        }

        float percentageTravelled
        {
            get
            {
                return discreteController.percentageTravelled;
            }
        }

        static float GetAbsoluteLaneOffset(ContinuousLaneController vh)
        {
            return vh.discreteController.linkOn.GetLateralOffsetCenter(vh.discreteController.laneOn) + vh.laneOffset;
        }

        void Start()
        {
            discreteController = GetComponent<VehicleLaneController>();
            StartCoroutine(UpdateLaneChangingMove());
        }

        // Update is called once per frame
        void Update()
        {
            gameObject.transform.localScale = Vector3.one;
            if (discreteController.Step(Time.deltaTime * speed))
            {
                // Update visual
                transform.position = discreteController.linkOn.GetPosition(percentageTravelled, discreteController.laneOn, laneOffset);
                transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(lateralSpeed, speed) * Mathf.Rad2Deg, transform.up) *
                    Quaternion.LookRotation(discreteController.linkOn.curve.GetForward(percentageTravelled));

                laneChangeCD -= Time.deltaTime;

                // Update speed
                UpdateFreeSpaceAhead();

                float marginedSpaceAhead;
                if (mustChangeLane)
                {
                    if (laneChangingMove != 0)
                    {
                        marginedSpaceAhead = Mathf.Max(freeSpaceAhead - 4f, 0f);
                    }
                    else
                    {
                        marginedSpaceAhead = Mathf.Max(freeSpaceAhead - forceLaneChangeDistance, 0f);
                    }
                }
                else
                {
                    marginedSpaceAhead = Mathf.Max(freeSpaceAhead - 4f, 0f);
                }

                speed = Mathf.Min(maxSpeed, Mathf.Sqrt(2 * maxAcceleration * marginedSpaceAhead));

                // Update lateralSpeed
                laneOffset += lateralSpeed * Time.deltaTime;
                if (laneChangingMove == 1 &&
                    GetAbsoluteLaneOffset(this) > Mathf.Min(0.5f, 1f - Mathf.Abs(discreteController.linkOn.GetLateralOffsetCenter(discreteController.laneOn + 1))))
                {
                    laneOffset += discreteController.linkOn.GetLateralOffsetCenter(discreteController.laneOn);
                    discreteController.RightSwitchLane();
                    laneOffset = laneOffset - 1 - discreteController.linkOn.GetLateralOffsetCenter(discreteController.laneOn);
                    laneChangingMove = 0;
                    laneChangeCD = Random.Range(3f, 5f);
                }

                if (laneChangingMove == -1 &&
                    GetAbsoluteLaneOffset(this) < -Mathf.Min(0.5f, 1f - Mathf.Abs(discreteController.linkOn.GetLateralOffsetCenter(discreteController.laneOn - 1))))
                {
                    laneOffset += discreteController.linkOn.GetLateralOffsetCenter(discreteController.laneOn);
                    discreteController.LeftSwitchLane();
                    laneOffset = laneOffset + 1 - discreteController.linkOn.GetLateralOffsetCenter(discreteController.laneOn);
                    laneChangingMove = 0;
                    laneChangeCD = Random.Range(3f, 5f);
                }

                if (laneChangingMove == 0)
                {
                    lateralSpeed = -laneOffset * 0.4f;
                }
                else
                {
                    lateralSpeed = Mathf.Sign(laneChangingMove) * 0.4f;
                }

                lateralSpeed *= Mathf.Min(speed * 0.3f, 1f);
            }


        }

        IEnumerator UpdateLaneChangingMove()
        {
            while (true)
            {
                switch (laneChangingMove)
                {
                    case 0:
                        if (laneChangeCD < 0)
                        {
                            if (mustChangeLane)
                            {
                                if (isRightLaneChangingApplicable)
                                {
                                    laneChangingMove = 1;
                                }
                                else
                                {
                                    if (isLeftLaneChangingApplicable)
                                    {
                                        laneChangingMove = -1;
                                    }
                                }
                            }
                            else
                            {
                                rightscore = 0f;
                                if (isRightLaneChangingApplicable && rightFrontDistance > forceLaneChangeDistance)
                                {
                                    var vehiclesOnRightLane = discreteController.linkOn.vehicles.Where(vh => vh.laneOn == discreteController.laneOn + 1);
                                    if (vehiclesOnRightLane.ToList().Count == 0)
                                    {
                                        if (rightFrontDistance > freeSpaceAhead * 0.99f && freeSpaceAhead < float.MaxValue * 0.5f)
                                        {
                                            rightscore = 1.5f;
                                        }
                                    }
                                    else
                                    {
                                        var rightAverageSpeed = vehiclesOnRightLane.Average(vh => vh.speed);
                                        rightscore = rightAverageSpeed / (speed + 0.01f);
                                    }
                                }

                                leftscore = 0f;
                                if (isLeftLaneChangingApplicable && leftFrontDistance > forceLaneChangeDistance)
                                {
                                    var vehiclesOnLeftLane = discreteController.linkOn.vehicles.Where(vh => vh.laneOn == discreteController.laneOn - 1);
                                    if (vehiclesOnLeftLane.ToList().Count == 0)
                                    {
                                        if (leftFrontDistance > freeSpaceAhead * 0.99f && freeSpaceAhead < float.MaxValue * 0.5f)
                                        {
                                            leftscore = 1.5f;
                                        }
                                    }
                                    else
                                    {
                                        var leftAverageSpeed = vehiclesOnLeftLane.Average(vh => vh.speed);
                                        leftscore = leftAverageSpeed / (speed + 0.01f);
                                    }
                                }

                                if (Mathf.Max(rightscore, leftscore) >= 1.5f)
                                {
                                    laneChangingMove = rightscore > leftscore ? 1 : -1;
                                }
                                
                            }
                        }
                        break;

                    case 1:
                        if (!isRightLaneChangingApplicable)
                        {
                            laneChangingMove = 0;
                        }
                        break;

                    case -1:
                        if (!isLeftLaneChangingApplicable)
                        {
                            laneChangingMove = 0;
                        }
                        break;
                }

                yield return new WaitForSeconds(Random.Range(0.4f, 0.8f));
            }
        }

        // If meets minimum requirement: following far away
        bool isRightLaneChangingApplicable
        {
            get
            {
                if (discreteController.laneOn == discreteController.linkOn.maxLane)
                {
                    return false;
                }

                
                if (discreteController.rightFollower)
                {

                    discreteController.rightFollower.isRightNeighborOf(discreteController, out _, out float rightBack);
                    return 2 * maxAcceleration * rightBack > Mathf.Pow(discreteController.rightFollower.speed, 2);
                }
                else
                {
                    discreteController.linkOn.GetPreviousLink(discreteController.laneOn, out Link myPrevLink, out _);
                    discreteController.linkOn.GetPreviousLink(discreteController.laneOn + 1, out Link rightPrevLink, out _);
                    if (myPrevLink != rightPrevLink)
                    {
                        return discreteController.linkOn.curve.curveLength * discreteController.percentageTravelled > 3f;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        float rightFrontDistance
        {
            get
            {
                int rightLane = discreteController.laneOn + 1;
                float rightFrontVacant = vanishingLane[rightLane] - percentageTravelled * discreteController.linkOn.curve.curveLength;


                if (discreteController.rightFollowing)
                {
                    discreteController.isLeftNeighborOf(discreteController.rightFollowing, out _, out float dist);
                    rightFrontVacant = Mathf.Min(rightFrontVacant, dist);
                }
                else
                {
                    if (discreteController.directFollowing)
                    {
                        rightFrontVacant = Mathf.Min(rightFrontVacant,
                            discreteController.GetDistanceDirectlyBehind(discreteController.directFollowing));
                    }
                    discreteController.linkOn.GetNextLink(discreteController.laneOn, out Link myNextLink, out _);
                    discreteController.linkOn.GetNextLink(discreteController.laneOn + 1, out Link rightNextLink, out _);
                    if (myNextLink != rightNextLink)
                    {
                        var expectedVacant = discreteController.linkOn.curve.curveLength * (1f - discreteController.percentageTravelled);
                        rightFrontVacant = Mathf.Min(rightFrontVacant, expectedVacant);
                    }
                }
                return rightFrontVacant;
            }
        }

        bool isLeftLaneChangingApplicable
        {
            get
            {
                if (discreteController.laneOn == discreteController.linkOn.minLane)
                {
                    return false;
                }

                if (discreteController.leftFollower)
                {
                    discreteController.leftFollower.isLeftNeighborOf(discreteController, out _, out float leftBack);
                    return 2 * maxAcceleration * leftBack > Mathf.Pow(discreteController.leftFollower.speed, 2);
                }
                else
                {
                    discreteController.linkOn.GetPreviousLink(discreteController.laneOn, out Link myPrevLink, out _);
                    discreteController.linkOn.GetPreviousLink(discreteController.laneOn - 1, out Link leftPrevLink, out _);
                    if (myPrevLink != leftPrevLink)
                    {
   
                         return discreteController.linkOn.curve.curveLength * discreteController.percentageTravelled > 3f;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        float leftFrontDistance
        {
            get
            {
                int leftLane = discreteController.laneOn - 1;
                float leftFrontVacant = vanishingLane[leftLane] - percentageTravelled * discreteController.linkOn.curve.curveLength;

                if (discreteController.leftFollowing)
                {
                    discreteController.isRightNeighborOf(discreteController.leftFollowing, out _, out float dist);
                    leftFrontVacant = Mathf.Min(leftFrontVacant, dist);
                }
                else
                {
                    if (discreteController.directFollowing)
                    {
                        leftFrontVacant = Mathf.Min(leftFrontVacant,
                            discreteController.GetDistanceDirectlyBehind(discreteController.directFollowing));
                    }
                    discreteController.linkOn.GetNextLink(discreteController.laneOn, out Link myNextLink, out _);
                    discreteController.linkOn.GetNextLink(discreteController.laneOn - 1, out Link leftNextLink, out _);
                    if (myNextLink != leftNextLink)
                    {
                        var expectedVacant = discreteController.linkOn.curve.curveLength * (1f - discreteController.percentageTravelled);
                        leftFrontVacant = Mathf.Min(leftFrontVacant, expectedVacant);
                    }
                }
                return leftFrontVacant;
            }
        }

        bool mustChangeLane
        {
            get
            {
                return vanishingLane[discreteController.laneOn] - discreteController.linkOn.curve.curveLength * discreteController.percentageTravelled < forceLaneChangeDistance;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
        }

        void UpdateFreeSpaceAhead()
        {
            if (discreteController.directFollowing)
            {
                freeSpaceAhead = discreteController.GetDistanceDirectlyBehind(discreteController.directFollowing);
            }
            else
            {
                freeSpaceAhead = vanishingLane[discreteController.laneOn] - discreteController.linkOn.curve.curveLength * discreteController.percentageTravelled;
            }
            if (discreteController.rightFollowing)
            {
                var rightFollowing = discreteController.rightFollowing;
                if (GetAbsoluteLaneOffset(rightFollowing.GetComponent<ContinuousLaneController>()) < -0.5f)
                {
                    discreteController.isLeftNeighborOf(rightFollowing, out _, out float RBehind);
                    freeSpaceAhead = Mathf.Min(freeSpaceAhead, RBehind);
                }
            }
            if (discreteController.leftFollowing)
            {
                var leftFollowing = discreteController.leftFollowing;
                if (GetAbsoluteLaneOffset(leftFollowing.GetComponent<ContinuousLaneController>()) > 0.5f)
                {
                    discreteController.isRightNeighborOf(leftFollowing, out _, out float LBehind);
                    freeSpaceAhead = Mathf.Min(freeSpaceAhead, LBehind);
                }
            }

            if (laneChangingMove == 1)
            {
                freeSpaceAhead = Mathf.Min(rightFrontDistance, freeSpaceAhead);
            }
            else
            {
                if (laneChangingMove == -1)
                {
                    freeSpaceAhead = Mathf.Min(leftFrontDistance, freeSpaceAhead);
                }
            }
            
        }
    }

}
