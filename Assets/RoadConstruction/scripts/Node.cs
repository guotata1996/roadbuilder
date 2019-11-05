using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using TrafficParticipant;

namespace TrafficNetwork
{
    [System.Serializable]
    public class Link
    {
        public Node sourceNode, targetNode;
        // Relative to its start node
        public int minLane, maxLane;
        public int targetMinLane;
        // If node position invalid then curve is null
        public Bezier curve;

        public Vector3 GetPosition(float percentage, int lane)
        {
            float departureLaneRightOffset = lane - ((float)sourceNode.laneCount - 1) / 2;
            float arrivalLaneRightOffset = targetMinLane + (lane - minLane) - ((float)targetNode.laneCount - 1) / 2;
            return curve.GetPoint(percentage) +
                        curve.GetRight(percentage) * Mathf.Lerp(departureLaneRightOffset, arrivalLaneRightOffset, percentage) * Node.laneWidth;
        }

        public float GetAncestorInfo(int lane, out Node ancestorNode, out int ancestorLane)
        {
            if (lane < minLane || lane > maxLane)
            {
                Debug.LogError("lane param wrong!");
            }
            Debug.Assert(sourceNode != null);
            Debug.Assert(sourceNode.ancestorNodeAndLane != null);
            var nodeAndLane = sourceNode.ancestorNodeAndLane[lane];
            ancestorNode = nodeAndLane.Key;
            ancestorLane = nodeAndLane.Value;
            return sourceNode.lengthSinceOrigin[lane];
        }

        public Link(Node src, Node target)
        {
            sourceNode = src;
            targetNode = target;
            // Set initial value to invalid
            minLane = -1;
            maxLane = targetMinLane = 0;

            vehicles = new List<VehicleLaneController>();
        }

        public List<VehicleLaneController> vehicles;

        public bool GetNextLink(int lane, out Link nextLink, out int nextLane)
        {
            if (lane < minLane || lane > maxLane || targetNode == null)
            {
                Debug.LogError("Link Param Error!");
                nextLink = null;
                nextLane = 0;
                return false;
            }
            int targetLane = (lane - minLane) + targetMinLane;
            nextLink = targetNode.outLinks.Find(lnk => lnk.minLane <= targetLane && targetLane <= lnk.maxLane);
            if (nextLink == null)
            {
                nextLane = 0;
                return false;
            }
            else
            {
                nextLane = targetLane;
                return true;
            }
        }

        public bool GetPreviousLink(int lane, out Link prevLink, out int prevLane)
        {
            if (lane < minLane || lane > maxLane || targetNode == null)
            {
                Debug.LogError("Link Param Error!");
                prevLink = null;
                prevLane = 0;
                return false;
            }
            prevLink = sourceNode.inLinks.Find(lnk => lnk.targetMinLane <= lane && lane <= lnk.maxLane - lnk.minLane + lnk.targetMinLane);
            if (prevLink == null)
            {
                prevLane = 0;
                return false;
            }
            else
            {
                prevLane = lane - prevLink.targetMinLane + prevLink.minLane;
                return true;
            }
        }

    }

    [ExecuteInEditMode]
    public class Node : MonoBehaviour
    {
        public const float laneWidth = 3.0f;

        [HideInInspector]
        public int laneCount = 1;
        [HideInInspector]
        public List<Link> outLinks = new List<Link>();

        // Generated before runtime
        [HideInInspector]
        public List<float> lengthSinceOrigin;
        [HideInInspector]
        public List<KeyValuePair<Node, int>> ancestorNodeAndLane;
        [HideInInspector]
        public List<Link> inLinks;

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

        private void Start()
        {
            GenerateCurves();
            LongitudinalInfoInited = false;
            InitLongitudinalInfo();
        }

        // Recalculate out links
        private void Update()
        {
            if (!Application.isPlaying)
            {
                GenerateCurves();
            }
        }

        void GenerateCurves()
        {
            outLinks = outLinks.Where(lnk => lnk.targetNode != null).ToList();
            outLinks.ForEach(link =>
            {
                link.curve = Bezier.Create(outRay, link.targetNode.outRay);
            });
        }

        // Display out conns for this partucular node
        private void Visualize(int subDivNum, Color endColor)
        {
            int order = 0;
            foreach(Link outLink in outLinks)
            {
                if (outLink.curve != null && outLink.minLane > -1)
                {

                    if (subDivNum >= 10)
                    {
                        Handles.Label(outLink.targetNode.transform.position, order.ToString());
                        order++;
                    }
                    for (int l = outLink.minLane; l <= outLink.maxLane; ++l)
                    {
                        Vector3 lastP = Vector3.zero;
                        for (int seg = 0; seg != subDivNum + 1; ++seg)
                        {
                            float percentage = ((float)seg) / subDivNum;
                            Vector3 p = outLink.GetPosition(percentage, l);
                            Gizmos.color = Color.Lerp(Color.white, endColor, percentage);
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

        void OnDrawGizmos()
		{
			Visualize(6, endColor: Color.blue);

            if (Application.isPlaying)
            {
                foreach (var link in outLinks)
                {
                    for (int i = link.minLane; i <= link.maxLane; ++i)
                    {
                        Node n;
                        int l;
                        float dist = link.GetAncestorInfo(i, out n, out l);
                        Handles.Label(link.GetPosition(0, i), dist.ToString() + "\n" + n.name + " " + l);
                    }
                }
            }
		}

        void OnDrawGizmosSelected()
		{
			Visualize(12, endColor: Color.red);
		}

        public static bool LongitudinalInfoInited;

        public static void InitLongitudinalInfo(){
            if (LongitudinalInfoInited)
                return;
            LongitudinalInfoInited = true;

            var allNodes = FindObjectsOfType(typeof(Node));
            foreach (var n in allNodes.ToList())
            {
                Node node = n as Node;
                node.lengthSinceOrigin = new List<float>();
                node.ancestorNodeAndLane = new List<KeyValuePair<Node, int>>();
                for (int i = 0; i != node.laneCount; ++i){
                    node.lengthSinceOrigin.Add(0);
                    node.ancestorNodeAndLane.Add(new KeyValuePair<Node, int>(node, i));
                }
                node.inLinks = new List<Link>();
            }

            foreach(var n in allNodes.ToList()){
                Node node = n as Node;
                node.outLinks.ForEach(link => {
                    for (int i = link.minLane; i <= link.maxLane; ++i){
                        Link nextLink;
                        int nextLane;
                        link.GetNextLink(i, out nextLink, out nextLane);
                        if (nextLink != null){
                            link.targetNode.lengthSinceOrigin[nextLane] = Mathf.Infinity;
                        }
                    }
                    link.targetNode.inLinks.Add(link);
                });
            }
            
            foreach (var n in allNodes.ToList()){
                Node node = n as Node;
                foreach(Link link in node.outLinks){
                    for (int i = link.minLane; i <= link.maxLane; ++i){
                        if (node.lengthSinceOrigin[i] > 0){
                            continue;
                        }
                        Link currLink = link;
                        int currLane = i;
                        
                        float lengthSinceOrigin = 0;
                        while (true){
                            lengthSinceOrigin += currLink.curve.curveLength;
                            Link savedLink = currLink;
                            if (!currLink.GetNextLink(currLane, out currLink, out currLane)){
                                break;
                            }
                            savedLink.targetNode.lengthSinceOrigin[currLane] = lengthSinceOrigin;
                            savedLink.targetNode.ancestorNodeAndLane[currLane] = new KeyValuePair<Node, int>(node, i);
                        }
                    }
                }
            }
        }
	}
}