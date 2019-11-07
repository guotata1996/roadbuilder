using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrafficParticipant;
using TrafficNetwork;

public class VehicleSpawner : MonoBehaviour
{
    public VehicleLaneController spawnPrefab;

    public Node targetNode;

    public float interval = 5;

    public bool clickToSpawn;

    private void Awake()
    {
        targetNode = transform.parent.GetComponent<Node>();
    }
    // Start is called before the first frame update
    void Start()
    {
        clickToSpawn = false;
        InvokeRepeating("Spawn", 0, interval);
    }

    void Update()
    {
        if (clickToSpawn){
            clickToSpawn = false;
            Spawn();
        }
    }

    void Spawn()
    {
        var controller = Instantiate(spawnPrefab.gameObject, GameObject.Find("Vehicles").transform).GetComponent<VehicleLaneController>();
        
        controller.laneOn = Random.Range(0, targetNode.laneCount);
        controller.linkOn = targetNode.outLinks.Find(lnk => lnk.minLane <= controller.laneOn && controller.laneOn <= lnk.maxLane);
        controller.gameObject.transform.localScale = Vector3.zero;
    }


}
