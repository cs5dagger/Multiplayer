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
    public static UIController instance;

    void Awake()
    {
        instance = this;
    }

    public TMP_Text OverheatedMessage;
    public Slider WeaponTemperatureSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
