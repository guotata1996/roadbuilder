using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveLaneCommand : Command
{
    private Lane deleted;
    
    public void Execute(object data)
    {
        Lane targetLane = (Lane) data;
        RoadPositionRecords.DeleteLanes(new List<Lane>{targetLane});

        deleted = targetLane;
    }

    public void Undo()
    {
        RoadPositionRecords.RestoreLanes(new List<Lane>{deleted});
    }
}
