using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrafficNetwork;
using System.Linq;
using MoreLinq;

namespace TrafficParticipant
{
    public class VehicleLaneController : MonoBehaviour
    {
        VehicleLaneController directFollowing, directFollower;

        VehicleLaneController _rightFollowing, _leftFollowing, _rightFollower, _leftFollower;
        VehicleLaneController rightFollowing
        {
            set
            {
                if (!value && _rightFollowing)
                {
                    _rightFollowing._leftFollower = null;
                }
                else
                {
                    if (value)
                    {
                        value._leftFollower = this;
                    }
                }
                _rightFollowing = value;
            }
            get
            {
                return _rightFollowing;
            }
        }

        VehicleLaneController leftFollowing
        {
            set
            {
                if (!value && _leftFollowing)
                {
                    _leftFollowing._rightFollower = null;
                }
                else
                {
                    if (value)
                    {
                        value._rightFollower = this;
                    }
                }
                _leftFollowing = value;
            }
            get
            {
                return _leftFollowing;
            }
        }

        VehicleLaneController rightFollower
        {
            get
            {
                return _rightFollower;
            }
        }

        VehicleLaneController leftFollower
        {
            get
            {
                return _leftFollower;
            }
        }

        public Link linkOn;
        public int laneOn;
        public float percentageTravelled;
        public float speed;

        private void Start()
        {
            //TODO: bump detection
            if (linkOn == null)
            {
                Debug.LogError("LinkOn must be set before start!");
            }

            VehicleLaneController frontVehicle = FindFrontVehicleInLane(linkOn, laneOn);
            directFollowing = frontVehicle;
            if (frontVehicle != null)
            {
                frontVehicle.directFollower = this;
            }

            if (laneOn + 1 <= linkOn.maxLane)
            {
                rightFollowing = GetFollowingForNewRightLane();
            }

            if (laneOn - 1 >= linkOn.minLane)
            {
                leftFollowing = GetFollowingForNewLeftLane();
            }

            linkOn.vehicles.Add(this);
            percentageTravelled = 0;
        }

        private void Update()
        {
            if (Step(speed * Time.deltaTime)){
                transform.position = linkOn.GetPosition(percentageTravelled, laneOn);
                transform.rotation = Quaternion.LookRotation(linkOn.curve.GetForward(percentageTravelled));
            }
        }

        // distance is real
        public bool Step(float distance)
        {
            if (directFollowing != null) {
                Debug.Assert(distance * distance < (directFollowing.transform.position - transform.position).sqrMagnitude);
            }
            if (percentageTravelled + distance / linkOn.curve.curveLength >= 1.0f)
            {
                bool has_right_neighbor = laneOn != linkOn.maxLane;
                bool has_left_neighbor = laneOn != linkOn.minLane;

                linkOn.vehicles.Remove(this);
                float onOldCurve = linkOn.curve.curveLength * (1.0f - percentageTravelled);
                if (!linkOn.GetNextLink(laneOn, out linkOn, out laneOn))
                {
                    // Path terminated
                    DestroyImmediate(gameObject);
                    return false;
                }
                bool now_has_right_neighbor = laneOn != linkOn.maxLane;
                bool now_has_left_neighbor = laneOn != linkOn.minLane;

                // Enter new link
                linkOn.vehicles.Add(this);
                percentageTravelled = (distance - onOldCurve) / linkOn.curve.curveLength;
                // Travel aross Node: Update neighbors
                if (has_right_neighbor && !now_has_right_neighbor)
                {
                    rightFollowing = null;
                    if (rightFollower)
                    {
                        rightFollower.leftFollowing = null;
                    }
                }
                
                if (!has_right_neighbor && now_has_right_neighbor)
                {
                    rightFollowing = GetFollowingForNewRightLane();
                }

                if (has_left_neighbor && !now_has_left_neighbor)
                {
                    leftFollowing = null;
                    if (leftFollower)
                    {
                        leftFollower.rightFollowing = null;
                    }
                }

                if (!has_left_neighbor && now_has_left_neighbor)
                {
                    leftFollowing = GetFollowingForNewLeftLane();
                }
                
                return true;
            }
            else
            {
                percentageTravelled += distance / linkOn.curve.curveLength;
                float itravelledLinks = linkOn.GetAncestorInfo(laneOn, out _, out _);
                // Handle take over

                if (rightFollowing)
                {
                    Debug.Assert(rightFollowing.laneOn == laneOn + 1);

                    float rightTravelledLinks = rightFollowing.linkOn.GetAncestorInfo(laneOn, out _, out _);

                    if (itravelledLinks + percentageTravelled * linkOn.curve.curveLength >
                        rightTravelledLinks + rightFollowing.percentageTravelled * rightFollowing.linkOn.curve.curveLength)
                    {
                        rightFollowing.leftFollowing = this;
                        if (directFollower && directFollower.rightFollowing == null)
                        {
                            directFollower.rightFollowing = rightFollowing;
                        }
                        rightFollowing = rightFollowing.directFollowing;
                    }

                }
                if (leftFollowing)
                {
                    Debug.Assert(leftFollowing.laneOn == laneOn - 1);

                    float leftTravelledLinks = leftFollowing.linkOn.GetAncestorInfo(laneOn, out _, out _);

                    if (itravelledLinks + percentageTravelled * linkOn.curve.curveLength >
                        leftTravelledLinks + leftFollowing.percentageTravelled * leftFollowing.linkOn.curve.curveLength)
                    {
                        leftFollowing.rightFollowing = this;
                        if (directFollower && directFollower.leftFollowing == null)
                        {
                            directFollower.leftFollowing = leftFollowing;
                        }
                        leftFollowing = leftFollowing.directFollowing;
                    }

                }

                return true;
            }

        }


