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
    public float RespawnWaitTime = 5f;

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
    public void Die(string killer)
    {
        UIController.instance.DeathText.text = "You were killed by : " + killer;

        if(player != null)
        {
            StartCoroutine(DieCoroutine());
        }
    }

    /// <summary>
    /// Respawn player after seconds
    /// </summary>
    /// <returns></returns>
    public IEnumerator DieCoroutine()
    {
        PhotonNetwork.Instantiate(DeathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIController.instance.DeathScreen.SetActive(true);
        yield return new WaitForSeconds(RespawnWaitTime);
        UIController.instance.DeathScreen.SetActive(false);
        SpawnPlayer();
    }
    #endregion
}
