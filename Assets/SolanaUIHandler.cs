using Solana.Unity.Programs.Utilities;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;

public class SolanaUIHandler : MonoBehaviour
{
    public TextMeshProUGUI TextMeshProUGUI;
    public TextMeshProUGUI balance;

    public GameObject walletElementPref;
    public GameObject walletElementHolder;
    public GameObject closeButton;
    public PlayerNameHandler playerNameHandler;


    private string EndpointName = "https://api.deotoken.com/api/gamers/";


    public class BuyChipWithSolAccounts
    {
        public PublicKey Buyer { get; set; }

        public PublicKey GlobalConfig { get; set; }

        public PublicKey Treasury { get; set; }

        public PublicKey ChipMint { get; set; }

        public PublicKey BuyerChipAccount { get; set; }

        public PublicKey TokenProgram { get; set; }

        public PublicKey AssociatedTokenProgram { get; set; } = new PublicKey("ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL");
        public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
    }
    public const string ID = "3FFYCYGMqkjjpxMvGXu5XiRnZQtGJMN9r73Hh1yiBVjH";
    public static Solana.Unity.Rpc.Models.TransactionInstruction BuyChipWithSol(BuyChipWithSolAccounts accounts, ulong amount, PublicKey programId = null)
    {
        programId ??= new(ID);
        List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Buyer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.GlobalConfig, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Treasury, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.ChipMint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BuyerChipAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.AssociatedTokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
        byte[] _data = new byte[1200];
        int offset = 0;
        _data.WriteU64(9180956995626968905UL, offset);
        offset += 8;
        _data.WriteU64(amount, offset);
        offset += 8;
        byte[] resultData = new byte[offset];
        Array.Copy(_data, resultData, offset);
        return new Solana.Unity.Rpc.Models.TransactionInstruction { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
    }
    private void OnEnable()
    {
#if !UNITY_EDITOR
        if(!String.IsNullOrEmpty(Web3.Account.PublicKey.Key))
        {
            gameObject.SetActive(false);
        }
        else
        {
            DoLogin();
         
        }
#else
        gameObject.SetActive(false);
#endif
    }

    private void DoLogin() 
    {
        Web3.Instance.LoginWalletAdapter();
        Web3.OnLogin += OnLogin;
        Web3.OnBalanceChange += OnBalanceChange;
    }
    private void OnDisable()
    {
        Web3.OnLogin -= OnLogin;
        Web3.OnBalanceChange -= OnBalanceChange;
    }

    public void OnLogin(Account account)
    {
        gameObject.SetActive(false);
        TextMeshProUGUI.text = "public key: "+account.PublicKey;
        //GameObject gameObject =  Instantiate(walletElementPref,walletElementHolder.transform);
      //  gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "public key: "+account.PublicKey;
        closeButton.SetActive(true);
       // StartCoroutine(GetRequest(EndpointName + Web3.Account.PublicKey));
       // account.PublicKey.FindProgramAdress
       // bl_Lobby.Instance.ChangeWindow("server");
       playerNameHandler.GetGamerData();
    }

    public void OnBalanceChange(double amount)
    {
        balance.text = "SOL:"+ amount.ToString();
    }


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



}
