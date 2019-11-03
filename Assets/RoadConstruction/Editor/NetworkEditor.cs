using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TrafficNetwork
{
    public class NetworkEditor : EditorWindow
    {
        [MenuItem("Window/Network")]
        public static void ShowWindow()
        {
            GetWindow<NetworkEditor>("Edit network");
        }

        Node start;

        private void OnGUI()
        {
            var selected = Selection.activeGameObject;
            if (selected != null && selected.GetComponent<Node>() != null)
            {
                Node selectedNode = selected.GetComponent<Node>();

                if (GUILayout.Button("Clear"))
                {
                    start = null;

                }

                if (start == null)
                {
                    if (GUILayout.Button("Start Link"))
                    {
                        start = selectedNode;
                    }
                }
                else
                {
                    if (GUILayout.Button("End Link"))
                    {
                        if (selectedNode == start || start.outLinks.Any(lnk => lnk.targetNode == selectedNode))
                        {
                            Debug.LogWarning("Invalid outlink. Please restart.");
                        }
                        else
                        {
                            start.outLinks.Add(new Link(start, selectedNode));
                        }
                        start = null;
                    }
                }
                


                /*
                foreach (Node n in FindObjectsOfType<Node>())
                {
                    if (n.outLinks != null)
                    {
                        foreach (Link l in n.outLinks)
                        {
                            Vector3 middle = 0.5f * (n.transform.position + l.targetNode.transform.position);
                            Debug.DrawLine(n.transform.position, middle, Color.white);
                            Debug.DrawLine(middle, l.targetNode.transform.position, Color.red);
                        }
                    }

                }
                */
            }

            if (GUILayout.Button("Generate Longitudinal Info")){
                Node.LongitudinalInfoInited = false;
            }
        }


    }
}