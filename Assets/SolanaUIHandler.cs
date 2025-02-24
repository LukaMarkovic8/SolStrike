using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SolanaUIHandler : MonoBehaviour
{
    public TextMeshProUGUI TextMeshProUGUI;
    public TextMeshProUGUI balance;

    public GameObject walletElementPref;
    public GameObject walletElementHolder;
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
        TextMeshProUGUI.text = account.PublicKey;
        GameObject gameObject =  Instantiate(walletElementPref,walletElementHolder.transform);
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = account.PublicKey;
    }

    public void OnBalanceChange(double amount)
    {
        balance.text = amount.ToString();
    }






}
