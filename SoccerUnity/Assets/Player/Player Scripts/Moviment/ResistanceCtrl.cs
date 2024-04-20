using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistanceCtrl : MovementPlayerComponent
{
    
    
    float Value = 1;
    void Start()
    {
        
    }
    private void Update()
    {
        print(resistanceParameters);
        if (ForwardDesiredVelocitySpeed >= movementValues.MaxForwardRunSpeed + 0.1f)
        {
            Value = Mathf.Clamp(Value - Time.deltaTime * resistanceParameters.wearCurve.Evaluate((movementValues.velocityObsolete.Value - movementValues.MaxForwardRunSpeed) / movementValues.MaxForwardSprintSpeed) * resistanceParameters.wearSpeed, 0, 1);

        }
        else
        {
            Value = Mathf.Clamp(Value + (1.25f - Time.deltaTime * resistanceParameters.recoveryCurve.Evaluate(movementValues.velocityObsolete.Value / movementValues.MaxForwardRunSpeed)) * resistanceParameters.recoverSpeed, 0, 1);
        }
    }
    public float getForwardVelocity()
    {
        return resistanceParameters.adjustValueCurve.Evaluate(Value);
    }
}
