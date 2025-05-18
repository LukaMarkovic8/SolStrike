using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShutDownOnFocus : MonoBehaviour
{
    public bl_WaitingRoomUI waitingRoomUI;
    void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log("Application focus changed: " + hasFocus);
        if (!hasFocus)
        {
            Debug.Log("Application lost focus, shutting down the waiting room UI.");
            waitingRoomUI.LeaveRoom(true);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
