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
}

public struct DriverBehavior{
    public float v0, T, s0, delta, a, b;
    public DriverBehavior(float desire_v){
        v0 = desire_v;
        T = 1.0f;
        s0 = 2f;
        delta = 4.0f;
        a = 1f;
        b = 1.5f;
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

    /*always sorted*/
    List<Vehicle>[] vehicles;   

    public void VehicleLeave(Vehicle vh, int laneNo){
        Debug.Assert(vehicles[laneNo].Remove(vh));
    }

    public void VehicleEnter(Vehicle vh, int laneNo){
        vehicles[laneNo].Insert(0, vh);
    }

    /*for another lane*/
    /*
    IDMInfo GetIDMInfo(int lane, float position){
        Debug.Assert(0 <= lane && lane < laneNum);
        List<Vehicle> targetLane = vehicles[lane];
        if (targetLane.Count == 0){
            return new IDMInfo(lane);
        }

        Vehicle leading = targetLane.FirstOrDefault(vehicle => vehicle.distTraveledOnSeg >= position);

    }
    */
    /*for same lane*/
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
            leadingS = leadingVh == null ? Mathf.Infinity : leadingVh.distTraveledOnSeg + me.distTowardsEndOfSeg - leadingVh.bodyLength;
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
        float desiredDist = driver.s0 + 
                                   Mathf.Max(0f, iDMInfo.myV * driver.T + iDMInfo.myV * iDMInfo.deltaV / (2 * Mathf.Sqrt(driver.a * driver.b)));
       
        return driver.a * (1f - Mathf.Pow(iDMInfo.myV / driver.v0, driver.delta) - Mathf.Pow(desiredDist / iDMInfo.leadingS, 2));
    }

    public void updateAccs(){
        for (int l = 0; l != vehicles.Length; ++l){
            var targetLane = vehicles[l];
            for (int i = 0; i != targetLane.Count; ++i){
                float acc = GetIDMAcc(GetIDMInfo(l, i), new DriverBehavior(10f));
                targetLane[i].acceleration = acc;
            }
        }
    }
}
