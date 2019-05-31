using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneGroup : IEnumerable
{
    const float separation = 1.2f;

    List<Lane> contents = new List<Lane>();

    public LaneGroup(Lane mid_lane, int count)
    {
        int mid_index = (count - 1) / 2;
        for(int i = 0; i != count; ++i)
        {
            if (i == mid_index)
            {
                mid_lane.laneGroup = this;
                contents.Add(mid_lane);
            }
            else
            {
                Curve offset_curve = mid_lane.xz_curve.Clone();
                offset_curve.ShiftRight((i - mid_index) * separation);

                Lane offset_lane = new Lane(offset_curve, mid_lane.y_func)
                {
                    laneGroup = this
                };

                contents.Add(offset_lane);
            }
        }

        for (int i = 0; i != count; ++i)
        {
            for (int j = 0; j != count; ++j)
            {
                if (i == j)
                {
                    continue;
                }
                Lane i_lane = contents[i];
                Lane j_lane = contents[j];
                int diff = j - i;
                i_lane.xz_curve.OnShapeChanged += (sender, e) =>
                {
                    Curve offset = i_lane.xz_curve.Clone();

                    offset.ShiftRight(diff * separation);
                    j_lane.xz_curve = offset;
                };

                i_lane.y_func.OnValueChanged += (sender, e) =>
                {
                    j_lane.y_func = i_lane.y_func.Clone();
                };

            }
        }
    }

    public IEnumerator GetEnumerator()
    {
        return contents.GetEnumerator();
    }
}
