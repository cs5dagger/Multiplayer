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
    void Start()
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

    void Update()
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
        ListPlayersSend();
    }

    /// <summary>
    /// Send new player list
    /// </summary>
    public void ListPlayersSend()
    {
        object[] package = new object[AllPlayers.Count];
        
        for(int i = 0; i < AllPlayers.Count; i++)
        {
            object[] packagePiece = new object[4];
            packagePiece[0] = AllPlayers[i].name;
            packagePiece[1] = AllPlayers[i].actor;
            packagePiece[2] = AllPlayers[i].kills;
            packagePiece[3] = AllPlayers[i].death;

            package[i] = packagePiece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All},
            new SendOptions { Reliability = true }
            );
    }

    /// <summary>
    /// Recieve new player list
    /// </summary>
    public void ListPlayersReceive(object[] dataRecieved)
    {
        AllPlayers.Clear();
        for(int i  = 0; i < dataRecieved.Length; i++)
        {
            object[] piece = (object[])dataRecieved[i];
            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );
            AllPlayers.Add(player);

            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                /// Current (our) player number
                index = i;
            }
        }
    }

    /// <summary>
    /// Send new stats
    /// </summary>
    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange};
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    /// <summary>
    /// Recieve new stats
    /// </summary>
    public void UpdateStatsReceive(object[] dataRecieved)
    {
        int actor = (int)dataRecieved[0];
        int statType = (int)dataRecieved[1];
        int amount = (int)dataRecieved[2];

        for(int i = 0;i < AllPlayers.Count; i++)
        {
            if (AllPlayers[i].actor == actor)
            {
                switch(statType)
                {
                    case 0: // kills
                        AllPlayers[i].kills += amount;
                        break;
                    case 1: // deaths
                        AllPlayers[i].death += amount;
                        break;
                }

                if(i == index)
                {
                    UpdateStatsToDisplay();
                }
                break;
            }
        }
    }

    /// <summary>
    /// Update stats on display
    /// </summary>
    public void UpdateStatsToDisplay()
    {
        if (AllPlayers.Count > index)
        {
            UIController.instance.KillsText.text = "KILLS: " + AllPlayers[index].kills;
            UIController.instance.DeathsLabel.text = "DEATHS: " + AllPlayers[index].death;
        }
        else
        {
            UIController.instance.KillsText.text = "KILLS: 0";
            UIController.instance.DeathsLabel.text = "DEATHS: 0";
        }
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
