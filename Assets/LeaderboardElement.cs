using System;
using System.Collections;
using System.Collections.Generic;
using Solana.Unity.SDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardElement : MonoBehaviour
{
    public Sprite sprite;

    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI deathsText;
    public TextMeshProUGUI gamesPlayedText;
    public TextMeshProUGUI placeText;
    internal void SetData(PlayerData player)
    {
        usernameText.text = player.username;
        pointsText.text = player.points.ToString();
        killsText.text = player.kills.ToString();
        deathsText.text = player.deaths.ToString();
        placeText.text = player.place.ToString();
        gamesPlayedText.text = player.partyCount.ToString();

        if(player.accountId == Web3.Account.PublicKey.Key)
        {
            gameObject.GetComponent<Image>().sprite = sprite;
        }
    }

}
