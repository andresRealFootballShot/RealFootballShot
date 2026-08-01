using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Rules : MonoBehaviour
{
    protected bool enabledRules { get => MatchComponents.enabledRules; }
    protected MatchData MatchData { get => MatchComponents.MatchData; }
    protected MatchCtrl MatchCtrl { get => MatchComponents.MatchCtrl; }
    protected float currentMatchTime { get => MatchComponents.MatchData.currentMatchTime; set => MatchComponents.MatchData.currentMatchTime = value; }
    protected bool inGame { get => MatchComponents.MatchData.inGame; set => MatchComponents.MatchData.inGame = value; }
    protected bool endGame { get => MatchComponents.MatchData.endMatch; set => MatchComponents.MatchData.endMatch = value; }
    protected CornerComponents currentCorner { get => MatchComponents.MatchData.currentCorner; set => MatchComponents.MatchData.currentCorner = value; }
    
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    protected bool CheckCorner(out CornerComponents cornerResult)
    {
        bool isForward, isInside;
        foreach (Team team in Teams.teamsList)
        {
            SideOfField sideOfField = team.SideOfField;
            foreach (var corner in sideOfField.corners)
            {
                foreach (var plane in corner.planes)
                {
                    plane.PointIsForward(MatchComponents.ballComponents.rigBall.position, out isInside, out isForward, corner.name);
                    if (isInside && !isForward)
                    {
                        cornerResult = corner;
                        
                        return true;
                    }
                }
            }
        }
        cornerResult = null;
        return false;
    }
    protected void CornerPlaceBall()
    {
        MatchComponents.ballPosition = currentCorner.cornerPoint.position;
        MatchComponents.ballRigidbody.velocity = Vector3.zero;
        MatchComponents.ballRigidbody.angularVelocity = Vector3.zero;
        foreach(Team team in Teams.teamsList)
        {
            team.StartPressurePosition();
        }

    }
    protected void StartCorner()
    {
        MatchCtrl.EnableGame();
    }
}
