using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Solana.Unity.Wallet;

public class DataManager : MonoBehaviour
{
    // Singleton Instance
    private static DataManager _instance;
    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject();
                _instance = singletonObject.AddComponent<DataManager>();
                singletonObject.name = "DataManager";
                DontDestroyOnLoad(singletonObject); // Persist across scene loads
            }
            return _instance;
        }
    }

    // Data Storage (Example: Dictionary to store parsed JSON data)
    private Dictionary<string, object> _storedData = new Dictionary<string, object>();
    public Dictionary<string, object> StoredData => _storedData; // Public getter

    // Optional: Event to notify when data is updated
    public delegate void DataUpdatedDelegate(string key);
    public event DataUpdatedDelegate OnDataUpdated;

    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region HTTP Calls


    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
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

    public IEnumerator PostRequest(string url, string bodyJsonString, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJsonString);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for the response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"POST request failed for URL: {url} - Error: {webRequest.error}");
                onError?.Invoke(webRequest.error);
            }
        }
    }

    #endregion

    #region Data Parsing (Example: JSON)

    public T ParseJson<T>(string jsonString) where T : class
    {
        try
        {
            return JsonUtility.FromJson<T>(jsonString);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing JSON: {e.Message}");
            return null;
        }
    }

    public void StoreParsedData(string key, object data)
    {
        if (_storedData.ContainsKey(key))
        {
            _storedData[key] = data;
        }
        else
        {
            _storedData.Add(key, data);
        }
        OnDataUpdated?.Invoke(key); // Notify listeners that data has been updated
    }

    public T GetStoredData<T>(string key)
    {
        if (_storedData.ContainsKey(key) && _storedData[key] is T)
        {
            return (T)_storedData[key];
        }
        Debug.LogWarning($"Data with key '{key}' not found or is of the wrong type.");
        return default(T);
    }

    #endregion

    // Example Usage (can be called from other scripts)
}


public static class Signature
{
    public static PublicKey TokenProgram22 = new PublicKey("TokenzQdBNbLqP5VEhdkAS6EPFLC1PHnBqCXEpPxuEb");
    public static string baseUrl = "https://api.solstrike.xyz/api/";
    public static string SignatureString = "signature";
    public static byte[] SignitureBytes = new byte[0];
    public static string UUid = "signature";
    public static byte[] UUidBytes = new byte[0];
    public static string SignedMessage = "signature";
    public static string Poruka = "Sign this message to log in. It is free and will not trigger any blockchain transaction.";
    public static string PublicKey = "pk";
    public static GamerData GamerData = new GamerData();
    public static int UnclaimedChipsAmount = 0;
    public static int StandardChipsAmount = 0;
    public static bool HasReservedChips = false;
    public static PlayerData[] LeaderBoardPlayers = new PlayerData[0];

    public static string marker = "-account:";


    public static byte[] PublicKeyBytes { get; internal set; }
    public static double SolanaBalance { get; internal set; }

    public static (string, string) SplitStringByAccount(string input, out string beforeAccount, out string afterAccount)
    {
        beforeAccount = null;
        afterAccount = null;


        int index = input.IndexOf(marker);

        if (index != -1)
        {
            beforeAccount = input.Substring(0, index);
            afterAccount = input.Substring(index + marker.Length);
            return new(beforeAccount, afterAccount);
        }
        else
        {
            return (null, null);
            // Handle the case where the marker is not found, perhaps by setting both to the original string or empty
            // For this specific request, we are setting them to null as initialized.
        }
    }
    public static string GetJustUsername(string input)
    {


        string beforeAccount;
  
        int index = input.IndexOf(marker);

        if (index != -1)
        {
            beforeAccount = input.Substring(0, index);
           
            return beforeAccount;
        }
        else
        {
            return null;
            // Handle the case where the marker is not found, perhaps by setting both to the original string or empty
            // For this specific request, we are setting them to null as initialized.
        }
    }
}