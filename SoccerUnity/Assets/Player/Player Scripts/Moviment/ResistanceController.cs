using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistanceController : MonoBehaviour
{
    public float ValueHUD { get { return this.value; } set { CurrentValueEvent?.Invoke((value-min)/(max-min)); this.value = value; } }
    public Variable<float> resistanceVar = new Variable<float>();
    float value;
    public float speed,speedRecover;
    public float min, max;
    public Slash slashResistance;
    public delegate void FloatDelegate(float value);
    public event FloatDelegate CurrentValueEvent;
    public ComponentsKeys keys;
    public ComponentsPlayer componentsPlayer;
    public AnimationCurve valueCurve,curve1,curve2;
    public MovimentValues movimentValues;
    float _value;
    void Awake()
    {
        if (slashResistance != null)
        {
            CurrentValueEvent += slashResistance.ValueChange;
        }
        ValueHUD = 1;
        _value = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (movimentValues.velocityObsolete.Value >= movimentValues.MaxForwardRunSpeed+0.1f)
        {
            _value = Mathf.Clamp(_value - Time.deltaTime * curve1.Evaluate((movimentValues.velocityObsolete.Value - movimentValues.MaxForwardRunSpeed)/movimentValues.MaxForwardSprintSpeed)* speed,0,1);
            
        }
        else
        {
            _value = Mathf.Clamp(_value+Time.deltaTime * (1.25f-curve2.Evaluate(movimentValues.velocityObsolete.Value / movimentValues.MaxForwardRunSpeed))*speedRecover,0,1);
            
        }
        ValueHUD = _value;
        resistanceVar.Value = valueCurve.Evaluate(_value);

    }
}
