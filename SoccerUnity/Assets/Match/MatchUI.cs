using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchUI : MonoBehaviour
{
    public MatchCtrl matchCtrl;
    public Canvas canvas;
    public TextMeshProUGUI time;


    void Start()
    {
        canvas.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        float transformTotalSeconds = Mathf.Lerp(0, 5400, matchCtrl.normalizedTime);
        int minutes = Mathf.FloorToInt(transformTotalSeconds/60);
        int seconds = Mathf.FloorToInt(transformTotalSeconds% 60);
        string minutesString = minutes >= 10 ?minutes.ToString(): "0" + minutes;
        string secondsString = seconds >= 10 ?seconds.ToString(): "0" + seconds;
        time.text = minutesString + " : " + secondsString;
    }
}
