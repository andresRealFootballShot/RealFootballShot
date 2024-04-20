using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class FueraDeJuego : MonoBehaviour
{
    public class PlayerFueraDeJuego{
       public Transform trans;
        public int actor;
        public PlayerFueraDeJuego(int actor,Transform trans)
        {
            this.actor = actor;
            this.trans = trans;
        }
    }
    public float lineaRed, lineaBlue;
    public Transform center;
    List<PlayerFueraDeJuego> list=new List<PlayerFueraDeJuego>();
    List<int> playersInFueraDeJuego = new List<int>();
    public MatchDataObsolete matchData;
    public Teams teams;
    int actorWithPosession;
    public AudioSource audioSource;
    public float distance;
    public ComponentsPlayer componentsPlayer;
    public BallComponents componentsBall;
    public PhotonView pVPlayer;
    public AudioSource pitido;
    public Canvas canvas;
    void Start()
    {
        componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        //kickEvent.Event += OwnershipBall;

    }
    public void Animation()
    {
        pitido.Play();
        canvas.enabled = true;
        Invoke("EndAnimation",1.5f);
    }
    void EndAnimation()
    {
        canvas.enabled = false;
    }
    public void AddPlayer(int actor,Transform trans)
    {
        if(list.Find(x => x.actor == actor) == null)
        {
            list.Add(new PlayerFueraDeJuego(actor, trans));
        }
    }
    void OwnershipBall(int actor)
    {
        if (!teams.isPortero(actor))
        {
            actorWithPosession = actor;
            if (matchData.fueraDeJuego)
            {
                matchData.fueraDeJuego=false;
            }
            if (matchData.matchStarted && !matchData.endMatch && matchData.enableGoals)
            {
                float pos = center.position.z;
                string team = teams.getTeamFromActor(actorWithPosession).TeamName;
                if (team == "Blue")
                {
                    foreach (PlayerFueraDeJuego player in list)
                    {
                        if (teams.getTeamFromActor(player.actor).TeamName == "Red")
                        {

                            if (pos < player.trans.position.z)
                            {
                                pos = player.trans.position.z;
                            }
                        }
                    }
                    playersInFueraDeJuego.Clear();
                    foreach (PlayerFueraDeJuego player in list)
                    {
                        if (teams.getTeamFromActor(player.actor).TeamName == "Blue" && player.actor != actorWithPosession)
                        {
                            if (pos < player.trans.position.z)
                            {
                                playersInFueraDeJuego.Add(player.actor);
                            }
                        }
                    }
                }
                else
                {
                    foreach (PlayerFueraDeJuego player in list)
                    {
                        if (teams.getTeamFromActor(player.actor).TeamName == "Blue")
                        {

                            if (pos > player.trans.position.z)
                            {
                                pos = player.trans.position.z;
                            }
                        }
                    }
                    playersInFueraDeJuego.Clear();
                    foreach (PlayerFueraDeJuego player in list)
                    {
                        if (teams.getTeamFromActor(player.actor).TeamName == "Red" && player.actor != actorWithPosession)
                        {
                            if (pos > player.trans.position.z)
                            {
                                playersInFueraDeJuego.Add(player.actor);
                            }
                        }
                    }
                }
            }
        }
    }
    private void Update()
    {
        foreach(int actor in playersInFueraDeJuego)
        {
            Transform trans = list.Find(x => x.actor == actor).trans;
            if (Vector3.Distance(trans.position, componentsBall.transBall.position) < 1)
            {
                fueraDeJuego();
            }
        }
        if (matchData.fueraDeJuego && matchData.matchStarted && !matchData.endMatch && matchData.enableGoals)
        {
            
            string teamSaque = teams.getTeamFromActor(actorWithPosession).TeamName;
            if (teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber).name == teamSaque)
            {
                Vector3 dir = componentsPlayer.transBody.position - new Vector3(componentsBall.transBall.position.x, componentsPlayer.transBody.position.y, componentsBall.transBall.position.z);
                if (dir.magnitude < distance)
                {
                    componentsPlayer.transBody.position = new Vector3(componentsBall.transBall.position.x,componentsPlayer.transBody.position.y, componentsBall.transBall.position.z) + dir.normalized * distance;
                }
            }
        }
    }
    public void fueraDeJuego()
    {
        
        if (!matchData.fueraDeJuego)
        {
            pVPlayer.RPC("SetFueraDeJuego", RpcTarget.All, true);
        }
    }
}
