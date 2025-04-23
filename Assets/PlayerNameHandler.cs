using Solana.Unity.SDK;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNameHandler : MonoBehaviour
{


    private void OnEnable()
    {
    }
    [ContextMenu ("TEST DATA")]
    public void GetGamerData2() => StartCoroutine(GetGamerDataCoroutine2());


    public void GetGamerData() => StartCoroutine(GetGamerDataCoroutine());

    private IEnumerator GetGamerDataCoroutine()
    {
        string url = Signature.baseUrl + Web3.Account.PublicKey.Key;
        Debug.Log("Sending GET request to: " + url);

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

                  // StartCoroutine(SendPutRequest(url));
                    // TODO: Parse responseJson here
                    break;
            }
        }
    }

    IEnumerator SendPutRequest(string url)
    {
        string jsonData = "{\"username\": \"Luka\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest www = UnityWebRequest.Put(url, bodyRaw))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error sending PUT: {www.responseCode} - {www.error}");
                if (www.downloadHandler != null) Debug.LogError($"Response: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"PUT successful: {www.responseCode}");
                if (www.downloadHandler != null) Debug.Log($"Response: {www.downloadHandler.text}");
            }
        }
    }
    private IEnumerator GetGamerDataCoroutine2()
    {
        string url = a + "9wcLmTeeuMKE2phTJDWT9MwqKju2ooaqQodDn9GX7RNX";
     


        Debug.Log("Sending GET request to: " + url);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            //webRequest.SetRequestHeader("Accept", "application/json");

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

                    StartCoroutine(SendPutRequest(url));
                    // TODO: Parse responseJson here
                    break;
            }
        }
    }
    private void GetPlayerName()
    {

    }

    string a = "api.solstrike.xyz/gamers/leaderboard/";
    private void SetPlayerName()
    {

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
