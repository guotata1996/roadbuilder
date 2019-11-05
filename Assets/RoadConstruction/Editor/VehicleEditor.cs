using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TrafficParticipant
{
    [CustomEditor(typeof(VehicleLaneController))]
    public class VehicleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            VehicleLaneController ctrl = (VehicleLaneController)target;
            if (GUILayout.RepeatButton("Forward"))
            {
                ctrl.speed = 20f;
            }
            else
            {
                ctrl.speed = 0f;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
            {
                ctrl.LeftSwitchLane();
            }

            if (GUILayout.Button(">"))
            {
                ctrl.RightSwitchLane();
            }
            GUILayout.EndHorizontal();
        }
    }
}
