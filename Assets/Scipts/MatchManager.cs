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
        ChangeStat
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
    }

    private void Update()
    {

    }

    public void OnEvent(EventData photonEvent)
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
