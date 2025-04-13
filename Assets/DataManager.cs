using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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


public static class Siginiture
{
    public static string SignatureString = "signature";
    public static byte[] SignitureBytes = new byte[0];
    public static string UUid = "signature";
    public static byte[] UUidBytes = new byte[0];

    public static string PublicKey = "pk";

    public static byte[] PublicKeyBytes { get; internal set; }
}