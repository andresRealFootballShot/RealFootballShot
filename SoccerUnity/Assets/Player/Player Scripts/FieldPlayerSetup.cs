using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldPlayerSetup : PlayerComponent
{
    PublicFieldPlayerData myPublicFieldData;
    void Start()
    {
        //setup();
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(setup);
    }
    public void setup()
    {
        MovimentValues movimentValues = MyFunctions.GetComponentInChilds<MovimentValues>(gameObject,true);
        ResistanceController resistanceController = MyFunctions.GetComponentInChilds<ResistanceController>(gameObject, true);
        myPublicFieldData = MyFunctions.GetComponentInChilds<PublicFieldPlayerData>(gameObject, true);

        myPublicFieldData.maxSpeedVar = movimentValues.maxSpeed;
        myPublicFieldData.velocityVar = movimentValues.velocityObsolete;
        if (resistanceController != null)
        {
            myPublicFieldData.resistanceVar = resistanceController.resistanceVar;
        }
        myPublicFieldData.maximumJumpForce = movimentValues.MaximumJumpForce;
        myPublicFieldData.rigidbody = MyFunctions.GetComponentInChilds<Rigidbody>(gameObject, true);
        setupChaserDataList(myPublicFieldData);
        setupGetTimeToReachPoint();
        PublicPlayerDataList.addPublicFieldPlayerData(myPublicFieldData);
    }
    public static void setupChaserDataList(PublicFieldPlayerData publicFieldPlayerData)
    {
        Animator animator = publicFieldPlayerData.animator;
        Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
        Vector3 position = publicFieldPlayerData.bodyTransform.InverseTransformPoint(head.TransformPoint(Vector3.up*0.15f));
        publicFieldPlayerData.addChaserData(new ChaserData(position, publicFieldPlayerData, MatchComponents.footballField.fullFieldArea, 0, "Head"));
        publicFieldPlayerData.addChaserData(new ChaserData(Vector3.zero, publicFieldPlayerData, MatchComponents.footballField.fullFieldArea, publicFieldPlayerData.playerComponents.scope, "Body"));
    }
    void setupGetTimeToReachPoint()
    {
        GetTimeToReachPoint getTimeToReachPoint = new GetTimeToReachPoint(myPublicFieldData);
        getTimeToReachPoint.getTimeToReachPointDelegate = getTimeToReachPoint.linearGetTimeToReachPosition;
        //getTimeToReachPoint.getTimeToReachPointDelegate = getTimeToReachPoint.accelerationGetTimeToReachPosition;
        //getTimeToReachPoint.getTimeToReachPointDelegate = getTimeToReachPoint.linearGetTimeToReachPosition;
        myPublicFieldData.playerComponents.GetTimeToReachPosition = getTimeToReachPoint;
    }
}
