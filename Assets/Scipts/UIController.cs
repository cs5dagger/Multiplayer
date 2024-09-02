using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    
    #endregion

    #region Methods and Overrides

    #endregion
}
