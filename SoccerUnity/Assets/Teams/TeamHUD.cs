using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TeamHUD : MonoBehaviour
{
    public Team team;
    public TextMeshProUGUI goalsText;
    private void Start()
    {
        
    }
    public void goalAdded(GoalData goalData)
    {
        goalsText.text = team.goals.Count.ToString();
    }
}
