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
using Cysharp.Threading.Tasks.Triggers;


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
        byte[] response = await Web3.Wallet.SignMessage(bytes);
        string result = System.Text.Encoding.UTF8.GetString(response);
        Debug.Log("result: " + result);
        Siginiture.SignedMessage = myuuidAsString;
        Siginiture.SignatureString = result;
        result = Encoding.ASCII.GetString(response);
        Debug.Log("ASCII: " + result);

        //   Try UTF-16 Big Endian
        string result1 = Encoding.BigEndianUnicode.GetString(response);
        Debug.Log("UTF-16 BE: " + result1);
        // Try ISO-8859 - 1(Latin - 1)
        string result2 = Encoding.GetEncoding("ISO-8859-1").GetString(response);
        Debug.Log("ISO-8859-1: " + result2);
        //   Try UTF-16 Big Endian
        string result5 = Encoding.BigEndianUnicode.GetString(response);
        Debug.Log("UTF-16 BE: " + result5);

        //   Try Windows-1252
        /*    string result3 = Encoding.GetEncoding(1252).GetString(response);
            Debug.Log("Windows-1252: " + result3);*/

        //  Try UTF-16 Little Endian(common)
        string result4 = Encoding.Unicode.GetString(response); // Same as UTF-16LE
        Debug.Log("UTF-16 LE: " + result4);
       // List<SignaturePubKeyPair> a = Web3.Base.DeduplicateTransactionSignatures(response,false);
        bool verified = Web3.Account.Verify(bytes, response);
        Debug.Log(Siginiture.PublicKey + " Message signed     SignedMessage:" + Siginiture.SignedMessage + "   SignatureString:" + Siginiture.SignatureString);
        Debug.Log(response.ToString());
        Debug.Log(verified.ToString());
        gameObject.SetActive(false);

    }


    public void OnBalanceChange(double amount)
    {
        //  balance.text = "SOL:" + amount.ToString();
    }




}
