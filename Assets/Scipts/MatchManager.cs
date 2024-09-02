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

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    #endregion

    #region Public Variables
    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();
    public int KillsToWin = 3;
    public Transform MapCamPoint;
    public GameState State = GameState.Waiting;
    public float WaitAfterEnding = 5f;

    #endregion

    #region Private Variables
    private int index;
    private List<LeaderBoardPlayer> leaderBoardPlayers = new List<LeaderBoardPlayer>();

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

            State = GameState.Playing;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && State != GameState.Ending)
        {
            if(UIController.instance.LeaderBoard.activeInHierarchy)
            {
                UIController.instance.LeaderBoard.SetActive(false);
            }
            else
            {
                ShowLeaderBoard();
            }
        }
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
        object[] package = new object[AllPlayers.Count + 1];
        package[0] = State;
        
        for(int i = 0; i < AllPlayers.Count; i++)
        {
            object[] packagePiece = new object[4];
            packagePiece[0] = AllPlayers[i].name;
            packagePiece[1] = AllPlayers[i].actor;
            packagePiece[2] = AllPlayers[i].kills;
            packagePiece[3] = AllPlayers[i].death;

            package[i + 1] = packagePiece;
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
        State = (GameState)dataRecieved[0];

        for(int i  = 1; i < dataRecieved.Length; i++)
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
                index = i - 1;
            }
        }
        StateCheck();
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
                /// Update leaderboard if open
                if(UIController.instance.LeaderBoard.activeInHierarchy)
                {
                    ShowLeaderBoard();
                }
                break;
            }
        }
        ScoreCheck();
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

    /// <summary>
    /// Display leaderboard
    /// </summary>
    void ShowLeaderBoard()
    {
        UIController.instance.LeaderBoard.SetActive(true);

        foreach(LeaderBoardPlayer lp in leaderBoardPlayers)
        {
            Destroy(lp.gameObject);
        }
        leaderBoardPlayers.Clear();

        UIController.instance.leaderBoardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayer(AllPlayers);
        foreach(PlayerInfo player in sorted)
        {
            LeaderBoardPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderBoardPlayerDisplay, UIController.instance.leaderBoardPlayerDisplay.transform.parent);
            newPlayerDisplay.SetDetails(player.name, player.kills, player.death);
            newPlayerDisplay.gameObject.SetActive(true);
            leaderBoardPlayers.Add(newPlayerDisplay);
        }
    }

    /// <summary>
    /// Sort according to kill count
    /// </summary>
    /// <param name="playerInfos"></param>
    /// <returns></returns>
    private List<PlayerInfo> SortPlayer(List<PlayerInfo> playerInfos)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while(sorted.Count < playerInfos.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = playerInfos[0];
            
            foreach(PlayerInfo player in playerInfos)
            {
                if(!sorted.Contains(player))
                {
                    if(player.kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.kills;
                    }
                }
            }
            sorted.Add(selectedPlayer);
        }

        return sorted;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        /// when match ends, leave room
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// check for winner, Whenever stats are updated
    /// </summary>
    void ScoreCheck()
    {
        bool winnerFound = false;
        foreach(PlayerInfo player in AllPlayers)
        {
            if(player.kills >= KillsToWin && KillsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if(winnerFound)
        {
            /// check if master client and game already not ended
            if(PhotonNetwork.IsMasterClient && State != GameState.Ending)
            {
                State = GameState.Ending;
                ListPlayersSend();
            }
        }
    }

    /// <summary>
    /// Functionality based on state of game, ENDING
    /// </summary>
    void StateCheck()
    {
        if(State == GameState.Ending)
        {
            EndGame();
        }
    }

    /// <summary>
    /// End current game
    /// </summary>
    void EndGame()
    {
        State = GameState.Ending;
        if(PhotonNetwork.IsMasterClient)
        {
            /// Destryo all instances after match
            PhotonNetwork.DestroyAll();
        }
        UIController.instance.EndScreen.SetActive(true);
        ShowLeaderBoard();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(EndGameCoroutine());
    }

    private IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(WaitAfterEnding);
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
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
