using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DOTS_ChaserDataCalculation;
public class StandardDOTSOptimalPointManager : MonoBehaviour, ILoad
{
    public new bool enabled = true;
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    public Rigidbody ballRigidbody;
    public SphereCollider ballSphereCollider;
    public bool updateChaserData;
    public int teamSize=22;
    public float force;
    public Transform testTransform;
    public OptimalPointDOTSCreator OptimalPointDOTSCreator;
    EventTrigger trigger = new EventTrigger();
    void Start()
    {
        //applyForceToBall();
        
    }

    public void Load(int level)
    {

        if (loadLevel == level)
        {
            Load();
        }
    }
    void Load()
    {
        if (enabled)
        {
            //MatchEvents.setMainBall.AddListener(applyForceToBall);
            //OptimalPointDOTSCreator.teamSize = teamSize;
            //MatchEvents.ballPhysicsMaterialLoaded.AddListener(OptimalPointDOTSCreator.createPathComponents);

            //MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(() => MatchEvents.addedPublicPlayerDataToList.AddListenerConsiderInvoked(addPlayerToOptimalPoint));
        }
    }
    void applyForceToBall()
    {
        Time.timeScale = 0.25f;
        float groundY = ballSphereCollider.radius * ballSphereCollider.transform.localScale.x;
        ballRigidbody.position = testTransform.position + Vector3.up * groundY;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = testTransform.forward * force;

    }
    public void addPlayerToOptimalPoint(PublicPlayerData publicPlayerData)
    {
        //if (OptimalPointDOTSCreator.publicPlayerDatas.Count >= OptimalPointDOTSCreator.teamSize || !publicPlayerData.addToOptimalPoint) return;

        //OptimalPointDOTSCreator.addPlayerToOptimalPointDOTS(publicPlayerData);
        /*
        if (publicPlayerData.useAccelerationInChaserDataCalculation)
        {
            //publicPlayerData.useAccelerationInChaserDataCalculation = false;
            for (int i = 0; i < 22; i++)
            {

            }
        }*/
    }
    void Update()
    {
        if (enabled)
        {
            //OptimalPointDOTSCreator.updateChaserDataOfPublicPlayerData();
            //OptimalPointDOTSCreator.updateComponents();
        }
        /*if (Input.GetKeyDown(KeyCode.V))
        {
            applyForceToBall();
            Time.timeScale = 0.15f;
        }*/
    }

}
