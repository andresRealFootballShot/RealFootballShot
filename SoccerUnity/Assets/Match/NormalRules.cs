using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class NormalRules : Rules
{
    public float cornerPlaceCountdown = 2, cornerKickCountdown;
    void Start()
    {
        enabledRules = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (enabledRules)
        {
            if (CheckCorner(out CornerComponents corner))
            {
                currentCorner = corner;
                Invoke(nameof(CornerPlaceBall), cornerPlaceCountdown);
                enabledRules = false;
                matchState = MatchState.Corner;
            }

            
        }
    }
    

    
}
