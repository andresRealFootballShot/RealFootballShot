using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class FindMatch : MonoBehaviour
{
    string nameMatch;
    public Button button;
    private void Start()
    {
        button.interactable = false;
    }
    public void NameMatchChanged(string value)
    {
        nameMatch = value;
        button.interactable = !value.Equals("");
    }
    public void FindMatch_Click()
    {
        if(nameMatch!=string.Empty)
            PhotonNetwork.JoinRoom(nameMatch);
    }
}
