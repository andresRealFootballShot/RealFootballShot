using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyTransition : ITransition
{
    float duration,period;
    emptyDelegate functions;
    public emptyDelegate endTransition { get; set; }
    public bool isInterruptible { get; set; }

    public EmptyTransition(float duration, float period,bool interruptTransition)
    {
        this.duration = duration;
        this.period = period;
        isInterruptible = interruptTransition;
    }
    public void addFunction(emptyDelegate function)
    {
        functions += function;
    }
    public void removeFunction(emptyDelegate function)
    {
        functions -= function;
    }
    public IEnumerator Coroutine()
    {
        float time = 0;

        while (time < duration)
        {
            functions();

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
        functions();
        endTransition?.Invoke();
    }
}
