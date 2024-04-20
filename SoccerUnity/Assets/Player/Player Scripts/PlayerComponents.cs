using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponents : PlayerComponent
{
    public override PlayerComponents playerComponents { get => this; }
    public Transform root;
    public new PublicPlayerData publicPlayerData;
    public new PlayerData playerData = new PlayerData();
    public PlayerEvents playerEvents;
    public AddPlayerToTeam addPlayerToTeam;
    public string movimentValuesType;
    [HideInInspector]
    public new MovimentValues movementValues;
    public MovementCtrl movementCtrl;
    public SoccerPlayerData soccerPlayerData;
    public Animator animator;
    public MyRaycastHit wallRayCast;
    public new Transform bodyTransform { get => publicPlayerData.bodyTransform; }
    
    public new Rigidbody rigidbody;
    public ResistanceCtrl resistanceCtrl;
    public ResistanceParameters resistanceParameters;
    public BallControl ballControl;
    public BallDriving ballDriving;
    public new PlayerSkills playerSkills;
    public new PlayerParameters playerParameters;
    [HideInInspector]
    public float friction;
    public GetTimeToReachPoint GetTimeToReachPosition;
    public float maxSpeedRotation { get =>movementValues.rotationSpeed; }
    void Start()
    {
        setPlayerComponents();
        setupRadio();
        setupHeight();
        setupMovimentValues();
        MatchEvents.matchLoaded.AddListenerConsiderInvoked(setupFriction);
    }
    public void setPlayerComponents()
    {
        List<PlayerComponent> playerComponents = MyFunctions.GetComponentsInChilds<PlayerComponent>(gameObject, true, false);
        foreach (var playerComponent in playerComponents)
        {
            if (playerComponent.playerComponents == null)
            {
                playerComponent.playerComponents = this;
            }
        }
    }
    public float getMaxSpeed()
    {
        float angle = Vector3.Angle(bodyY0Forward, ForwardDesiredDirection.normalized);
        float maxSpeed = Mathf.Lerp(movementValues.maxForwardSpeed, movementValues.maxHorizontalSpeed, angle / 90);
        maxSpeed = Mathf.Lerp(maxSpeed, movementValues.maxBackSpeed, (angle-90) / 90);
        
        return maxSpeed;
    }
    public float getRunSpeed()
    {
        float angle = Vector3.Angle(bodyY0Forward, ForwardDesiredDirection.normalized);
        float maxSpeed = Mathf.Lerp(movementValues.forwardRunSpeed, movementValues.horizontalRunSpeed, angle / 90);
        maxSpeed = Mathf.Lerp(maxSpeed, movementValues.backRunSpeed, (angle - 90) / 90);
        return maxSpeed;
    }
    public float getSprintSpeed()
    {
        float angle = Vector3.Angle(bodyY0Forward, ForwardDesiredDirection.normalized);
        float maxSpeed = Mathf.Lerp(movementValues.forwardSprintSpeed, movementValues.horizontalSprintSpeed, angle / 90);
        maxSpeed = Mathf.Lerp(maxSpeed, movementValues.backSprintSpeed, (angle - 90) / 90);
        return maxSpeed;
    }
    public new float getMaxAcceleration()
    {
        float angle = Vector3.Angle(bodyY0Forward, ForwardDesiredDirection.normalized);
        float acceleration = Mathf.Lerp(movementValues.forwardAcceleration, movementValues.horizontalAcceleration, angle / 90);
        acceleration = Mathf.Lerp(acceleration, movementValues.backAcceleration, (angle - 90) / 90);
        return acceleration;
    }
    public new float getMaxDeceleration()
    {
        float angle = Vector3.Angle(bodyY0Forward, ForwardDesiredDirection.normalized);
        float deceleration = Mathf.Lerp(movementValues.forwardDeceleration, movementValues.horizontalDeceleration, angle / 90);
        deceleration = Mathf.Lerp(deceleration, movementValues.backDeceleration, (angle - 90) / 90);
        return deceleration;
    }
    protected void setupMovimentValues()
    {
        List<MovimentValues> movimentValuesArray = MyFunctions.GetComponentsInChilds<MovimentValues>(transform.gameObject,true,false);
        foreach (var item in movimentValuesArray)
        {
            
            if (item.info.Equals(movimentValuesType))
            {
                movementValues = item;
            }
        }
    }
    void setupRadio()
    {
        CapsuleCollider capsuleCollider = rigidbody.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            playerData.bodyRadio = capsuleCollider.radius;
        }
    }
    void setupHeight()
    {
        CapsuleCollider capsuleCollider = rigidbody.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            playerData.height = capsuleCollider.height * bodyTransform.localScale.y ;
        }
    }
    void setupFriction()
    {

        PhysicMaterial ballPhysicsMaterial = MatchComponents.ballComponents.physicMaterial;
        PhysicMaterial footballFieldPhysicMaterial = MatchComponents.footballField.footballFieldPhysicMaterial;
        float dynamicFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.dynamicFriction, footballFieldPhysicMaterial.dynamicFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float staticFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.staticFriction, footballFieldPhysicMaterial.staticFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        friction = staticFriction > dynamicFriction ? staticFriction : dynamicFriction;
    }
}
