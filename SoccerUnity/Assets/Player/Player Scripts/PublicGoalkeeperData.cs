using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicGoalkeeperData : PublicPlayerData
{
    public GoalkeeperComponents components;
    public GoalkeeperValues values;
    public override bool IsGoalkeeper { get => true; }
    public override bool maximumJumpHeightIsInArea(float maximumJumpHeight, Vector3 point)
    {
        Area area;
        if(maximumJumpHeights.TryGetValue(maximumJumpHeight,out area))
        {
            if (area != null)
            {
                return area.PointIsInside(point);
            }
            else
            {
                return true;
            }
        }
        return false;
    }
}
