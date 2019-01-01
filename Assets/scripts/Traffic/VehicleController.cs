using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

struct IDMInfo
{
    public int laneNo;
    public int leadingNo;
    public float leadingS, leadingV, myV;

    public IDMInfo(int _laneNo)
    {
        laneNo = _laneNo;
        leadingNo = -1;
        leadingS = Mathf.Infinity;
        leadingV = 0f;
        myV = 0f;
    }

    public float deltaV{
        get{
            return myV - leadingV;
        }
    }
}

struct DriverBehavior{
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
    IDMInfo GetIDMInfo(int lane, int index){
        Debug.Assert(0 <= lane && lane < laneNum);
        List<Vehicle> targetLane = vehicles[lane];
        if (index == targetLane.Count - 1){
            return new IDMInfo
            {
                laneNo = lane,
                leadingNo = -1,
                leadingS = Mathf.Infinity, //TODO: consider node blocking
                leadingV = 0f,
                myV = targetLane[index].speed
            };
        }
        else{
            return new IDMInfo
            {
                laneNo = lane,
                leadingNo = index + 1,
                leadingS = targetLane[index + 1].distTraveledOnSeg - targetLane[index].distTraveledOnSeg
                                                - targetLane[index + 1].bodyLength,
                leadingV = targetLane[index + 1].speed,
                myV = targetLane[index].speed
            };
        }
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
