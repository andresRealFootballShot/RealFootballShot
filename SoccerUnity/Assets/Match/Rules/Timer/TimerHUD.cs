using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerHUD : MonoBehaviour
{
    public TextHUD text;
    public Variable<int> minutesFloatVar;
    public Variable<float> secondsFloatVar;
    Variable<string> timeStrVar = new Variable<string>();
    int minutes { get => minutesFloatVar.Value; set => minutesFloatVar.Value = value; }
    float seconds { get => secondsFloatVar.Value; set => secondsFloatVar.Value = value; }
    string timeStr { get => timeStrVar.Value; set => timeStrVar.Value = value; }
    public void Load(Variable<int> minutesFloatVar, Variable<float> secondsFloatVar)
    {
        this.minutesFloatVar = minutesFloatVar;
        this.secondsFloatVar = secondsFloatVar;
        this.minutesFloatVar.addObserver(MinutesValueChanged);
        this.secondsFloatVar.addObserver(SecondsValueChanged);
        text.SetVariable(timeStrVar);
    }
    public void MinutesValueChanged(int value)
    {
        //minutes = value;
        timeStr = getStringTime();
    }
    public void SecondsValueChanged(float value)
    {
        //seconds = value;
        timeStr = getStringTime();
    }
    string getStringTime()
    {
        return minutes + ":" + seconds.ToString("f0");
    }
}
