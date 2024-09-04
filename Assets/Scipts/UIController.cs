using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

/// <summary>
/// Singleton
/// Cannot attach to player because player is killed and respawned which caues the null reference,
/// that's why create a singleton
/// </summary>

public class UIController : MonoBehaviour
{
    #region Instance
    
    public static UIController instance;

    void Awake()
    {
        instance = this;
    }

    #endregion

    #region Public Variables
    
    public TMP_Text OverheatedMessage;
    public Slider WeaponTemperatureSlider;
    public GameObject DeathScreen;
    public TMP_Text DeathText;
    public Slider HealthSlider;
    public TMP_Text KillsText;
    public TMP_Text DeathsLabel;
    public GameObject LeaderBoard;
    public LeaderBoardPlayer leaderBoardPlayerDisplay;
    public GameObject EndScreen;
    public GameObject OptionsScreen;

    #endregion

    #region Methods and Overrides

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }
    }

    /// <summary>
    /// Show or hide pause screen
    /// </summary>
    public void ShowHideOptions()
    {
        if(!OptionsScreen.activeInHierarchy)
        {
            OptionsScreen.SetActive(true);
            if(Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            OptionsScreen.SetActive(false);
        }
    }

    /// <summary>
    /// close current game and return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
}
