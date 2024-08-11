using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    #region Instance
    public static MatchManager instance;
    private void Awake()
    {
        instance = this;
    }
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

    #endregion
}
