using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NickNameHUDCtrl : MonoBehaviourPunCallbacks
{
    public GameObject nickNameGObj;
    public Vector3 offsetPos;
    public PublicPlayerData publicPlayerData;
    public TextHUD text;
    Transform transCamera;
    void Start()
    {
        transCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }
    // Update is called once per frame
    void Update()
    {
        nickNameGObj.transform.position = publicPlayerData.bodyTransform.position + offsetPos;
        nickNameGObj.transform.LookAt(nickNameGObj.transform.position + transCamera.transform.forward);
    }
    public void setColor(Equipement equipement)
    {
        text.SetVariableColor(equipement.mainColorVar);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {

    }
}
