using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class BallComponents : MonoBehaviour, ILoad
{
    public Rigidbody rigBall;
    public Transform transCenterBall, transBall;
    public Vector3 position { get => transBall.position; set => transBall.position = value; }
    public Collider colliderBall;
    public SphereCollider sphereCollider;
    public CollisionBallEvent collisionEvent;
    public BallSoundCtrl controllerKickSound;
    public PhotonView photonViewBall;
    public OnlineBallCtrl kickRPCs;
    public RaycastBallVelocity raycastBallGoal;
    public PhysicMaterial physicMaterial { get => colliderBall.material; set => colliderBall.material = value; }
    public float radio;
    public float friction,dynamicFriction;
    public float bounciness;
    public float drag { get => rigBall.drag; }
    public float mass { get => rigBall.mass; }
    public static int _loadLevel = 0;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    protected void Load()
    {
        //radio = transBall.localScale.x * sphereCollider.radius;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void Copy(BallComponents components)
    {
        rigBall = components.rigBall;
        transCenterBall = components.transCenterBall;
        transBall = components.transBall;
        colliderBall = components.colliderBall;
        collisionEvent = components.collisionEvent;
        photonViewBall = components.photonViewBall;
        kickRPCs = components.kickRPCs;
    }
}
