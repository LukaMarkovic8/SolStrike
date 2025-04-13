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
using Org.BouncyCastle.Asn1.Ocsp;


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
        SignMessageAsync();
    }

    public async Task SignMessageAsync()
    {
        // Example of signing a message
        Guid myuuid = Guid.NewGuid();
        string myuuidAsString = myuuid.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(myuuidAsString);
        byte[] bytessiginture;
        await Web3.Base.SignMessage(bytes);
        Siginiture.UUid = myuuidAsString;
        Siginiture.SignatureString = Web3.Base.SignatureString;
        Debug.Log("Message signed     Siginiture.UUid:" + Siginiture.UUid + "    Siginiture.SignatureString:" + Siginiture.SignatureString);
        gameObject.SetActive(false);
   

    }

    public void OnBalanceChange(double amount)
    {
        //  balance.text = "SOL:" + amount.ToString();
    }




}
