using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterClientRPCSetup : MonoBehaviour
{
    EventTrigger trigger = new EventTrigger();
    void Start()
    {
        trigger.addTrigger(MatchEvents.teamsSetuped, false,1,true);
        trigger.addFunction(publicPlayerDataOfAddedPlayerToTeamIsAvailable);
        trigger.endLoadTrigger();
        
    }
    void publicPlayerDataOfAddedPlayerToTeamIsAvailable()
    {
        //MatchEvents.matchLoaded.Invoke();
        trigger.removeTrigger(MatchEvents.publicPlayerDataOfFieldPositionsAreAvailable);
        trigger = null;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
