using UnityEngine;
using TMPro;
using Solana.Unity.Soar.Accounts;
using Photon.Realtime;
using static bl_CountDown;
using System.Text;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

namespace MFPS.Runtime.UI.Layout
{
    public class bl_RoundFinishScreen : bl_RoundFinishScreenBase
    {
        public GameObject content;
        [SerializeField] private TextMeshProUGUI FinalUIText = null;
        [SerializeField] private TextMeshProUGUI FinalCountText = null;
        [SerializeField] private TextMeshProUGUI FinalWinnerText = null;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        public override void SetCountdown(int count)
        {
            count = Mathf.Clamp(count, 0, int.MaxValue);
            FinalCountText.text = count.ToString();
        }

        /// <summary>
        /// Show the final round UI
        /// </summary>
        public override void Show(string winner)
        {
            Photon.Realtime.Player[] players = bl_PhotonNetwork.PlayerList;
            PlayerResult[] playerResults = new PlayerResult[players.Length];

            for (int i = 0; i < players.Length; i++)
            {
                playerResults[i] = new PlayerResult();
                playerResults[i].accountId = Signature.SplitStringByAccount(players[i].NickName, out string bf, out string after).Item2;
                playerResults[i].kills = players[i].GetKills();
                playerResults[i].deaths = players[i].GetDeaths();
                playerResults[i].headshots = 0;


                Debug.Log($"Player {i}: {players[i].NickName} - Account ID: {playerResults[i].accountId}");

            }
            GameOverData data = new GameOverData();
            data.accountId = Signature.PublicKey;
            data.gameId = bl_PhotonNetwork.CurrentRoom.Name;
            data.signature = Signature.SignatureString;
            data.signedMessage = Signature.SignedMessage;
            data.playerResults = playerResults;
            Debug.Log(data.playerResults.Length+"  "+ data.playerResults[0].accountId);
            if (data.playerResults.Length < 1)
            {
                Debug.LogError("No player results found.");
                return;
            }
            else
            {
                Debug.Log("Player results found."+ data.playerResults[0].accountId);
            }
                StartCoroutine(PostRequestCoroutine(Signature.baseUrl + "games/over", data));
            content.SetActive(true);
            FinalUIText.text = (bl_RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht) ? bl_GameTexts.FinalOneMatch.Localized(38) : bl_GameTexts.FinalRounds.Localized(32);
            FinalWinnerText.text = string.Format("{0} {1}", Signature.GetJustUsername(winner), bl_GameTexts.FinalWinner).Localized(33).ToUpper();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {

            content.SetActive(false);
        }


        // --- Coroutine to Handle the Asynchronous POST Request ---
        private IEnumerator PostRequestCoroutine(string url, GameOverData data)
        {
            // 1. Serialize the C# object into a JSON string
            string jsonData = JsonConvert.SerializeObject(data);
            Debug.Log("Sending gamne over JSON: " + jsonData);


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
            }
        }













    }




}