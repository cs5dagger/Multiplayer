using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Unity.VisualScripting;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Public Variables
    
    public static Launcher Instance;
    public GameObject LoadingScreen;
    public TMP_Text LoadingText; 
    public GameObject MenuButtons;
    public GameObject CreateRoomScreen;
    public TMP_InputField RoomNameInput;
    public GameObject RoomScreen;
    public TMP_Text RoomNameText, PlayerNameLabel;
    public GameObject ErrorScreen;
    public TMP_Text ErrorText;
    public GameObject RoomBrowserScreen;
    public RoomButton RoomButton;
    public GameObject NameInputScreen;
    public TMP_InputField NameInputText;
    public string LevelToPlay;
    public GameObject StartButton;
    public GameObject RoomTestButton;

    #endregion

    #region Private Variables 

    private bool HasSetNickName;
    private List<TMP_Text> AllPlayerNames = new List<TMP_Text>();
    private List<RoomButton> AllRoomButtons = new List<RoomButton>();

    #endregion

    #region Overrides and Methods

    /// <summary>
    /// Awake method of unity
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Start method of unity
    /// </summary>
    private void Start()
    {
        CloseMenus();
        LoadingScreen.SetActive(true);
        LoadingText.text = "Connecting To Network...";
        PhotonNetwork.ConnectUsingSettings(); /// Uses photon server settings to connect to photon network

#if UNITY_EDITOR
        RoomTestButton.SetActive(true);
#endif

    }

    /// <summary>
    /// Method called when connected to master server and ready for matchmaking or lobby
    /// </summary>
    public override void OnConnectedToMaster()
    {
        //base.OnConnectedToMaster();
        //CloseMenus();
        //MenuButtons.SetActive(true);
        PhotonNetwork.JoinLobby();

        /// Tells the client to load same scene as master
        PhotonNetwork.AutomaticallySyncScene = true;
        
        LoadingText.text = "Joining Lobby...";
    }

    /// <summary>
    /// Called when enter a lobby on master server
    /// </summary>
    public override void OnJoinedLobby()
    {
        //base.OnJoinedLobby();
        CloseMenus();
        MenuButtons.SetActive(true);

        /// nickname is the displayed name to us
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if(!HasSetNickName)
        {
            CloseMenus();
            NameInputScreen.SetActive(true);
            
            /// If we have name stored, than set name to that string
            if(PlayerPrefs.HasKey("PlayerName"))
            {
                NameInputText.text = PlayerPrefs.GetString("PlayerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        }
    }

    /// <summary>
    /// Close all menus and buttons
    /// </summary>
    void CloseMenus()
    {
        LoadingScreen.SetActive(false);
        MenuButtons.SetActive(false);
        CreateRoomScreen.SetActive(false);
        RoomScreen.SetActive(false);
        ErrorScreen.SetActive(false);
        RoomBrowserScreen.SetActive(false);
        NameInputScreen.SetActive(false);
    }

    /// <summary>
    /// Open create room menu
    /// Called on button click in scene
    /// </summary>
    public void OpenCreateRoom()
    {
        CloseMenus();
        CreateRoomScreen.SetActive(true);
    }

    /// <summary>
    /// Create Room
    /// </summary>
    public void CreateRoom()
    {
        if(!string.IsNullOrEmpty(RoomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(RoomNameInput.text, options);
            CloseMenus();
            LoadingText.text = "Creating Room...";
            LoadingScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Set name of the joined room
    /// </summary>
    public override void OnJoinedRoom()
    {
        CloseMenus();
        RoomScreen.SetActive(true);

        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        /// if current player is the master enable start game button
        if(PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    /// <summary>
    /// Display name of all players in the lobby
    /// </summary>
    private void ListAllPlayers()
    {
        foreach(TMP_Text player in AllPlayerNames)
        {
            Destroy(player.gameObject);
        }
        AllPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for(int i=0;i<players.Length;i++)
        {
            TMP_Text newPlayerLabel = Instantiate(PlayerNameLabel, PlayerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);
            AllPlayerNames.Add(newPlayerLabel);
        }
    }

    /// <summary>
    /// When player enters the room
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(PlayerNameLabel, PlayerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);
        AllPlayerNames.Add(newPlayerLabel);
    }

    /// <summary>
    /// When player leaves the room
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    /// <summary>
    /// Called when room creation failed
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenus();
        ErrorText.text = $"Failed to create new room : {message}";
        ErrorScreen.SetActive(true);
    }

    /// <summary>
    /// Close error screen and open main menu
    /// </summary>
    public void CloseErrorScreen()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    /// <summary>
    /// Leave room
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        LoadingText.text = "Leaving Room... ";
        LoadingScreen.SetActive(true);
    }

    /// <summary>
    /// After room is left
    /// </summary>
    public override void OnLeftRoom()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }
    
    /// <summary>
    /// open room browser window
    /// </summary>
    public void OpenRoomBrowser()
    {
        CloseMenus();
        RoomBrowserScreen.SetActive(true);
    }

    /// <summary>
    /// Close room browser window
    /// </summary>
    public void CloseRoomBrowser()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    /// <summary>
    /// Called when room list is updated
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomButton rb in AllRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        AllRoomButtons.Clear();

        RoomButton.gameObject.SetActive(false);

        for(int i=0;i<roomList.Count;i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                /// instantiate new button
                RoomButton newButton = Instantiate(RoomButton, RoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);
                AllRoomButtons.Add(newButton);
            }
        }
    }

    /// <summary>
    /// Join room
    /// </summary>
    /// <param name="inputInfo"></param>
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseMenus();
        LoadingText.text = "Joining Room...";
        LoadingScreen.SetActive(true);
    }

    /// <summary>
    /// Set Player nick name
    /// </summary>
    public void SetNickName()
    {
        if(!string.IsNullOrEmpty(NameInputText.text))
        {
            PhotonNetwork.NickName = NameInputText.text;

            /// use playerprefs for storing nick name to avoid new name everytime game runs
            PlayerPrefs.SetString("PlayerName", NameInputText.text);

            CloseMenus();
            MenuButtons.SetActive(true);
            HasSetNickName = true;
        }
    }

    /// <summary>
    /// Method to start game and tell master the scene to load
    /// </summary>
    public void StartGame()
    {
        PhotonNetwork.LoadLevel(LevelToPlay);
    }

    /// <summary>
    /// Switch master if previous master terminates the game
    /// </summary>
    /// <param name="newMasterClient"></param>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        /// if current player is the master enable start game button
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    /// <summary>
    /// Method to run quick join on unity editor for test
    /// </summary>
    public void QuickJoin()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        PhotonNetwork.CreateRoom("Test", options);
        CloseMenus();
        LoadingText.text = "Creating Room...";
        LoadingScreen.SetActive(true);
    }

    /// <summary>
    /// Close Game window (only builds)
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion
}
