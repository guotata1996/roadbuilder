using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace TrafficNetwork
{
    [CustomEditor(typeof(Node))]
    public class NodeEditor : Editor
    {
        private ReorderableList laneConfigureList;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Node node = (Node)target;
            EditorGUIUtility.labelWidth = 40f;

            EditorGUI.BeginChangeCheck();

            laneConfigureList = new ReorderableList(serializedObject, serializedObject.FindProperty("outLinks"), false, false, false, false);
            laneConfigureList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index < 0)
                {
                    return;
                }
                var element = laneConfigureList.serializedProperty.GetArrayElementAtIndex(index);
                
                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.1f, rect.y, rect.width * 0.15f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("minLane"));
                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.25f, rect.y, rect.width * 0.15f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("maxLane"));
                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.4f, rect.y, rect.width * 0.15f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("targetMinLane"));
                EditorGUI.IntSlider(new Rect(rect.x + rect.width * 0.55f, rect.y, rect.width * 0.45f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("lateralOffsetMode"), -1, 1);

                // Adjust to valid value

                int userMin = element.FindPropertyRelative("minLane").intValue;

                if (userMin >= 0)
                {
                    int userMax = element.FindPropertyRelative("maxLane").intValue;
                    int userTargetMin = element.FindPropertyRelative("targetMinLane").intValue;
                    int targetNodeLaneCount = ((Node)element.FindPropertyRelative("targetNode").objectReferenceValue).laneCount;

                    userMin = Mathf.Clamp(userMin, 0, node.laneCount - 1);
                    userMax = Mathf.Clamp(userMax, userMin, Mathf.Min(node.laneCount - 1, userMin + targetNodeLaneCount - 1));

                    int outputLaneCount = userMax - userMin + 1;
                    userTargetMin = Mathf.Clamp(userTargetMin, 0, targetNodeLaneCount - outputLaneCount);

                    element.FindPropertyRelative("minLane").intValue = userMin;
                    element.FindPropertyRelative("maxLane").intValue = userMax;
                    element.FindPropertyRelative("targetMinLane").intValue = userTargetMin;
                }
                
            };
            laneConfigureList.DoLayoutList();

            GUILayout.BeginVertical();
            float currentY = laneConfigureList.GetHeight();
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(new Rect(0, currentY, 300, EditorGUIUtility.singleLineHeight), serializedObject.FindProperty("laneCount"), new GUIContent("#Lanes: "));
            if (serializedObject.FindProperty("laneCount").intValue < 1)
            {
                serializedObject.FindProperty("laneCount").intValue = 1;
            }

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                
            }
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();


        }
    }

}