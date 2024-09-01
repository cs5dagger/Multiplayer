using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderBoardPlayer : MonoBehaviour
{
    public TMP_Text PlayerNameText;
    public TMP_Text Kills;
    public TMP_Text Deaths;

    public void SetDetails(string name, int kills, int deaths)
    {
        PlayerNameText.text = name;
        Kills.text = kills.ToString();
        Deaths.text = deaths.ToString();
    }
}
