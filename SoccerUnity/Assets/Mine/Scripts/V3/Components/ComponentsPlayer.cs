using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ComponentsPlayer : MonoBehaviour
{
    public static PlayerIDMonoBehaviour myMonoPlayerID;
    public static PublicFieldPlayerData publicPlayerData;
    public Rigidbody rigBody;
    public Transform transBody,transCamera,transPivotCamera2,transPivotCamera1,transModelo;
    public CapsuleCollider colliderPlayer;
    public Animator animatorPlayer;
    public new Camera camera;
    //public PhotonView photonView;
    public BallComponents componentsBall;
    public GameObject behaviourGObj;
    //public GameObject hudGObj;
    public GameObject cameraBehaviourGObj,movimentGObj,kickGObjt,AimGObj,ArrowGObj,TransparencyGObj,PlayerModeCtrlGObj,eventsGObj;
    public Vector3 localPositionPivot1,localPositionPivot2;
    public ScriptsPlayer scriptsPlayer;
    public PlayerComponents playerComponents;
    public static ComponentsPlayer currentComponentsPlayer;
    bool isEnabled=true;
    void Awake()
    {
        /*
        if (enableAll)
        {
            EnableAll();
        }
        else
        {
            DisableAll();
        }*/
    }
    private void Start()
    {
        
    }
    public void SetParentPivot(GameObject gameObject)
    {
        transPivotCamera1.parent = gameObject.transform;
        transPivotCamera1.localPosition = localPositionPivot1;
    }
    public void EnableOnlyCamera()
    {
        if (isEnabled)
        {
            cameraBehaviourGObj.SetActive(true);
            movimentGObj.SetActive(false);
            kickGObjt.SetActive(false);
            AimGObj.SetActive(false);
            //hudGObj.SetActive(false);
            scriptsPlayer.hudCtrl.HideHUD();
            ArrowGObj.SetActive(false);
            TransparencyGObj.SetActive(false);
            PlayerModeCtrlGObj.SetActive(false);
            eventsGObj.SetActive(false);
            //behaviourGObj.SetActive(true);
            animatorPlayer.SetFloat("vertical", 0);
            animatorPlayer.SetFloat("sprint", 0);
        }
    }
    public void InvokeEnableAll(float time)
    {
        Invoke(nameof(EnableAll), time);
    }
    public void pruebasEnableOnlyCamera()
    {
        cameraBehaviourGObj.SetActive(true);
        movimentGObj.SetActive(false);
        kickGObjt.SetActive(false);
        AimGObj.SetActive(false);
        //hudGObj.SetActive(false);
        ArrowGObj.SetActive(false);
        TransparencyGObj.SetActive(false);
        PlayerModeCtrlGObj.SetActive(false);
        eventsGObj.SetActive(false);
        //behaviourGObj.SetActive(true);
    }
    public void pruebasEnableAll()
    {
        cameraBehaviourGObj.SetActive(true);
        movimentGObj.SetActive(true);
        kickGObjt.SetActive(true);
        AimGObj.SetActive(true);
        //hudGObj.SetActive(true);
        ArrowGObj.SetActive(true);
        TransparencyGObj.SetActive(true);
        PlayerModeCtrlGObj.SetActive(true);
        eventsGObj.SetActive(true);
        //behaviourGObj.SetActive(true);
    }
    public void EnableAll()
    {
        cameraBehaviourGObj.SetActive(true);
        movimentGObj.SetActive(true);
        kickGObjt.SetActive(true);
        AimGObj.SetActive(true);
        //hudGObj.SetActive(true);
        scriptsPlayer.hudCtrl.ShowHUD();
        ArrowGObj.SetActive(true);
        TransparencyGObj.SetActive(true);
        PlayerModeCtrlGObj.SetActive(true);
        eventsGObj.SetActive(true);
        isEnabled = true;
        //behaviourGObj.SetActive(true);
    }
    public void DisableAll()
    {
        cameraBehaviourGObj.SetActive(false);
        movimentGObj.SetActive(false);
        kickGObjt.SetActive(false);
        AimGObj.SetActive(false);
        //hudGObj.SetActive(false);
        scriptsPlayer.hudCtrl.HideHUD();
        ArrowGObj.SetActive(false);
        TransparencyGObj.SetActive(false);
        PlayerModeCtrlGObj.SetActive(false);
        eventsGObj.SetActive(false);
        isEnabled = false;
        animatorPlayer.SetFloat("vertical", 0);
        animatorPlayer.SetFloat("sprint", 0);
        //behaviourGObj.SetActive(false);
    }
    public void Copy(ComponentsPlayer components)
    {
        rigBody = components.rigBody;
        transBody = components.transBody;
        colliderPlayer = components.colliderPlayer;
        animatorPlayer = components.animatorPlayer;
        //photonView = components.photonView;
    }
}