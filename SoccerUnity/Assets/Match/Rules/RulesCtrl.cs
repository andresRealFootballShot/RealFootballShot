using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulesCtrl : MonoBehaviour
{
    static GameObject staticRulesCheckers;
    public GameObject rulesCheckers;
    public static bool checkersEnabled;
    void Awake()
    {
        staticRulesCheckers = rulesCheckers;
        MatchEvents.startMatch.AddListener(Enable);
        MatchEvents.continueMatch.AddListener(Enable);
        MatchEvents.endPart.AddListener(Disable);
        MatchEvents.stopMatch.AddListener(Disable);
        MatchEvents.endMatch.AddListener(Disable);
    }

    public static void Enable()
    {
        if (MatchData.ImReferee)
        {
            //staticRulesCheckers.SetActive(true);
            checkersEnabled = true;
        }
    }
    public static void Disable()
    {
        //staticRulesCheckers.SetActive(false);
        checkersEnabled = false;
    }
}
