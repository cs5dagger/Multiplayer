using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region Instance And Enums
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }

    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }

    #endregion

    #region Public Variables
    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();
    #endregion

    #region Private Variables
    private int index;

    #endregion

    #region Methods and Overrides
    private void Start()
    {
        /// If not connected to network, load main menu scene
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    private void Update()
    {

    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        /// Code above 200 are reserved by photon
        if(photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;
            
            switch(theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatsReceive(data);
                    break;
            }
        }
    }

    /// <summary>
    /// Send new player
    /// </summary>
    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient},
            new SendOptions { Reliability = true}
            );
    }

    /// <summary>
    /// Recieve new player
    /// </summary>
    public void NewPlayerReceive(object[] dataRecieved)
    {
        PlayerInfo playerInfo = new PlayerInfo((string)dataRecieved[0], (int)dataRecieved[1], (int)dataRecieved[2], (int)dataRecieved[3]);
        AllPlayers.Add(playerInfo);
    }

    /// <summary>
    /// Send new player list
    /// </summary>
    public void ListPlayersSend()
    {

    }

    /// <summary>
    /// Recieve new player list
    /// </summary>
    public void ListPlayersReceive(object[] dataRecieved)
    {

    }

    /// <summary>
    /// Send new stats
    /// </summary>
    public void UpdateStatsSend()
    {

    }

    /// <summary>
    /// Recieve new stats
    /// </summary>
    public void UpdateStatsReceive(object[] dataRecieved)
    {

    }

    #endregion

    #region Helper Class

    /// make this visible in inspector
    [System.Serializable]
    public class PlayerInfo
    {
        public string name;
        public int actor /*network assigned number*/, kills, death;
        
        public PlayerInfo(string _name, int _actor, int _kills, int _death)
        {
            name = _name;
            actor = _actor;
            kills = _kills;
            death = _death;
        }
    }

    #endregion
}
