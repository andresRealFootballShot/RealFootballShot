using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalComponents : MonoBehaviour
{
    public Transform centerOptimalPosition,centerMatchStoppedState, left, right, down, up, maxYTrans,forward;

    public PlaneWithLimits goalPlane;
    public PlaneWithLimits ballGoesInsidePlane;
    public GameObject goalkeeper;
    public GoalChecker goalChecker;
    public float maxY { get => maxYTrans.position.y; }
    public bool checkEnable()
    {
        return goalPlane != null && ballGoesInsidePlane != null && goalPlane.checkAreaLoaded() && ballGoesInsidePlane.checkAreaLoaded();
    }
}
