using Solana.Unity.SDK;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNameHandler : MonoBehaviour
{

    private string baseUrl = "https://api.deotoken.com/api/gamers/";

    private void OnEnable()
    {
    }
    [ContextMenu ("TEST DATA")]
    public void GetGamerData2() => StartCoroutine(GetGamerDataCoroutine2());


    public void GetGamerData() => StartCoroutine(GetGamerDataCoroutine());

    private IEnumerator GetGamerDataCoroutine()
    {
        string url = baseUrl + Web3.Account.PublicKey.Key;
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
                    // TODO: Parse responseJson here
                    break;
            }
        }
    }


    private IEnumerator GetGamerDataCoroutine2()
    {
        string url = baseUrl + "9wcLmTeeuMKE2phTJDWT9MwqKju2ooaqQodDn9GX7RNX";
        //string url = "https://api.deotoken.com/api/gamers/9wcLmTeeuMKE2phTJDWT9MwqKju2ooaqQodDn9GX7RNX";


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
                    // TODO: Parse responseJson here
                    break;
            }
        }
    }
    private void GetPlayerName()
    {

    }


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
