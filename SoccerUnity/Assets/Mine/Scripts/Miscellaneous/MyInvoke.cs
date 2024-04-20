using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyInvoke
{
    float time;
    emptyDelegate function;
    public MyInvoke(float time,emptyDelegate function)
    {
        this.time = time;
        this.function = function;
    }
    public MyInvoke()
    {
    }
    public IEnumerator Coroutine(float time,emptyDelegate function)
    {
        float t = 0;
        while (t<time)
        {
            t += Time.timeScale * Time.deltaTime;
            yield return null;
        }
        function?.Invoke();
    }
    public IEnumerator Coroutine()
    {
        float t = 0;
        while (t < time)
        {
            t += Time.timeScale * Time.deltaTime;
            yield return null;
        }
        function?.Invoke();
    }
}
