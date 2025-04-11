using UnityEngine;
using TMPro;
using Solana.Unity.SDK;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using Solana.Unity.Soar.Accounts;

public class bl_CountDown : bl_CountDownBase
{
    public GameObject Content;
    public TextMeshProUGUI CountDownText;
    public AudioClip CountAudio;

    private Animator CountAnim;
    private AudioSource ASource;
    private int countDown = 5;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_PhotonNetwork.AddNetworkCallback(PropertiesKeys.CountdownEvent, OnNetworkEvent);

        //TODO SEND START GAME;
        accountId = Web3.Wallet.Account.PublicKey.Key;
        gameId = bl_PhotonNetwork.CurrentRoom.Name;
    }
    public string baseUrl = "YOUR_API_BASE_URL_HERE";

    // Example data - replace these with your actual dynamic data
    public string accountId = "user123";
    public string gameId = "game456";
    public string signature = "example_signature";
    public string signedMessage = "example_signed_message";
    public string network = "mainnet"; // Or whatever network applies

    // --- Data Structure for the Request Body ---
    // Must match the JSON structure exactly
    // [System.Serializable] is needed for JsonUtility to work
    [System.Serializable]
    public class PlayRequestData
    {
        public string accountId;
        public string gameId;
        public string signature;
        public string signedMessage;
        public string network;
    }

    // --- Public Method to Trigger the Request ---
    // You can call this method from a UI Button or another script
    public void SendPlayRequest()
    {
        // Create the data object
        PlayRequestData data = new PlayRequestData
        {
            accountId = this.accountId,
            gameId = this.gameId,
            signature = this.signature,
            signedMessage = this.signedMessage,
            network = this.network
        };

        // Start the Coroutine to handle the web request
        StartCoroutine(PostRequestCoroutine(baseUrl + "/api/games/play", data));
    }

    // --- Coroutine to Handle the Asynchronous POST Request ---
    private IEnumerator PostRequestCoroutine(string url, PlayRequestData data)
    {
        // 1. Serialize the C# object into a JSON string
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log("Sending JSON: " + jsonData);

        // 2. Convert the JSON string to a byte array using UTF8 encoding
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);

        // 3. Create the UnityWebRequest object
        // Use the constructor for more control, especially for setting upload handler
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // 4. Set the Upload Handler to send the raw JSON data
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);

            // 5. Set the Download Handler to receive the response
            request.downloadHandler = new DownloadHandlerBuffer();

            // 6. Set the Content-Type header to application/json
            // This is crucial for the server to understand the request body format
            request.SetRequestHeader("Content-Type", "application/json");
            // Add any other required headers here (e.g., Authorization)
            // request.SetRequestHeader("Authorization", "Bearer YOUR_TOKEN_HERE");

            Debug.Log($"Sending POST request to: {url}");

            // 7. Send the request and wait for the response
            yield return request.SendWebRequest();

            // 8. Check for errors
            // Use request.result for Unity 2020.1+
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"Error: {request.error}");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    // ProtocolError means the server responded with an error code (4xx or 5xx)
                    Debug.LogError($"HTTP Error: {request.responseCode} - {request.error}");
                    Debug.LogError($"Response Body: {request.downloadHandler.text}"); // Log server error message if any
                    break;
                case UnityWebRequest.Result.Success:
                    // Request was successful (response code 2xx, including 201)
                    Debug.Log($"Success! Response Code: {request.responseCode}");
                    Debug.Log($"Response Body: {request.downloadHandler.text}");
                    // Optionally, check for the specific 201 code if needed:
                    if (request.responseCode == 201)
                    {
                        Debug.Log("Server confirmed resource creation (201).");
                        // Handle successful response data here
                        // Example: Parse request.downloadHandler.text if it contains JSON
                    }
                    break;
            }
        } // The 'using' statement ensures request.Dispose() is called automatically
    }




    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_PhotonNetwork.RemoveNetworkCallback(OnNetworkEvent);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnNetworkEvent(ExitGames.Client.Photon.Hashtable data)
    {
        OnReceiveCount((int)data["c"]);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void StartCountDown(bool overrideIfStarted = false)
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;
        if (IsCounting && !overrideIfStarted)
        {
            Debug.Log($"Countdown has already started.");
            return;
        }


        countDown = bl_GameData.Instance.CountDownTime;
        bl_MatchTimeManagerBase.Instance.SetTimeState(RoomTimeState.Countdown, true);
        bl_GameManager.Instance.SetGameState(MatchState.Starting);
        IsCounting = true;

        InvokeRepeating(nameof(SetCountDown), 1, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public override void SetCount(int count)
    {
        OnCountChanged(count);
    }

    /// <summary>
    /// 
    /// </summary>
    void SetCountDown()
    {
        countDown--;
        if (countDown <= 0)
        {
            CancelInvoke(nameof(SetCountDown));
            IsCounting = false;
        }

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("c", countDown);
        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.CountdownEvent, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    void OnReceiveCount(int count)
    {
        if (!bl_PhotonNetwork.IsMasterClient)
        {
            countDown = count;
        }
        if (countDown <= 0)
        {
            CancelInvoke(nameof(SetCountDown));
            bl_MatchTimeManagerBase.Instance.InitAfterCountdown();
        }
        else
        {
            bl_MatchTimeManagerBase.Instance.SetTimeState(RoomTimeState.Countdown);
        }
        OnCountChanged(countDown);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    private void OnCountChanged(int count)
    {
        if (CountAudio != null)
        {
            if (ASource == null) { ASource = GetComponent<AudioSource>(); }
            ASource.clip = CountAudio;
            ASource.Play();
        }

        CountDownText.text = count.ToString();
        if (count > 0)
        {
            Content.SetActive(true);

            CountAnim = Content.GetComponent<Animator>();
            CountAnim.Play("count", 0, 0);
        }
        else
        {
            Content.SetActive(false);
        }
    }
}