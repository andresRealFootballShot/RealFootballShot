using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerState
{
    WithPossession, Free, LookingBall, Lock
}
public class PlayerStateMono : MonoVariable<PlayerState>
{
}
