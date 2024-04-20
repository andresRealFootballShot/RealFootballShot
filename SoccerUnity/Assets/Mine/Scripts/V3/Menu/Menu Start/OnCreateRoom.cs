using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class OnCreateRoom : MonoBehaviourPunCallbacks
{
    
    public GameObject typeMatchDataPref;
    [HideInInspector]
    public string typeMatchString;
    public bool isPublic;
    void Start()
    {
        
    }
    public void setTypeMatchData(string typeMatch,bool isPublic)
    {
        this.typeMatchString = typeMatch;
        this.isPublic = isPublic;
    }
    public override void OnCreatedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            
            PhotonNetwork.AutomaticallySyncScene = true;
            //SceneSetup.setStaticSetup(SceneTypeID.NormalMatch, SceneModeID.Online, false, false);
            /*GameObject gameObject = PhotonNetwork.InstantiateSceneObject(typeMatchDataPref.name, Vector3.zero, Quaternion.identity, 0, null);
            DontDestroyOnLoad(gameObject);
            TypeNormalMatch typeMatch = (TypeNormalMatch)System.Enum.Parse(typeof(TypeNormalMatch), typeMatchString);
            TypeMatch.SendTypeMatchData(typeMatchString, isPublic, TypeMatch.sizeFootballFieldDictionary[typeMatch]);
            */

            
            MatchData.matchState = MatchState.WaitingForWarmUp;
            //LoadScene.notifyClearBeforeLoadScene();
            PhotonNetwork.LoadLevel("Scenes/" + TypeMatch.getNameScene(typeMatchString));
        }
    }

}
