using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Rules : MonoBehaviour
{
    protected bool enabledRules { get => MatchComponents.enabledRules; set => MatchComponents.enabledRules = value; }
    protected float currentMatchTime { get => MatchComponents.RulesData.currentMatchTime; set => MatchComponents.RulesData.currentMatchTime = value; }
    protected bool inGame { get => MatchComponents.RulesData.inGame; set => MatchComponents.RulesData.inGame = value; }
    protected bool endGame { get => MatchComponents.RulesData.endGame; set => MatchComponents.RulesData.endGame = value; }
    protected CornerComponents currentCorner { get => MatchComponents.RulesData.currentCorner; set => MatchComponents.RulesData.currentCorner = value; }
    protected MatchState matchState { get => MatchComponents.RulesData.matchState; set => MatchComponents.RulesData.matchState = value; }
    
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
    }
    protected void StartCorner()
    {
        MatchComponents.enabledRules = true;
    }
}
