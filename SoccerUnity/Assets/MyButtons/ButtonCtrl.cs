using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonCtrl : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool enableButtonAtStart;
    Dictionary<string,List<MyUIAnimation>> animationsDictionary = new Dictionary<string, List<MyUIAnimation>>();
    public string enableAnimationsName;
    public string disableAnimationsName;
    public string enterAnimationsName;
    public string exitAnimationsName;
    public string downAnimationsName;

    bool isEnabled;
    public UnityEvent selectedEvent;
    void Awake()
    {
        setupDictionary();
        getAnimations();
        SetState();
    }
    void setupDictionary()
    {
        animationsDictionary.Add(enableAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(disableAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(enterAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(exitAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(downAnimationsName, new List<MyUIAnimation>());
    }
    void getAnimations()
    {
        MaskableGraphic[] maskableGraphics = transform.GetComponentsInChildren<MaskableGraphic>();
        foreach (MaskableGraphic maskableGraphic in maskableGraphics)
        {
            MyUIAnimation[] animations = maskableGraphic.transform.GetComponentsInChildren<MyUIAnimation>();
            foreach (MyUIAnimation animation in animations)
            {
                animation.MaskableGraphic = maskableGraphic;
                animationsDictionary[animation.name].Add(animation);
            }

        }
    }
    void SetState()
    {
        if (enableButtonAtStart)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }
    public void SelectFunction()
    {
        selectedEvent?.Invoke();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isEnabled)
        {
            foreach (MyUIAnimation animation in animationsDictionary[downAnimationsName])
            {
                animation.Play();
            }
            SelectFunction();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEnabled)
        {
            foreach (MyUIAnimation animation in animationsDictionary[enterAnimationsName])
            {
                animation.Play();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEnabled)
        {
            foreach (MyUIAnimation animation in animationsDictionary[exitAnimationsName])
            {
                animation.Play();
            }
        }
    }
    public void Enable()
    {
        foreach (MyUIAnimation animation in animationsDictionary[enableAnimationsName])
        {
            animation.Play();
        }
        isEnabled = true;
    }
    public void Disable()
    {
        foreach (MyUIAnimation animation in animationsDictionary[disableAnimationsName])
        {
            animation.Play();
        }
        isEnabled = false;
    }
    
}
