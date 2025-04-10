using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

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

    public IEnumerator GetRequests(string url)//, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the request and wait for the response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
             //   onComplete?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"GET request failed for URL: {url} - Error: {webRequest.error}");
               // onError?.Invoke(webRequest.error);
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

}