using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalkeeperValues : MonoBehaviour,ILoad
{
    public Variable<float> maxSpeedVar=new Variable<float>();
    public float maxSpeed { get => maxSpeedVar.Value; set => maxSpeedVar.Value = value; }
    public float initMaxSpeed=5,minRandomReflexes=0.1f,maxRandomReflexes=0.25f,speedRotation=5;

    public float maxVelocityInterseccionCenterVelocity, minVelocityInterseccionCenterVelocity;
    public float minDistanceInterseccionCenterVelocity = 1, radioDistanceInterseccionCenterVelocity = 3;
    public AnimationCurve speedCurve;
    public float minDistanceBallPlayer = 1, radioDistanceBallPlayer = 3;
    public float timeJumping = 1;
    public float minDistanceToBallWhitOwner = 3;
    public float optimalPositionPassVelocity=20;
    public float optimalPositionSpeedKick=50;
    public float minDistanceFollowBall=2;
    public float adjustBallIsFar1, adjustBallIsFar2;
    public float adjustDistanceBallGoalkeeper1, adjustDistanceBallGoalkeeper2;
    public AnimationCurve adjustBallIsFarCurve;
    public AnimationCurve optimalPositionEscoradoBallInfluence;
    public float cornerAngleAdjust1, cornerAngleAdjust2;
    public float cornerDistanceAdjust1, cornerDistanceAdjust2;
    public float angleCenterPositionAdjust;
    [HideInInspector]
    public float radioInterseccionCenter,minRadioInterseccionCenter;
    [HideInInspector]
    public float maxVelocity, minVelocity;
    [HideInInspector]
    public float radioFollowBall=2;
    [HideInInspector]
    public float minRadioFollowBall=1;
    [HideInInspector]
    public float minVelocityFollowBall, maxVelocityFollowBall;
    public float maxYPosition { get; set; }
    public float maxHeightInArea { get; set; }
    public float initMaxHeightOutsideArea;
    public float maxHeightOutsideArea { get; set; }
    public float distanceLerpCloseTarget { get; set; } = 0.25f;
    public float maxDistanceClosestPointOfLine = 2;
    public float height { get; set; }
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    public MyEvent isLoadedEvent = new MyEvent(nameof(GoalkeeperValues) +" | "+ nameof(isLoadedEvent));
    public float chaserScope,passScope;
    public bool useRivalsAdvandedPosition=true;
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    void Load()
    {
        maxSpeedVar.Value = initMaxSpeed;
        //maxYPosition = goalComponents.maxY;
        maxHeightOutsideArea = initMaxHeightOutsideArea;
        isLoadedEvent.Invoke();
    }
    public void setGoalComponents(GoalComponents goalComponents)
    {
        maxYPosition = goalComponents.maxY;
        maxHeightInArea = getMaxHeight();
    }
    public float getMaxHeight()
    {
        List<string> nameList = new List<string>() { "Armature" , "Corazon1" , "Corazon2" , "Corazon3" };
        List<Transform> transformList = new List<Transform>();
        foreach (var name in nameList)
        {
            Transform trans = MyFunctions.FindChildContainsName(gameObject, name, true).transform;
            if (trans != null)
            {
                transformList.Add(trans);
            }
        }
        float distance=0;
        for (int i = 0; i < Mathf.Clamp(transformList.Count-1,0,transformList.Count); i++)
        {
            distance +=Vector3.Distance(transformList[i].position, transformList[i+1].position);
        }
        height = distance;
        distance += maxYPosition;
        return distance;
    }
}
