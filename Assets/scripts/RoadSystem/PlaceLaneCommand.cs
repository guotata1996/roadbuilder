using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceLaneCommand : Command
{
    List<Lane> added;
    List<Lane> deleted;

    public void Execute(object data)
    {
        RoadPositionRecords.AddLane((Curve3DSampler)data, out added, out deleted);
    }

    public void Undo()
    {
        RoadPositionRecords.DeleteLanes(added);
        RoadPositionRecords.RestoreLanes(deleted);
    }
}
