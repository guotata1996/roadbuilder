using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct IDMInfo
{
    public int laneNo;
    public float leadingS, leadingV, followingS, followingV, myV;

    public float deltaV{
        get{
            return myV - leadingV;
        }
    }

    public float followingDeltaV{
        get{
            return followingV - myV;
        }
    }
}

public struct DriverBehavior
{
    public float v0, T, s0, delta, a, b, p, delta_a, a_bias_byPathPreference;
    public DriverBehavior(float desire_v)
    {
        v0 = desire_v;
        T = 1.0f;
        s0 = 2f;
        delta = 4.0f;
        a = 1f;
        b = 1.5f;

        p = 0.2f;
        delta_a = 0.1f;
        a_bias_byPathPreference = 0.2f;
    }

    public static DriverBehavior Default{
        get{
            return new DriverBehavior(10f);
        }
    }
}

public class VehicleController {
    int laneNum;

    public VehicleController(int _laneNum){
        vehicles = new List<Vehicle>[_laneNum];
        for (int i = 0; i != _laneNum; ++i){
            vehicles[i] = new List<Vehicle>();
        }
        laneNum = _laneNum;
    }

    /*always sorted
    TODO: test performance*/
    List<Vehicle>[] vehicles;  

    public void VehicleLeave(Vehicle vh, int laneNo){
        Debug.Assert(vehicles[laneNo].Remove(vh));
    }

    class VehicleOrderComparator : IComparer<Vehicle>
    {
        public int Compare(Vehicle x, Vehicle y)
        {
            return Comparer<float>.Default.Compare(x.distTraveledOnSeg, y.distTraveledOnSeg);
        }
    };

    public int VehicleEnter(Vehicle vh, int laneNo){
        Debug.Assert(vh.VhCtrlOfCurrentSeg == this);
        int insertPoint = vehicles[laneNo].BinarySearch(vh, new VehicleOrderComparator());
        Debug.Assert(insertPoint < 0);
        vehicles[laneNo].Insert(~insertPoint, vh);
        return ~insertPoint;
    }

    public IDMInfo GetIDMInfo(int lane, int index){
        Debug.Assert(0 <= lane && lane < laneNum);
        List<Vehicle> targetLane = vehicles[lane];
        Vehicle me = targetLane[index];

        float leadingV, leadingS;
        if (index == targetLane.Count - 1){
            //I am the oldest/leading vehicle
            int? nextCorrespondingLane = me.correspondingLaneOfNextSeg;

            Vehicle leadingVh = nextCorrespondingLane == null ? null :
                me.VhCtrlOfNextSeg.vehicles[me.correspondingLaneOfNextSeg.Value].Count == 0 ?
                  null : me.VhCtrlOfNextSeg.vehicles[me.correspondingLaneOfNextSeg.Value].First();
                  
            leadingV = leadingVh == null ? 0f : leadingVh.speed;
            leadingS = leadingVh == null ? 
                nextCorrespondingLane == null && !me.onLastSeg ? me.distTowardsEndOfSeg - me.bodyLength : Mathf.Infinity
                : 
                leadingVh.distTraveledOnSeg + me.distTowardsEndOfSeg - leadingVh.bodyLength;
        }
        else{
            leadingV = targetLane[index + 1].speed;
            leadingS = targetLane[index + 1].distTraveledOnSeg - me.distTraveledOnSeg - targetLane[index + 1].bodyLength;
        }
        
        float followingV, followingS;

        if (index == 0){
            //I am the youngest/last vehicle
            int? prevCorrespondingLane = me.correspondingLaneOfPrevSeg;
            Vehicle followingVh = prevCorrespondingLane == null ? null :
                me.VhCtrlOfPrevSeg.vehicles[me.correspondingLaneOfPrevSeg.Value].Count == 0 ?
                  null : me.VhCtrlOfPrevSeg.vehicles[me.correspondingLaneOfPrevSeg.Value].Last();

            followingV = followingVh == null ? 0f : followingVh.speed;
            followingS = followingVh == null ? Mathf.Infinity : me.distTraveledOnSeg + followingVh.distTowardsEndOfSeg - me.bodyLength;
        }
        else{
            followingV = targetLane[index - 1].speed;
            followingS = me.distTraveledOnSeg - targetLane[index - 1].distTraveledOnSeg - me.bodyLength;
        }

        return new IDMInfo
        {
            laneNo = lane,
            leadingS = leadingS,
            leadingV = leadingV,
            myV = targetLane[index].speed,
            followingS = followingS,
            followingV = followingV
        };
            
    }

    float GetIDMAcc(IDMInfo iDMInfo, DriverBehavior driver){
        if (iDMInfo.leadingS <= 0){
            //if would collide
            return -Algebra.Infinity;
        }

        float desiredDist = driver.s0 + 
                                   Mathf.Max(0f, iDMInfo.myV * driver.T + iDMInfo.myV * iDMInfo.deltaV / (2 * Mathf.Sqrt(driver.a * driver.b)));
       
        return driver.a * (1f - Mathf.Pow(iDMInfo.myV / driver.v0, driver.delta) - Mathf.Pow(desiredDist / iDMInfo.leadingS, 2));
    }

