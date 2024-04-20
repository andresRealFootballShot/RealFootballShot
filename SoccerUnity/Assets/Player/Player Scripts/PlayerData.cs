using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public float bodyRadio;
    public float height;
    //public float scope { get => 0.5f + bodyRadio + MatchComponents.ballRadio; }
    
    public Vector3 Velocity { get; set; }
    public Vector3 NormalizedVelocity { get => Velocity.normalized; }
    public float Speed { get => Velocity.magnitude; }
    public float AngularSpeed { get; set; }
    public float VerticalSpeed { get; set; }
    public float HorizontalSpeed { get; set; }
    public float RotationSpeed { get; set; }
}
