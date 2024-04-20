using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFeatures : MonoBehaviour
{
    public int numPlayers;
    int currentPlayers;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool isFull()
    {
        if (currentPlayers == numPlayers)
        {
            return true;
        }
        return false;
    }
    public virtual void AddPlayer()
    {
        numPlayers++;
    }
}
