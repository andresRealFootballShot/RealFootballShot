using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSetup : MonoBehaviour,ILoad
{
    public static int staticLoadLevel = MatchEvents.staticLoadLevel+1;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }
    public void Load(int level)
    {
        if (level == loadLevel)
        {
            Load();
        }
    }
    private void Load()
    {
        
    }
    void Start()
    {
        GameObject ballGObj = gameObject;
        Rigidbody rigidbody = ballGObj.GetComponent<Rigidbody>();
        rigidbody.interpolation = RigidbodyInterpolation.None;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        BallComponents componentsBall = MyFunctions.GetComponentInChilds<BallComponents>(ballGObj, true);
        componentsBall.radio = componentsBall.transBall.localScale.x * componentsBall.sphereCollider.radius;

        MatchComponents.ballComponents = componentsBall;
        MatchEvents.footballFieldLoaded.AddListener(frictionSetup);
        MatchEvents.setMainBall.Invoke(MatchComponents.ballRigidbody.gameObject);

    }
    void frictionSetup()
    {
        
        PhysicMaterial ballPhysicsMaterial = MatchComponents.ballComponents.physicMaterial;
        PhysicMaterial footballFieldPhysicMaterial = MatchComponents.footballField.footballFieldPhysicMaterial;
        float bounciness = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.bounciness, footballFieldPhysicMaterial.bounciness, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.bounceCombine, footballFieldPhysicMaterial.bounceCombine));
        float dynamicFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.dynamicFriction, footballFieldPhysicMaterial.dynamicFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float staticFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.staticFriction, footballFieldPhysicMaterial.staticFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float friction = staticFriction > dynamicFriction ? staticFriction : dynamicFriction;
        MatchComponents.ballComponents.friction = friction;
        MatchComponents.ballComponents.dynamicFriction = dynamicFriction;
        MatchComponents.ballComponents.bounciness = bounciness;
        MatchEvents.ballPhysicsMaterialLoaded.Invoke();
    }
}
