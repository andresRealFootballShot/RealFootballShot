using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class TimerObsolete : MonoBehaviourPunCallbacks,IPunObservable
{
    
    public Canvas canvas;
    public FloatMy min, seconds;
    public StringMy text;
    public float timeWaitCoroutine;
    public delegate void EmptyDelegate();
    public event EmptyDelegate EndEvent;
    public bool pause;
    bool startTimer;
    void Start()
    {
       canvas = transform.parent.GetComponent<Canvas>();
    }
    public void StartTimer()
    {
        
        pause = false;
        startTimer = true;
        //StartCoroutine(RunTimer());
    }
    public void Init()
    {
        canvas.enabled = true;
        min.Value = min.initialValue;
        seconds.Value = seconds.initialValue;
    }
    private void Update()
    {
        if (startTimer){
            if (min.Value >= 0)
            {
                if (!pause)
                {
                    text.Value = min.Value + ":" + Mathf.Round(seconds.Value).ToString();
                    if (seconds.Value <= 0)
                    {
                        min.Value--;
                        seconds.Value = seconds.initialValue;
                    }
                    seconds.Value-=Time.deltaTime;
                }
            }
            else
            {
                text.Value = 0 + ":" + 0;
                EndEvent?.Invoke();
                startTimer = false;
            }
        }
        
        
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(min.Value);
            stream.SendNext(seconds.Value);
        }
        else
        {
            min.Value = (float)stream.ReceiveNext();
            seconds.Value = (float)stream.ReceiveNext();
        }
    }
}
