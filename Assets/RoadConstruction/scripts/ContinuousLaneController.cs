using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficParticipant
{
    [RequireComponent(typeof(VehicleLaneController))]
    public class ContinuousLaneController : MonoBehaviour
    {
        VehicleLaneController discreteController;
        /*should be in [-1,1]*/
        float laneOffset;

        float freeSpaceAhead;

        void Start()
        {
            discreteController = GetComponent<VehicleLaneController>();
        }

        // Update is called once per frame
        void Update()
        {
            laneOffset = discreteController.linkOn.GetLateralOffsetCenter(discreteController.laneOn);

            // Check for free space ahead
            if (discreteController.directFollowing)
            {
                freeSpaceAhead = discreteController.GetDistanceDirectlyBehind(discreteController.directFollowing);
            }
            else
            {
                freeSpaceAhead = float.MaxValue;
            }
            if (discreteController.rightFollowing)
            {
                var rightFollowing = discreteController.rightFollowing;
                if (rightFollowing.GetComponent<ContinuousLaneController>().laneOffset < -0.5f)
                {
                    discreteController.isLeftNeighborOf(rightFollowing, out _, out float RBehind);
                    freeSpaceAhead = Mathf.Min(freeSpaceAhead, RBehind);
                }
            }
            if (discreteController.leftFollowing)
            {
                var leftFollowing = discreteController.leftFollowing;
                if (leftFollowing.GetComponent<ContinuousLaneController>().laneOffset > 0.5f)
                {
                    discreteController.isRightNeighborOf(leftFollowing, out _, out float LBehind);
                    freeSpaceAhead = Mathf.Min(freeSpaceAhead, LBehind);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, transform.position + transform.forward * freeSpaceAhead + Vector3.up);
        }
    }

}