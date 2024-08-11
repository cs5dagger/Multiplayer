using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    #region Instance creation
    public static PlayerSpawner instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region Public Variables
    public GameObject PlayerPrefab;
    public GameObject DeathEffect;

    #endregion

    #region Private Variables
    private GameObject player;

    #endregion

    #region Methods and Overrides
    private void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }    
    }

    /// <summary>
    /// Instantiate new player on network
    /// </summary>
    public void SpawnPlayer()
    {
        /// Get new spawn point
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        
        player = PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    /// <summary>
    /// Actions to perform on player death
    /// </summary>
    public void Die()
    {
        PhotonNetwork.Instantiate(DeathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        SpawnPlayer();
    }
    #endregion
}
