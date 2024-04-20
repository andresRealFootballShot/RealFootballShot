using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineErrorHandler : MonoBehaviour
{
    public GameObject menuErrorOnlinePrefab;
    static GameObject staticMenuErrorOnlinePrefab;
    public static string onlineErrorMessage = "You left the game because there was an error";

    private void Awake()
    {
        staticMenuErrorOnlinePrefab = menuErrorOnlinePrefab;
    }
    public static void OnlineError(string info)
    {
        Debug.LogError("OnlineError dont implemented");
        return;
        GameObject newMenuErrorOnline = Instantiate(staticMenuErrorOnlinePrefab);
        DontDestroyOnLoad(newMenuErrorOnline);
        MenuErrorOnlineCtrl menuErrorOnlineCtrl = newMenuErrorOnline.GetComponent<MenuErrorOnlineCtrl>();
        menuErrorOnlineCtrl.setup("Start", 3, onlineErrorMessage + " | "+info, true);
        LeaveRoom.leaveRoom();
    }
}
