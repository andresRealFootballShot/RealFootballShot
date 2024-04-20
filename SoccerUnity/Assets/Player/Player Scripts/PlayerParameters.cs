using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerParameters", menuName = "ScriptableObjects/PlayerParameters", order = 2)]
public class PlayerParameters :ScriptableObject
{
    public AnimationCurve ballControlYAngleCurve;
}
