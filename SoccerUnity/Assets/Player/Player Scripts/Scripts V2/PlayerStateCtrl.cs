using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCtrl : MonoBehaviour
{
    public Dictionary<PlayerState,GameObject> states = new Dictionary<PlayerState, GameObject>();
    public GameObject statesParent;
    public KickEvent kickEvent;
    PlayerState playerState;
    void Start()
    {
        kickEvent.Event += Kick;
        getStates();
    }
    void getStates()
    {
        List<PlayerStateMono> _states = MyFunctions.GetComponentsInChilds<PlayerStateMono>(statesParent,false, true);
        foreach (var item in _states)
        {
            states.Add(item.Value,item.gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    void Kick(Vector3 value)
    {
        if (playerState == PlayerState.LookingBall || playerState == PlayerState.WithPossession)
        {
            if (value.magnitude < 50)
            {
                changeStateTo(PlayerState.WithPossession);
            }
            else
            {
                changeStateTo(PlayerState.LookingBall);
            }
        }
    }
    public void changeStateTo(PlayerState playerState)
    {
        foreach (var item in states)
        {
            item.Value.SetActive(item.Key.Equals(playerState));
        }
    }
}
