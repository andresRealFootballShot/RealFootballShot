using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class OnlineEvents : MonoBehaviour, IClearBeforeLoadScene
{
    
    public static MyEvent myPlayerIDSetuped = new MyEvent(nameof(myPlayerIDSetuped));
    public static MyEvent joinedRoom = new MyEvent(nameof(joinedRoom));
    public static MyEvent masterClientRPCsInstantiated = new MyEvent(nameof(masterClientRPCsInstantiated));
    public static MyEvent connected = new MyEvent(nameof(connected));
    public static MyEvent createRoomFailed = new MyEvent(nameof(createRoomFailed));
    public void Clear()
    {
        myPlayerIDSetuped = new MyEvent(nameof(myPlayerIDSetuped));
        joinedRoom = new MyEvent(nameof(joinedRoom));
        masterClientRPCsInstantiated = new MyEvent(nameof(masterClientRPCsInstantiated));
        connected = new MyEvent(nameof(connected));
        createRoomFailed = new MyEvent(nameof(createRoomFailed));
    }
}
