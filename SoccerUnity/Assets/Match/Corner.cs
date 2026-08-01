using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Corner : Rules
{
    public float cornerPlaceCountdown = 2, cornerKickCountdown=1;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (enabledRules)
        {
            if (CheckCorner(out CornerComponents corner))
            {
                currentCorner = corner;
                MatchCtrl.Corner();
                
                Invoke(nameof(CornerPlaceBall), cornerPlaceCountdown);
                Invoke(nameof(StartCorner), cornerPlaceCountdown+ cornerKickCountdown);
               
               
            }

            
        }
    }
    

   
}
