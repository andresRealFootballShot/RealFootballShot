using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    public MyEvent<Team> addTeamEvent = new MyEvent<Team>(nameof(addTeamEvent));
    public MyEvent<GameObject> instantiatedModel = new MyEvent<GameObject>(nameof(instantiatedModel));
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
