using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChaserDataCalculationParameters
{
    public float timeRange, timeIncrement, minAngle, minVelocity, maxAngle, maxVelocity;

    public ChaserDataCalculationParameters(float timeRange, float timeIncrement, float minAngle, float minVelocity, float maxAngle, float maxVelocity)
    {
        this.timeRange = timeRange;
        this.timeIncrement = timeIncrement;
        this.minAngle = minAngle;
        this.minVelocity = minVelocity;
        this.maxAngle = maxAngle;
        this.maxVelocity = maxVelocity;
    }
}
