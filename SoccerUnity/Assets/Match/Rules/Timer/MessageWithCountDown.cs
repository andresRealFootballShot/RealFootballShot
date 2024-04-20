using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MessageWithCountDown : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public TextMeshProUGUI ReadyText;
    public Canvas canvas;
    public delegate void genericDelegate<T>(T value);
    Coroutine countDownCoroutine;
    public event emptyDelegate endCountDown;
    //Canvas canvas { get => Text.canvas;}
    public void Hide()
    {
        canvas.enabled = false;
    }
    public IEnumerator countDown<T>(float duration,float waitToStart,float waitToHide,float waitToEnd,string format,bool isMyTeam,T endArgs, genericDelegate<T> endFunction)
    {
        if (!isMyTeam && TypeMatch.getTeamMaxPlayersWithGoalkeepers() < 2 && false)
        {
            ReadyText.text = "Press \"" + ComponentsKeys.use + "\" if you're ready";
        }
        else
        {
            ReadyText.text = "";
        }
        float t = duration;
        yield return new WaitForSeconds(waitToStart);
        canvas.enabled = true;
        Coroutine _areReadyCoroutine=null;
        if (!isMyTeam && TypeMatch.getTeamMaxPlayersWithGoalkeepers() == 2 && false)
        {
            _areReadyCoroutine = StartCoroutine(areReadyCoroutine(endArgs, endFunction));
        }
        while (t > 0)
        {
            Text.text = String.Format(format, t.ToString("f0"));
            yield return new WaitForSeconds(1);
            t--;
        }
        Text.text = String.Format(format, t.ToString("f0"));
        MatchComponents.rulesComponents.whistleAnimation.Play();
        Invoke(nameof(hideCornerKickMessage), waitToHide);
        if (_areReadyCoroutine != null)
        {
            StopCoroutine(_areReadyCoroutine);
        }
        yield return new WaitForSeconds(waitToEnd);
        endFunction(endArgs);
        //cornerKick.randomPass(args.corner.cornerPoint.forward, "");
    }
    IEnumerator areReadyCoroutine<T>(T endArgs, genericDelegate<T> endFunction)
    {
        while (true)
        {
            if (Input.GetKeyDown(ComponentsKeys.use))
            {
                canvas.enabled = false;
                endFunction(endArgs);
                break;
            }
            yield return null;
        }
    }
    void hideCornerKickMessage()
    {
        canvas.enabled = false;
    }
}
