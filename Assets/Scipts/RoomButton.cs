using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text buttonText;
    private RoomInfo roomInfo;

    /// <summary>
    /// Set button details
    /// </summary>
    /// <param name="inputInfo"></param>
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        roomInfo = inputInfo;
        buttonText.text = roomInfo.Name;
    }

    /// <summary>
    /// Open room by calling join room button
    /// </summary>
    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(roomInfo);
    }
}
