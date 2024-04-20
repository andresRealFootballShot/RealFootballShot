using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    Variable<int> minutesVar = new Variable<int>();
    Variable<float> secondsVar = new Variable<float>();
    int minutes { get => minutesVar.Value; set => minutesVar.Value = value; }
    float seconds { get => secondsVar.Value; set => secondsVar.Value = value; }
    public TimerHUD timerHUD;
    public MyEvent endCountdown;
    public void resetValues()
    {
        minutes = MatchComponents.rulesSettings.partMinutes;
        seconds = MatchComponents.rulesSettings.partSeconds;
    }
    public void Load()
    {
        timerHUD.Load(minutesVar,secondsVar);
        endCountdown = new MyEvent(GetHashCode().ToString());
    }
    public void startProcess()
    {
        enabled = true;
    }
    public void pauseProcess()
    {
        enabled = false;
    }
    private void Update()
    {
        if(!MatchComponents.rulesSettings.lockTimerInStopMatch || RulesCtrl.checkersEnabled)
        {
            seconds -= Time.deltaTime * MatchComponents.rulesComponents.settings.speedTimer;
            if (seconds <= 0)
            {
                if (minutes > 0)
                {
                    minutes--;
                    seconds = 60;
                }
                else
                {
                    pauseProcess();
                    seconds = 0;
                    endCountdown.Invoke();
                }
            }
        }
    }
}
