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
                    GameObject newObject = Instantiate(selected, selected.transform.position + selected.transform.forward * 30, selected.transform.rotation, selectedNode.transform.parent);
                    if (int.TryParse(selected.name, out int i))
                    {
                        newObject.name = (i + 1).ToString();
                    }
                    else
                    {
                        newObject.name = "1";
                    }
                    Node newNode = newObject.GetComponent<Node>();
                    newNode.outLinks.Clear();
                    newNode.laneCount = selectedNode.laneCount;
                    if (selectedNode.outLinks.Count == 0)
                    {
                        Link defaultLink = new Link(selectedNode, newNode);
                        defaultLink.minLane = 0;
                        defaultLink.maxLane = newNode.laneCount - 1;
                        selectedNode.outLinks.Add(defaultLink);
                    }
                }

            }

        }


    }
}