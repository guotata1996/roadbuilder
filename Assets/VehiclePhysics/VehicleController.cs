using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public Transform wheelObject_FR, wheelObject_FL, wheelObject_BL, wheelObject_BR;
    public WheelCollider collider_FR, collider_FL, collider_BL, collider_BR;

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        float input_toque = Input.GetAxis("Vertical");
        float input_turn = Input.GetAxis("Horizontal");
        collider_FR.motorTorque = collider_FL.motorTorque = input_toque * 100;
        collider_FR.steerAngle = collider_FL.steerAngle = input_turn * 30;
        AssignWheelPoseToObject(collider_FR, wheelObject_FR);
        AssignWheelPoseToObject(collider_FL, wheelObject_FL);
        AssignWheelPoseToObject(collider_BR, wheelObject_BR);
        AssignWheelPoseToObject(collider_BL, wheelObject_BL);
    }

    void AssignWheelPoseToObject(WheelCollider src, Transform obj)
    {
        Vector3 pos = obj.position;
        Quaternion quat = obj.rotation;
        src.GetWorldPose(out pos, out quat);
        obj.position = pos;
        obj.rotation = quat;
    }
}
