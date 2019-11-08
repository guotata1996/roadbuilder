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
                            Link newLink = new Link(start, selectedNode);
                            start.outLinks.Add(newLink);
                        }
                        start = null;
                    }
                }
                if (GUILayout.Button("New Node"))
                {
                    GameObject newNode = Instantiate(selected, selected.transform.position + selected.transform.forward * 30, selected.transform.rotation, GameObject.Find("Nodes").transform);
                    newNode.GetComponent<Node>().outLinks.Clear();
                    newNode.GetComponent<Node>().laneCount = selectedNode.laneCount;
                }

            }

        }


    }
}