using DOTS_ChaserDataCalculation;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PublicPlayerData : MonoBehaviour
{
    public Variable<float> maxSpeedVar,velocityVar = new Variable<float>(),resistanceVar;
    public Variable<string> playerNameVar = new Variable<string>();
    public Variable<Vector3> initPosition = new Variable<Vector3>();
    public Variable<Quaternion> initRotation = new Variable<Quaternion>();
    public float maxSpeed { get {return maxSpeedVar.Value; } set { maxSpeedVar.Value=value; } }
    public Vector3 velocity { get { return playerData.Velocity; } set { playerData.Velocity = value; } }
    public float speed { get { return playerData.Speed; }}
    public float resistance { get { return resistanceVar.Value; } set { resistanceVar.Value = value; } }
    public float bodyRadio { get { return playerData.bodyRadio; } set { playerData.bodyRadio = value; } }
    public Vector3 position { get { return bodyTransform.position; } set { bodyTransform.position = value; } }
    public Vector3 InitPosition { get { return initPosition.Value; } set { initPosition.Value = value; } }
    public Quaternion InitRotation { get { return initRotation.Value; } set { initRotation.Value = value; } }
    //public float maximumJumpHeight { get; set; }
    public SortedList<float, Area> maximumJumpHeights { get; set; } = new SortedList<float, Area>();
    public string playerName { get { return playerNameVar.Value; } set { playerNameVar.Value = value; } }
    public string playerID { get { return playerIDMono.getStringID(); } set { playerIDMono.RemoteLoad(value); } }

    public List<ChaserData> ChaserDataList { get; set; } = new List<ChaserData>();
    //public MyEvent addedChaserDataEvent;
    public Variable<MyEvent> addedChaserDataEvent = new Variable<MyEvent>();
    public PlayerIDMonoBehaviour playerIDMono;
    public bool playerIDIsLoaded { get => playerIDMono.playerID != null; }
    public Transform bodyTransform;
    public CollisionEvent collisionEvent;
    public SetupModel setupModel;
    public NickNameHUDCtrl nickNameHUDCtrl;
    public bool useAccelerationInChaserDataCalculation=true;
    public bool addToOptimalPoint=true;
    public MovimentValues movimentValues { get => playerComponents.movementValues; }
    public PlayerData playerData { get => playerComponents.playerData; }
    public PlayerComponents playerComponents;
    public virtual bool IsGoalkeeper { get => false; }
    public float getTimeToReachPosition(Vector3 position,float scope)
    {
        return playerComponents.GetTimeToReachPosition.getTimeToReachPointDelegate(position,scope);
    }
    private void Start()
    {
        addedChaserDataEvent.Value = new MyEvent(nameof(addedChaserDataEvent) + GetInstanceID());
        
    }
    public static void getPlayerData(PublicPlayerData publicPlayerData, int index, out PlayerDataComponent playerDataComponent)
    {

        float maximumJumpHeight = 0;
        if (publicPlayerData.maximumJumpHeights.Count > 0)
        {

            maximumJumpHeight = publicPlayerData.maximumJumpHeights.Keys[0];
        }
        MovimentValues movimentValues = publicPlayerData.movimentValues;
        PlayerComponents playerComponents = publicPlayerData.playerComponents;
        Vector3 bodyY0Forward = publicPlayerData.bodyTransform.forward;
        float maxSpeed = publicPlayerData.maxSpeed;
        bool isGoalkeeper = false;
        playerDataComponent = new PlayerDataComponent(publicPlayerData.useAccelerationInChaserDataCalculation, index, publicPlayerData.position, bodyY0Forward, playerComponents.Velocity, playerComponents.Velocity.normalized, maxSpeed, publicPlayerData.playerComponents.movementValues.forwardAcceleration, playerComponents.movementValues.forwardDeceleration, maximumJumpHeight, playerComponents.scope, playerComponents.Speed, movimentValues.minSpeedForRotateBody, movimentValues.maxAngleForRun, playerComponents.maxSpeedRotation, movimentValues.maxSpeedForReachBall, MatchComponents.ballRigidbody.drag, Vector3.up, MatchComponents.footballField.position, 0, publicPlayerData.playerComponents.bodyBallRadio, publicPlayerData.playerData.height, isGoalkeeper, publicPlayerData.movimentValues.NormalMaximumJumpHeight, -1, publicPlayerData.playerData.height * 0.75f, publicPlayerData.playerComponents.soccerPlayerData.maxKickForce);
    }
    public void addChaserData(ChaserData chaserData)
    {
        ChaserDataList.Add(chaserData);
        addedChaserDataEvent.Value?.Invoke();
    }
    public virtual bool maximumJumpHeightIsInArea(float maximumJumpHeight,Vector3 point)
    {
        return true;
    }
    public virtual float getMaximumJumpHeightOfPoint(Vector3 point)
    {
        foreach (var item in maximumJumpHeights)
        {
            if (item.Value == null){
                return item.Key;
            }
            else if (item.Value.PointIsInside(point))
            {
                return item.Key;
            }
        }
        return maximumJumpHeights.Count > 0 ? maximumJumpHeights.IndexOfKey(0) : 0;
    }
    public void addMaximumJumpHeight(float value,Area area)
    {
        if (!maximumJumpHeights.ContainsKey(value))
        {
            maximumJumpHeights.Add(value, area);
        }
    }
    public bool getFirstChaserData(out ChaserData chaserData)
    {
        if (ChaserDataList.Count > 0)
        {
            chaserData = ChaserDataList[0];
            return true;
        }
        else
        {
            chaserData = null;
            return false;
        }
    }
}
