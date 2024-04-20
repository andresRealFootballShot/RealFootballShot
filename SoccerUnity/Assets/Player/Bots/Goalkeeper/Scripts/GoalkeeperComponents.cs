using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalkeeperComponents : PlayerComponents
{
    public GoalkeeperEvents goalkeeperEvents;
    public GoalkeeperCtrl goalkeeperCtrl;
    public GoalkeeperValues goalkeeperValues;
    public SetupGoalkeeper setupGoalkeeper;
    void Start()
    {
        base.setPlayerComponents();
        setupMovimentValues();
    }
}
