using Solana.Unity.Programs;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc;
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
using static Solana.Unity.SDK.Web3;
using System.Threading.Tasks;

public class SolanaUIHandler : MonoBehaviour
{
    public TextMeshProUGUI TextMeshProUGUI;
    public TextMeshProUGUI balance;

    public GameObject walletElementPref;
    public GameObject walletElementHolder;
    public GameObject closeButton;
    public PlayerNameHandler playerNameHandler;


    private string EndpointName = "https://api.deotoken.com/api/gamers/";

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
        //Debug.Log("Public Key: " + account.PublicKey);
        Siginiture.PublicKey = account.PublicKey.Key;
        TextMeshProUGUI.text = "SIGN MESSAGE";
        byte[] bytes = Encoding.UTF8.GetBytes(Siginiture.PublicKey);
        Siginiture.PublicKeyBytes = bytes;
        SignMessage();
    }

    public async Task SignMessage()
    {
        // Example of signing a message
        Guid myuuid = Guid.NewGuid();
        string myuuidAsString = myuuid.ToString();
        Debug.Log("UUID: " + myuuidAsString);
        byte[] bytes = Encoding.UTF8.GetBytes(myuuidAsString);
        await Web3.Base.SignMessage(bytes);
        Debug.Log("Message signed");
        Siginiture.SignatureString = myuuidAsString;
        gameObject.SetActive(false);

    }
    public void OnBalanceChange(double amount)
    {
      //  balance.text = "SOL:" + amount.ToString();
    }




}
