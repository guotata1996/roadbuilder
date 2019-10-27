using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace TrafficNetwork
{
    [System.Serializable]
    public class Link
    {
        public Node targetNode;
        public int minLane, maxLane;
        public int targetMinLane;
        // If node position invalid then curve is null
        public Bezier curve;

        public Link(Node n)
        {
            targetNode = n;
            // Set initial value to invalid
            minLane = -1;
            maxLane = targetMinLane = 0;
        }
    }

    [ExecuteInEditMode]
    public class Node : MonoBehaviour
    {
        const float laneWidth = 0.5f;

        [HideInInspector]
        public int laneCount = 1;
        [HideInInspector]
        public List<Link> outLinks = new List<Link>();
        public Vector3 direction
        {
            get
            {
                return Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            }
        }

        public Ray outRay
        {
            get
            {
                return new Ray(transform.position, direction);
            }
        }

        // Recalculate out links
        private void Update()
        {
            outLinks = outLinks.Where(lnk => lnk.targetNode != null).ToList();

            outLinks.ForEach(link=> {
                link.curve = Bezier.Create(outRay, link.targetNode.outRay);
            });
        }

        // Display out conns for this partucular node
        private void OnDrawGizmosSelected()
        {
            int subDivNum = 10;

            int order = 0;
            foreach(Link outLink in outLinks)
            {
                if (outLink.curve != null)
                {
                    Handles.Label(outLink.targetNode.transform.position, order.ToString());
                    order++;
                    for (int l = outLink.minLane; l <= outLink.maxLane; ++l)
                    {
                        float departureLaneRightOffset = l - ((float)laneCount - 1) / 2;
                        float arrivalLaneRightOffset = outLink.targetMinLane + (l - outLink.minLane) - ((float)outLink.targetNode.laneCount - 1) / 2;

                        Vector3 lastP = Vector3.zero;
                        for (int seg = 0; seg != subDivNum + 1; ++seg)
                        {
                            float percentage = ((float)seg) / subDivNum;
                            Vector3 p = outLink.curve.GetPoint(percentage) +
                                outLink.curve.GetRight(percentage) * Mathf.Lerp(departureLaneRightOffset, arrivalLaneRightOffset, percentage) * laneWidth;
                            Gizmos.color = Color.Lerp(Color.white, Color.red, percentage);
                            if (seg != 0)
                            {
                                Gizmos.DrawLine(lastP, p);
                            }
                            lastP = p;
                        }
                    }
                }
            }
        }

    }

    
}