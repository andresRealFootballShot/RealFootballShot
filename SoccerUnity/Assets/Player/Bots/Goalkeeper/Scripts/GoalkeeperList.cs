using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalkeeperList : MonoBehaviour,ILoad
{
    public static List<GoalkeeperCtrl> goalkeeperCtrlList = new List<GoalkeeperCtrl>();
    public static int _loadLevel = 0;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }


    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    public void Load()
    {
        goalkeeperCtrlList.Clear();
    }
    public static void addGoalkeeper(GoalkeeperCtrl goalkeeper)
    {
        goalkeeperCtrlList.Add(goalkeeper);
    }
}
