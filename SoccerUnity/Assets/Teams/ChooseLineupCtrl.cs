using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseLineupCtrl : MonoBehaviour
{
    public class ChooseLineupData
    {
        public TypeNormalMatch typeMatchName;
        public Lineup.TypeLineup typeLineup;
        public ChooseLineupData(TypeNormalMatch typeMatchName, Lineup.TypeLineup typeLineup)
        {
            this.typeMatchName = typeMatchName;
            this.typeLineup = typeLineup;
        }
    }

    public Transform midfield;
    public ChooseFieldPositionCtrl chooseFieldPositionCtrl;
    void Start()
    {

    }
    public static void chooseLineup(Team team, Lineup.TypeLineup typeLineup)
    {
        team.setLineup(typeLineup);
    }
}