        // Slow
        // Must be called at t = 0
        private static VehicleLaneController FindFrontVehicleInLane(Link link, int lane)
        {
            Link startLink = link;

            Link search = link;
            int searchLane = lane;

            int steps = 0;
            while (search.vehicles.Count == 0 || search.vehicles.TrueForAll(vh => vh.laneOn != searchLane))
            {
                if (!search.GetNextLink(searchLane, out search, out searchLane))
                {
                    return null;
                }
                // Loop Encountered
                if (steps > 0 && search == startLink)
                {
                    return null;
                }
                steps++;
            }

            var fronts = search.vehicles.Where(vh => vh.laneOn == searchLane);
            if (fronts.ToList().Count == 0) return null;
            return fronts.MinBy(vh => vh.percentageTravelled);
            
        }

        private VehicleLaneController GetFollowingForNewRightLane()
        {
            return _GetFollowingForNewNeighborLane(true);
        }

        private VehicleLaneController GetFollowingForNewLeftLane()
        {
            return _GetFollowingForNewNeighborLane(false);
        }

        // Must be Called at t = 0
        // Locate right FRONT neighbor and update info for all affected vehicles, don't care about right BACK
        // By the time this function is called, directFollowing & directFollower must already be set
        private VehicleLaneController _GetFollowingForNewNeighborLane(bool right)
        {
            
            // Slow
            Link startLink = linkOn;

            Link search = linkOn;
            int searchLane = laneOn;
            int searchNeighborLane = right ? laneOn + 1: laneOn - 1;
            int steps = 0;
            while (search.vehicles.Count == 0 || search.vehicles.TrueForAll(vh => vh.laneOn != searchNeighborLane))
            {
                Link tmp = search;
                if (!tmp.GetNextLink(searchLane, out tmp, out searchLane))
                {
                    return null;
                }
                Link tmp_neighbor = search;
                if (!tmp_neighbor.GetNextLink(searchNeighborLane, out tmp_neighbor, out searchNeighborLane))
                {
                    return null;
                }
                if (tmp != tmp_neighbor)
                {
                    // Right lane diverts
                    return null;
                }
                search = tmp;

                if (directFollowing != null && search == directFollowing.linkOn)
                {
                    //Should not exceed directFollowing
                    return null;
                }
                if (steps > 0 && search == startLink)
                {
                    // loop
                    return null;
                }
                steps++;
            }

            var neighborLaneGroup = search.vehicles.Where(vh => vh.laneOn == searchNeighborLane);
            if (neighborLaneGroup.ToList().Count == 0)
            {
                return null;
            }
            return neighborLaneGroup.MinBy(vh => vh.percentageTravelled);
            
        }

        void OnDrawGizmosSelected(){
            if (directFollowing != null){
                Gizmos.color = Color.black * new Vector4(1,1,1,0.5f);
                Gizmos.DrawSphere(directFollowing.transform.position, 2.5f);
            }
            if (directFollower != null){
                Gizmos.color = Color.white * new Vector4(1,1,1,0.5f);
                Gizmos.DrawSphere(directFollower.transform.position, 2.5f);
            }

            if (rightFollowing != null){
                Gizmos.color = Color.blue * new Vector4(1,1,1,0.5f);
                Gizmos.DrawCube(rightFollowing.transform.position, Vector3.one * 5f);
            }
            if (rightFollower != null){
                Gizmos.color = Color.green * new Vector4(1,1,1,0.5f);
                Gizmos.DrawCube(rightFollower.transform.position, Vector3.one * 5f);
            }

            if (leftFollowing != null)
            {
                Gizmos.color = Color.red * new Vector4(1, 1, 1, 0.5f);
                Gizmos.DrawCube(leftFollowing.transform.position, Vector3.one * 5f);
            }
            if (leftFollower != null)
            {
                Gizmos.color = Color.yellow * new Vector4(1, 1, 1, 0.5f);
                Gizmos.DrawCube(leftFollower.transform.position, Vector3.one * 5f);
            }
        }
    }
}
