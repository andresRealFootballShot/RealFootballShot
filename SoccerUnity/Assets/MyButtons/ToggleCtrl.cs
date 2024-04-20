using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ToggleCtrl : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool isEnabled = true;
    Dictionary<string, List<MyUIAnimation>> animationsDictionary = new Dictionary<string, List<MyUIAnimation>>();
    public string enableAnimationsName;
    public string disableAnimationsName;
    public string enterAnimationsName;
    public string exitAnimationsName;
    public string downAnimationsName;
    public string deselectedAnimationsName;
    public bool isSelected;
    public UnityEvent selectUnityEvent;
    public UnityEvent deselectUnityEvent;
    void Awake()
    {
        setupDictionary();
        getAnimations();
        //SetState();
    }
    void setupDictionary()
    {
        animationsDictionary.Add(enableAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(disableAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(enterAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(exitAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(downAnimationsName, new List<MyUIAnimation>());
        animationsDictionary.Add(deselectedAnimationsName, new List<MyUIAnimation>());
    }
    void getAnimations()
    {
        MaskableGraphic[] maskableGraphics = transform.GetComponentsInChildren<MaskableGraphic>();
        foreach (MaskableGraphic maskableGraphic in maskableGraphics)
        {
            MyUIAnimation[] animations = maskableGraphic.transform.GetComponentsInChildren<MyUIAnimation>();
            foreach (MyUIAnimation animation in animations)
            {
                animation.setMaskableGraphic(maskableGraphic);
                animationsDictionary[animation.name].Add(animation);
            }

        }
    }
    public void SetState(bool value)
    {
        switch (value)
        {
            case true:
                Enable();
                break;
            case false:
                Disable();
                break;
        }
    }
    public void Deselect()
    {
        if (isEnabled)
        {
            foreach (MyUIAnimation animation in animationsDictionary[deselectedAnimationsName])
            {
                //animation.Play();
                //las animaciones de deselect solo deben ejecutarse solo cuando estan selecionados para evitar desajustes en los tamaños.
                
                if (isSelected)
                {
                    animation.Play();
                }else if (animation.GetType() ==typeof (UIColorAnimation))
                {
                    animation.Play();
                }
            }
        }
        isSelected = false;
        deselectUnityEvent?.Invoke();
    }
    public void SetState()
    {
        if (isEnabled)
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
        if (isEnabled && !isSelected)
        {
            DebugsList.testing.print("Select "+name);
            foreach (MyUIAnimation animation in animationsDictionary[downAnimationsName])
            {
                animation.Play();
            }
            isSelected = true;
            selectUnityEvent?.Invoke();
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        SelectFunction();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEnabled && !isSelected)
        {
            foreach (MyUIAnimation animation in animationsDictionary[enterAnimationsName])
            {
                animation.Play();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEnabled && !isSelected)
        {
            foreach (MyUIAnimation animation in animationsDictionary[exitAnimationsName])
            {
                animation.Play();
            }
        }
    }
    public void Enable()
    {
        if (isSelected)
        {
            foreach (MyUIAnimation animation in animationsDictionary[downAnimationsName])
            {
                if (animation.GetType() == typeof(UIColorAnimation))
                {
                    animation.Play();
                }
            }
        }
        else
        {
            foreach (MyUIAnimation animation in animationsDictionary[enableAnimationsName])
            {
                animation.Play();
            }
        }
        isEnabled = true;
    }
    public void Disable()
    {
        if (true)
        {
            foreach (MyUIAnimation animation in animationsDictionary[disableAnimationsName])
            {
                animation.Play();
            }
            isEnabled = false;
        }
    }

}

