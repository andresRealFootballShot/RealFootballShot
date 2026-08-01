using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Photon.Pun.UtilityScripts.PunTeams;

public class Chronometer : Rules
{
    
    
    public int partsSize = 2;
    [Header("Time")]
    [Space(5)]
    public int minutes=5;
    public int seconds = 0;
   
    float totalSeconds{ get => minutes * 60 + seconds; }
    float partTime { get => (float)totalSeconds / partsSize; }
    int currentPart=1;
   
    public float normalizedTime { get=>currentMatchTime/(minutes*60+seconds); }
    public int currentMatchMinutes { get=>MatchComponents.MatchData.currentMatchMinutes; }
    public int currentMatchSeconds { get => MatchComponents.MatchData.currentMatchSeconds; }
    public int restMatchMinutes { get=> MatchComponents.MatchData.restMatchMinutes; }
    public int restMatchSeconds { get => MatchComponents.MatchData.restMatchSeconds; }
    
    public MatchCtrl matchSetup;
    void Start()
    {
        
        
        
        
    }
    
   
    void Update()
    {
        updateMatchTime();
        checkEndMatch();
        checkEndPart();
    }
    void updateMatchTime()
    {
        if (inGame)
        {

            currentMatchTime = Mathf.Clamp(currentMatchTime + Time.deltaTime, 0, totalSeconds);
        }
    }
    void checkEndPart()
    {
        if (currentMatchTime>=partTime*currentPart&&inGame)
        {
            currentPart++;
            if (currentPart > 0)
            {
                MatchComponents.MatchCtrl.changeSideOfField();
            }
            MatchComponents.MatchCtrl.StartContinueMatch();
            
        }
    }
    void checkEndMatch()
    {
        if (currentMatchTime >= totalSeconds&& !endGame)
        {

            MatchComponents.MatchCtrl.EndMatch();
        }
    }
  
 
}
