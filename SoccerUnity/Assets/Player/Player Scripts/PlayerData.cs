using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public float bodyRadio;
    public float height;
    //public float scope { get => 0.5f + bodyRadio + MatchComponents.ballRadio; }
    Vector3 velocity;
    float speed;
    public Vector3 Velocity { get=>velocity; set { velocity = value; NormalizedVelocity = value.normalized; speed = value.magnitude; } }
    public Vector3 NormalizedVelocity { get; set; }
    public float Speed { get => speed; }
    public float AngularSpeed { get; set; }
    public float VerticalSpeed { get; set; }
    public float HorizontalSpeed { get; set; }
    public float RotationSpeed { get; set; }
}
