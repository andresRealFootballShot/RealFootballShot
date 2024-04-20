using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotWithEffect : Kick
{
    public ComponentsKeys keys;
    List<Vector2> listMouse = new List<Vector2>();
    bool cancelShot;
    public bool isShootingEffect;
    float time, timeMovedMouse;
    public float timeCancel=0.25f;
    public float sensEffectJoy=1, sensEffectMouse=1;
    public MyRaycastHit myRaycast;
    int step;
    bool hitBool;
    void Update()
    {
        Kick();
    }
    void Kick()
    {
        if (Input.GetKeyDown(ComponentsKeys.joyShotEffect) || Input.GetKeyDown(ComponentsKeys.keyShotEffect))
        {
            
            if (step == 0||step==2)
                step++;
           
            if (step == 1)
            {
                if (!myRaycast.isHitting && controllerDistance.isClose())
                {
                    step = 0;
                }
                else
                {
                    InitEffect();
                }
            }
        }
        if (Input.GetKeyUp(ComponentsKeys.joyShotEffect) || Input.GetKeyUp(ComponentsKeys.keyShotEffect)){
            if (step ==1 )
                step++;
            
        }
        if (!cancelShot)
        {
            if (step == 1)
            {
                getCurrentShot();
                if (currentValue > max)
                {
                    CancelShot();
                    InitCurrentForce();
                }
            }
            if (step == 2)
            {
                getMovimentMouse();
                time += Time.deltaTime;
                if (time > 1.5f)
                {
                    CancelShot();
                    InitCurrentForce();
                }
            }
            
            if (step == 3 )
            {
                if (controllerDistance.isClose())
                {
                    if (hitBool)
                    {
                        EffectArgs effectArgs = new EffectArgs(listMouse, myRaycast.hit, componentsPlayer.componentsBall.rigBall, timeMovedMouse, currentValue);
                        effectArgs.calculatePoints();
                        effectArgs.getDistance();
                        effectArgs.calculateLocalDir();
                        effectArgs.calculateGlobalDir(componentsPlayer.transBody);
                        //Touch(-hit.normal* currentValue, hit.point, ForceMode.Impulse);
                        
                        addForceAtPosition(-myRaycast.hit.normal * currentValue, myRaycast.hit.point);

                        if(listMouse.Count>0)
                            StartCoroutine(componentsPlayer.componentsBall.collisionEvent.ApplyEffect(effectArgs));

                    }
                }
                InitCurrentForce();
                CancelShot();
            }
        }
        
    }
    
    void getMovimentMouse()
    {
        Vector2 mouse = new Vector2(Input.GetAxis("Mouse X")*sensEffectMouse + Input.GetAxis("RightStickX")* sensEffectJoy, Input.GetAxis("Mouse Y") * sensEffectMouse + Input.GetAxis("RightStickY")* sensEffectJoy) * Time.deltaTime;
        if (mouse.magnitude > 0.01f)
        {
            if (listMouse.Count == 0)
            {
                listMouse.Add(mouse);
            }
            else
            {
                listMouse.Add(mouse + listMouse[listMouse.Count - 1]);
            }
            timeMovedMouse += Time.deltaTime;
        }
        
    }
    void CancelShot()
    {
        cancelShot = true;
        isShootingEffect = false;
        step = 0;
        time = 0;
    }
    void InitEffect()
    {
        cancelShot = false;
        isShootingEffect = true;
        time = 0;
        timeMovedMouse = 0;
        listMouse.Clear();
    }
}