    float GetFollowerIDMAcc(IDMInfo myIDMInfo, DriverBehavior followingDriver){
        if (myIDMInfo.followingS <= 0){
            return -Algebra.Infinity;
        }

        float desiredDist = followingDriver.s0 +
                                           Mathf.Max(0f, myIDMInfo.followingV * followingDriver.T + myIDMInfo.followingV * myIDMInfo.followingDeltaV / (2 * Mathf.Sqrt(followingDriver.a * followingDriver.b)));
        return followingDriver.a * (1f - Mathf.Pow(myIDMInfo.followingV / followingDriver.v0, followingDriver.delta) - Mathf.Pow(desiredDist / myIDMInfo.followingS, 2));
    }

    float GetFollowerIDMAccWithoutMe(IDMInfo myIDMInfo, DriverBehavior followingDriver){
        IDMInfo partialInfoForFollower = new IDMInfo
        {
            laneNo = myIDMInfo.laneNo,
            leadingS = myIDMInfo.followingS + myIDMInfo.leadingS,
            leadingV = myIDMInfo.leadingV,
            myV = myIDMInfo.followingV
        };
        return GetIDMAcc(partialInfoForFollower, followingDriver);
    }

    public void updateAccs(){
        for (int l = 0; l != vehicles.Length; ++l){
            var targetLane = vehicles[l];
            for (int i = 0; i != targetLane.Count; ++i){
                float acc = GetIDMAcc(GetIDMInfo(l, i), DriverBehavior.Default);
                targetLane[i].acceleration = acc;
            }
        }
    }

    public void updateLanes()
    {
        HashSet<Vehicle> processed = new HashSet<Vehicle>();
        for (int l = 0; l != vehicles.Length; ++l){
            var targetLane = vehicles[l];
            for (int i = 0; i < targetLane.Count; ++i){
                Vehicle me = targetLane[i];
                if (processed.Contains(me)){
                    continue; // avoid duplicate treatment
                }
                else{
                    processed.Add(me);
                }

                float ns_acc = GetIDMAcc(GetIDMInfo(l, i), DriverBehavior.Default);
                float ns_following_acc = GetFollowerIDMAcc(GetIDMInfo(l, i), DriverBehavior.Default);
                float ns_following_acc_withoutme = GetFollowerIDMAccWithoutMe(GetIDMInfo(l, i), DriverBehavior.Default);

                float rs_acc = -Algebra.Infinity;
                float rs_following_acc = -Algebra.Infinity;
                float rs_following_acc_withoutme = -Algebra.Infinity;

                if (l > 0){
                    int imaginaryOrder = VehicleEnter(me, l - 1);
                    me.ShiftLane(true);
                    rs_acc = GetIDMAcc(GetIDMInfo(l - 1, imaginaryOrder), DriverBehavior.Default);
                    rs_following_acc = GetFollowerIDMAcc(GetIDMInfo(l - 1, imaginaryOrder), DriverBehavior.Default);
                    rs_following_acc_withoutme = GetFollowerIDMAccWithoutMe(GetIDMInfo(l - 1, imaginaryOrder), DriverBehavior.Default);
                    VehicleLeave(me, l - 1);
                    me.ShiftLane(false);
                }

                float rightShiftGain = rs_acc - ns_acc + DriverBehavior.Default.p * (ns_following_acc_withoutme - ns_following_acc + rs_following_acc - rs_following_acc_withoutme);
                rightShiftGain -= me.laneChangingPreference * DriverBehavior.Default.a_bias_byPathPreference;

                float ls_acc = -Algebra.Infinity;
                float ls_following_acc = -Algebra.Infinity;
                float ls_following_acc_withoutme = -Algebra.Infinity;
                if (l < laneNum - 1){
                    int imaginaryOrder = VehicleEnter(me, l + 1);
                    me.ShiftLane(false);
                    ls_acc = GetIDMAcc(GetIDMInfo(l + 1, imaginaryOrder), DriverBehavior.Default);
                    ls_following_acc = GetFollowerIDMAcc(GetIDMInfo(l + 1, imaginaryOrder), DriverBehavior.Default);
                    ls_following_acc_withoutme = GetFollowerIDMAccWithoutMe(GetIDMInfo(l + 1, imaginaryOrder), DriverBehavior.Default);
                    me.ShiftLane(true);
                    VehicleLeave(me, l + 1);
                }

                float leftShiftGain = ls_acc - ns_acc + DriverBehavior.Default.p * (ns_following_acc_withoutme - ns_following_acc + ls_following_acc - ls_following_acc_withoutme);
                leftShiftGain += me.laneChangingPreference * DriverBehavior.Default.a_bias_byPathPreference;
                
                if (Mathf.Max(rightShiftGain, leftShiftGain) > DriverBehavior.Default.delta_a){
                    if (rightShiftGain > leftShiftGain){
                        VehicleLeave(me, l);
                        VehicleEnter(me, l - 1);
                        me.ShiftLane(true);
                    }
                    else{
                        VehicleLeave(me, l);
                        VehicleEnter(me, l + 1);
                        me.ShiftLane(false);
                    }
                }
            }
        }
    }

}
