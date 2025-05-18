using System;
using System.Collections;
using System.Collections.Generic;
using Solana.Unity.SDK;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class HomeScreenController : MonoBehaviour
{


    //public GameObject reserveChipsHolder;


    public GameObject PlayButtonHolder;
    public GameObject ReserveButtonHolder;
    public GameObject BuyButtonHolder;
    public GameObject ActiveGameHolder;

    private float userDataFetchTimer = 0f;
    public float userDataFetchInterval = 10f; // 10 seconds
    private void Update()
    {
        if (Signature.GamerData != null)
        {
            int reserved = Convert.ToInt32(Signature.GamerData.reservedChips);
            double standard = Convert.ToDouble(Signature.StandardChipsAmount);

            if (Signature.GamerData.isInActiveGame)
            {
                PlayButtonHolder.SetActive(false);
                ReserveButtonHolder.SetActive(false);
                ActiveGameHolder.SetActive(true);
                BuyButtonHolder.SetActive(false);
            }
            else if (reserved > 0)
            {
                ActiveGameHolder.SetActive(false);
                PlayButtonHolder.SetActive(true);
                ReserveButtonHolder.SetActive(false);
                BuyButtonHolder.SetActive(false);
            }
            else if (reserved < 1 && standard >= 1)
            {
                ActiveGameHolder.SetActive(false);
                ReserveButtonHolder.SetActive(true);
                PlayButtonHolder.SetActive(false);
                BuyButtonHolder.SetActive(false);
            }
            else if (reserved < 1 && standard < 1)
            {
                ActiveGameHolder.SetActive(false);
                ReserveButtonHolder.SetActive(false);
                PlayButtonHolder.SetActive(false);
                BuyButtonHolder.SetActive(true);
            }


            if (Signature.GamerData.isInActiveGame)
            {
                userDataFetchTimer += Time.deltaTime;
            }
            else
            {
                if (userDataFetchTimer > 0)
                {
                    userDataFetchTimer = 0f; // Reset the timer if not in an active game

                }
            }


            if (Signature.GamerData != null && Signature.GamerData.isInActiveGame && userDataFetchTimer >= userDataFetchInterval)
            {
                StartCoroutine(GetUserData());
                userDataFetchTimer = 0f; // Reset the timer
            }
        }

    }


    IEnumerator GetUserData()
    {
        string url = Signature.baseUrl + Web3.Account.PublicKey.Key;
        //Debug.Log("Sending GET request to: " + url);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("accept", "application/json");

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"Error: {webRequest.error}\nURL: {url}");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"HTTP Error: {webRequest.error}\nCode: {webRequest.responseCode}\nURL: {url}");
                    break;
                case UnityWebRequest.Result.Success:
                    //  Debug.Log($"Success! Response Code: {webRequest.responseCode}");
                    string responseJson = webRequest.downloadHandler.text;
                    //    Debug.Log("Received JSON:\n" + responseJson);

                    try
                    {
                        GamerData gamerData = JsonUtility.FromJson<GamerData>(responseJson);
                        Signature.GamerData = gamerData;

                        // TODO: Parse responseJson here
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Debug.LogError("Error parsing JSON: " + ex.Message);
                        break;
                    }
            }
        }
    }


    private IEnumerator GetLeaderboardCoroutine()
    {
        string url = Signature.baseUrl + "leaderboard/" + Web3.Account.PublicKey.Key;
        Debug.Log("Sending GET  leaderboard request to: " + url);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("accept", "application/json");

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"Error: {webRequest.error}\nURL: {url}");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"HTTP Error: {webRequest.error}\nCode: {webRequest.responseCode}\nURL: {url}");
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log($"Success! Response Code: {webRequest.responseCode}");
                    string responseJson = webRequest.downloadHandler.text;
                    Debug.Log("Received JSON:\n" + responseJson);

                    PlayerListWrapper playerListWrapper = JsonUtility.FromJson<PlayerListWrapper>(responseJson);

                    // Now you can access the array of player data
                    PlayerData[] players = playerListWrapper.players;

                    // Example of how to access the parsed data
                    if (players != null)
                    {
                        Debug.Log("Parsed " + players.Length + " players.");
                        foreach (var player in players)
                        {
                            Debug.Log($"Account ID: {player.accountId}, Username: {player.username}, Points: {player.points}, Place: {player.place}");
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to parse player data.");
                    }

                    // TODO: Parse responseJson here
                    break;
            }
        }
    }
}
[System.Serializable] // This attribute is required for JsonUtility to work
public class PlayerData
{
    public string accountId;
    public string username;
    public string points; // JSON provides points as a string, keep as string or parse to int if needed
    public int partyCount;
    public int kills;
    public int deaths;
    public int place;

    // Optional: You can add helper methods here
    public int GetPointsAsInt()
    {
        if (int.TryParse(points, out int result))
        {
            return result;
        }
        return 0; // Or handle error appropriately
    }
}

// This is a helper class to wrap the JSON array for JsonUtility
[System.Serializable]
public class PlayerListWrapper
{
    public PlayerData[] players; // The name 'players' must match the key we'll use in the modified JSON
}