using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SolanaUIHandler : MonoBehaviour
{
    public TextMeshProUGUI TextMeshProUGUI;
    public TextMeshProUGUI balance;

    public GameObject walletElementPref;
    public GameObject walletElementHolder;
    public GameObject closeButton;



    private string EndpointName = "https://api.deotoken.com/api/gamers/";




    private void OnEnable()
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
        TextMeshProUGUI.text = "public key: "+account.PublicKey;
        //GameObject gameObject =  Instantiate(walletElementPref,walletElementHolder.transform);
      //  gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "public key: "+account.PublicKey;
        closeButton.SetActive(true);
       // StartCoroutine(GetRequest(EndpointName + Web3.Account.PublicKey));
       // account.PublicKey.FindProgramAdress
       // bl_Lobby.Instance.ChangeWindow("server");
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





}
