using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatTransition: ITransition
{
    public emptyDelegate endTransition { get; set; }
    public bool isInterruptible { get; set; }

    floatDelegate functions;
    public bool Lock;
    public float duration,period;
    public FloatTransition()
    {

    }
    public FloatTransition(float duration, float period,bool isInterruptible)
    {
        this.duration = duration;
        this.period = period;
        this.isInterruptible = isInterruptible;
    }

    public void addFunction(floatDelegate function)
    {
        functions+=function;
    }
    public void removeFunction(floatDelegate function)
    {
        foreach (var listener in functions.GetInvocationList())
        {
            if (listener.Equals(function))
            {
                functions -= function;
            }
        }
    }
    public IEnumerator Coroutine(float duration, float period)
    {
        float time = 0;

        while (time < duration)
        {
            functions?.Invoke(time);
            
            if (period == 0)
            {
                yield return null;
                time += Time.deltaTime;
            }
            else
            {
                yield return new WaitForSeconds(period);
                time += period;
            }
        }
        functions?.Invoke(duration);
        endTransition?.Invoke();
    }

    public IEnumerator Coroutine()
    {
        float time = 0;

        while (time < duration)
        {
            functions?.Invoke(time);

            if (period == 0)
            {
                yield return null;
                time += Time.deltaTime;
            }
            else
            {
                yield return new WaitForSeconds(period);
                time += period;
            }
        }
        functions?.Invoke(duration);
        endTransition?.Invoke();
    }
}
