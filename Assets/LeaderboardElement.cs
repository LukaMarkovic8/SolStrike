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
    // public TextMeshProUGUI gamesPlayedText;
    public TextMeshProUGUI placeText;
    internal void SetData(PlayerData player)
    {
        (string, string) ca = Signature.SplitStringByAccount(player.username, out string beforeAccount, out string afterAccount);

        string beforeAccountString = ca.Item1;


        if (player.username.Contains(Signature.marker))
        {
            usernameText.text = beforeAccountString;
        }
        else
        {
            usernameText.text = player.username;
        }

        string inputString = player.points;

        // Parse the string into a decimal
        decimal originalDecimal = decimal.Parse(inputString);

        // Round the decimal to 2 decimal places
        decimal roundedDecimal = Math.Round(originalDecimal, 2);

        pointsText.text = roundedDecimal.ToString();

        killsText.text = player.kills.ToString();
        deathsText.text = player.deaths.ToString();
        placeText.text = player.place.ToString();
        //   gamesPlayedText.text = player.partyCount.ToString();

        if (player.accountId == Web3.Account.PublicKey.Key)
        {
            gameObject.GetComponent<Image>().sprite = sprite;
        }
    }

}
