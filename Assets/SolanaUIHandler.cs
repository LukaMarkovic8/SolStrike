using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using SolStrike;
using SolStrike.Accounts;
using SolStrike.Program;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Solana.Unity.SDK.Web3;



public class SolanaUIHandler : MonoBehaviour
{
    public GameObject waitingForTransactionHolder;

    public TextMeshProUGUI unclaimedChipsText;
    public TextMeshProUGUI chipsText;
    // public TextMeshProUGUI matchCostText;
    public TextMeshProUGUI Points;
    public TextMeshProUGUI solanaBalanceText;


    public TextMeshProUGUI TextMeshProUGUI;
    public TextMeshProUGUI balance;
    public GameObject holder;
    public GameObject walletElementPref;
    public GameObject walletElementHolder;
    public GameObject closeButton;
    private PublicKey TokenProgram22 = new PublicKey("TokenzQdBNbLqP5VEhdkAS6EPFLC1PHnBqCXEpPxuEb");
    public bl_Lobby bl_Lobby;

    void Start()
    {
        // Add a listener to the onValueChanged event
        buyChipsInputFieldBuyScreen.onValueChanged.AddListener(OnInputFieldValueChanged);
        redeemChipsInputField.onValueChanged.AddListener(OnRedeemInputFieldValueChanged);
        reserveChipsInputField.onValueChanged.AddListener(OnReserveInputfieldChanged);

        if (!Signature.isFirstTime)
        {
            LoadData();
        }
        else
        {
      //
        }
      


    }


    private void OnEnable()
    {
#if !UNITY_EDITOR
        if(!String.IsNullOrEmpty(Web3.Account.PublicKey.Key))
        {
            holder.SetActive(false);
        }
        else
        {
            DoLogin();
         
        }
#else
        bl_Lobby.GPN();
        holder.SetActive(false);
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
        Signature.PublicKey = account.PublicKey.Key;
        TextMeshProUGUI.text = "SIGN MESSAGE";
        byte[] bytes = Encoding.UTF8.GetBytes(Signature.PublicKey);
        Signature.PublicKeyBytes = bytes;
        SignMessageAsync();
      
    }


    public async Task LoadData()
    {
        GetGamerData();
        // StartCoroutine(GetLeaderboardCoroutine());
        await GetAmountOfChipsWeb3Async(true);
        await GetSolanaBalance();
        StartCoroutine(GetUnclaimedChipsAfterSeconds(5));
    }

    public async Task SignMessageAsync()
    {


        string signedMessageId = Signature.Poruka + " - ";
        DateTime utcNow = DateTime.UtcNow;
        DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        long timestampInSeconds = (long)(utcNow - unixEpoch).TotalSeconds;
        signedMessageId += timestampInSeconds.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(signedMessageId);
        byte[] response = await Web3.Wallet.SignMessage(bytes);
        string base58Result = Solana.Unity.Wallet.Utilities.Encoders.Base58.EncodeData(response);
        // Debug.Log("Base58 result: " + base58Result);
        Signature.SignedMessage = signedMessageId;
        Signature.SignatureString = base58Result;

        //Debug.Log(Signature.PublicKey + "   SignedMessage:" + Signature.SignedMessage + "   SignatureString:" + Signature.SignatureString);
        bool verified = Web3.Account.Verify(bytes, response);
        // Debug.Log("Verification result: " + verified);
        await GetAmountOfChipsWeb3Async();
        // gameObject.SetActive(false);
    }


    public void OnBalanceChange(double amount)
    {
        Signature.SolanaBalance = amount;
        solanaBalanceText.text = "SOL BALANCE : " + Signature.SolanaBalance.ToString();

        //    Debug.Log("sol balance: " + amount);
    }

    [ContextMenu("TESTGET")]
    public void Test()
    {
        GetAmountOfChipsWeb3Async();
    }

    [ContextMenu("ReserveTest")]
    public void ReserveTest()
    {
        Reserve(1000000000 * (ulong)chipsToReserveAmount);
    }

    [ContextMenu("BuyTest")]
    public void BuyTest()
    {
        Buy((ulong)chipsToBuywithSOL * 1000000000);
    }
    [ContextMenu("SellTest")]
    public void SellTest()
    {
        Sell((ulong)chisToRedeem * 1000000000);
    }
    [ContextMenu("ClaimTest")]
    public void ClaimTest()
    {
        Claim();
    }
    [ContextMenu("GetGamerData")]
    public void GetGamerData() => StartCoroutine(GetGamerDataCoroutine());
    private string baseUrl = "https://api.solstrike.xyz/api/gamers/";
    public GameObject lederboardElementsHolder;
    public GameObject leaderboadElementPref;
    public GameObject leaderboadHeaderPref;


    private IEnumerator GetGamerDataCoroutine()
    {
        string url = baseUrl + Web3.Account.PublicKey.Key;
        //Debug.Log("Sending GET request to: " + url);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("accept", "application/json");

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"Error: {webRequest.error}\nURL: {url}");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"HTTP Error: {webRequest.error}\nCode: {webRequest.responseCode}\nURL: {url}");
                    break;
                case UnityWebRequest.Result.Success:
                    //  Debug.Log($"Success! Response Code: {webRequest.responseCode}");
                    string responseJson = webRequest.downloadHandler.text;
                   //    Debug.Log("Received JSON:\n" + responseJson);

                    try
                    {
                        GamerData gamerData = JsonUtility.FromJson<GamerData>(responseJson);
                        Signature.GamerData = gamerData;

                        // TODO: Parse responseJson here
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Debug.LogError("Error parsing JSON: " + ex.Message);
                        break;
                    }
            }
        }

    }
    public async Task GetSolanaBalance()
    {
        //  Debug.Log("Solana balance before: " + Signature.SolanaBalance);

        Signature.SolanaBalance = await Web3.Wallet.GetBalance();
        //   Debug.Log("Solana balance after: " + Signature.SolanaBalance);
    }
    public async Task GetAmountOfChipsWeb3Async(bool a = true)
    {


        PublicKey chipMint;
        PublicKey publicKeyProgram = new PublicKey(SolStrike.Program.SolStrikeProgram.ID);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var bump2);
        var tokenABalance = await Web3.Rpc.GetTokenAccountBalanceAsync(Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, TokenProgram22), Commitment.Confirmed);
        //  Debug.Log("tokenABalance : " + tokenABalance.WasSuccessful.ToString());

        if (tokenABalance.WasSuccessful)
        {
            //     Debug.Log(tokenABalance.Result.Value.UiAmountString);
            Signature.StandardChipsAmount = tokenABalance.Result.Value.AmountDouble;
            chipsText.text = "CHIPS BALANCE : " + Signature.StandardChipsAmount.ToString();
            solanaBalanceText.text = "SOL BALANCE : " + Signature.SolanaBalance.ToString();
            //Debug.Log("tokenABalance : " + tokenABalance.Result.Value.Amount.ToString());
            // balance.text = "CHIPS:" + tokenABalance.Result.Value.Amount.ToString();
        }
        else
        {
            chipsText.text = "CHIPS BALANCE : " + Signature.StandardChipsAmount.ToString();
            Signature.StandardChipsAmount = 0;
            //     Debug.Log("Error: " + tokenABalance.Reason);
        }

        if (!a)
        {
            return;
        }
        bl_Lobby.GPN();
        await GetAmountOfUnclaimedChipsWeb3Async();

    }


    IEnumerator GetUnclaimedChipsAfterSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        GetAmountOfUnclaimedChipsWeb3Async();
    }

    public async Task GetAmountOfUnclaimedChipsWeb3Async()
    {
        PublicKey claimableRewardsPDA;
        //Debug.Log("GetAmountOfUnclaimedChipsWeb3Async");
        //TODO LUKA
        //        holder.SetActive(false);

        SolStrikeClient solStrikeClient = new SolStrikeClient(Web3.Rpc, Web3.Base.ActiveStreamingRpcClient, new PublicKey(SolStrike.Program.SolStrikeProgram.ID));
        PublicKey.TryFindProgramAddress(new[] { Web3.Base.Account.PublicKey.KeyBytes }, new PublicKey(SolStrike.Program.SolStrikeProgram.ID), out claimableRewardsPDA, out var bump2);

        try
        {
            Solana.Unity.Programs.Models.AccountResultWrapper<ClaimableRewards> a = await solStrikeClient.GetClaimableRewardsAsync(claimableRewardsPDA, Commitment.Confirmed);


            if (a.WasSuccessful == false)
            {
                Signature.UnclaimedChipsAmount = 0;
                unclaimedChipsText.text = "UNCLAIMED CHIPS : " + Signature.UnclaimedChipsAmount.ToString();
                // Debug.Log("Error: " + a.WasSuccessful);
                return;
            }
            else
            {

                ulong amountUlong = a.ParsedResult.Amount;
                double amountInDesiredUnit = (double)amountUlong / 1_000_000_000.0;
                string formattedAmount = amountInDesiredUnit.ToString("F0");
                //   Debug.Log(amountUlong);
                //   Debug.Log("f:" + amountInDesiredUnit);
                //   Debug.Log("Formatted Amount: " + formattedAmount);
                //   Debug.Log(a.WasSuccessful + "  UnclaimedChips:" + ((float)a.ParsedResult.Amount).ToString() + "     a.OriginalRequest: " + a.OriginalRequest);
                Signature.UnclaimedChipsAmount = amountInDesiredUnit;
                unclaimedChipsText.text = "UNCLAIMED CHIPS : " + Signature.UnclaimedChipsAmount.ToString();

            }
        }
        catch (Exception ex)
        {


            // Debug.LogError("ERR GetAmountOfUnclaimedChipsWeb3Async:" + ex.ToString());
            //Debug.Log("tokenABalance : " + tokenABalance.Result.Value.Amount.ToString());
            // balance.text = "CHIPS:" + tokenABalance.Result.Value.Amount.ToString();

        }

        StartCoroutine(GetLeaderboardCoroutine());

    }

    public async void Buy(ulong chipsAmount)
    {
        double oldValue = Signature.StandardChipsAmount;

        string ProgramId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey globalConfig;
        PublicKey treasury;
        PublicKey chipMint;


        PublicKey publicKeyProgram = new PublicKey(ProgramId);

        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("GLOBAL_CONFIG") }, publicKeyProgram, out globalConfig, out var bump1);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var bump2);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var bump3);

        BuyChipWithSolAccounts account = new BuyChipWithSolAccounts();
        account.Buyer = Web3.Account.PublicKey;
        account.GlobalConfig = globalConfig;
        account.Treasury = treasury;
        account.ChipMint = chipMint;
        account.BuyerChipAccount = Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, TokenProgram22);
        account.TokenProgram = TokenProgram22;
        account.AssociatedTokenProgram = AssociatedTokenAccountProgram.ProgramIdKey;
        account.SystemProgram = SystemProgram.ProgramIdKey;
        TransactionInstruction buyChipInstruction = SolStrike.Program.SolStrikeProgram.BuyChipWithSol(account, chipsAmount, publicKeyProgram);

        string blockHash = await Web3.Base.GetBlockHash(Commitment.Confirmed);



        Transaction transaction = new Transaction();
        transaction.FeePayer = Web3.Account.PublicKey;
        transaction.RecentBlockHash = blockHash;
        transaction.Signatures = new List<SignaturePubKeyPair>();
        transaction.Instructions = new List<TransactionInstruction>();
        transaction.Instructions.Add(buyChipInstruction);
        waitingForTransactionHolder.SetActive(true);
        Transaction signedTransaction = await Base.SignTransaction(transaction);

        RequestResult<string> signature = await Base.ActiveRpcClient.SendTransactionAsync(Convert.ToBase64String(signedTransaction.Serialize()), true, Commitment.Confirmed);


        if (signature.WasSuccessful)
        {
            StartCoroutine(waitForChipsToChangeAfterBuy(oldValue));

        }
        else
        {

            waitingForTransactionHolder.SetActive(false);
            buyChipsScreenHolder.SetActive(false);

        }
        //Debug.Log("signature: " + signature.Result);
        // GetSolanaBalance();
        // GetAmountOfChipsWeb3Async(true);

    }

    public async void Reserve(ulong vhipsAmount)
    {
        double oldValue = Signature.StandardChipsAmount;
        string programId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey treasury;
        PublicKey chipMint;
        PublicKey publicKeyProgram = new PublicKey(programId);

        // Derive program addresses for Treasury and Chip Mint
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var bump1);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var bump2);

        // Prepare accounts for the ReserveChips instruction
        ReserveChipsAccounts accounts = new ReserveChipsAccounts
        {
            Signer = Web3.Account.PublicKey,
            Treasury = treasury,
            ChipMint = chipMint,
            //TreasuryChipTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(treasury, chipMint),
            TreasuryChipTokenAccount = treasury.DeriveAssociatedTokenAccount(chipMint, TokenProgram22),
            UserChipAccount = Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, TokenProgram22),
            TokenProgram = TokenProgram22//TokenProgram.ProgramIdKey


        };
        /*   Debug.Log($"Signer: {accounts.Signer}");
           Debug.Log($"Treasury: {accounts.Treasury}");
           Debug.Log($"ChipMint: {accounts.ChipMint}");
           Debug.Log($"TreasuryChipTokenAccount: {accounts.TreasuryChipTokenAccount}");
           Debug.Log($"UserChipAccount: {accounts.UserChipAccount}");
           Debug.Log($"TokenProgram: {accounts.TokenProgram}");*/
        // Create the ReserveChips instruction
        TransactionInstruction reserveVhipsInstruction = SolStrike.Program.SolStrikeProgram.ReserveChips(accounts, vhipsAmount, publicKeyProgram);
        // Fetch the recent block hash
        waitingForTransactionHolder.SetActive(true);

        string blockHash = await Web3.Base.GetBlockHash(Commitment.Confirmed);
        // Create and sign the transaction
        Transaction transaction = new Transaction
        {
            FeePayer = Web3.Account.PublicKey,
            RecentBlockHash = blockHash,
            Signatures = new List<SignaturePubKeyPair>(),
            Instructions = new List<TransactionInstruction> { reserveVhipsInstruction }
        };

        Transaction signedTransaction = await Web3.Base.SignTransaction(transaction);

        // Send the transaction
        RequestResult<string> signature = await Web3.Base.ActiveRpcClient.SendTransactionAsync(
            Convert.ToBase64String(signedTransaction.Serialize()),
            true, Commitment.Confirmed);

        if (signature.WasSuccessful)
        {
            // Debug.Log($"Successfully reserved {vhipsAmount} vhips. Transaction signature: {signature.Result}");
            //TOO FAKEING 1 because game data changes are too slow
            // Signature.GamerData.reservedChips = 1.ToString();
            StartCoroutine(waitReservedChipsToChange(oldValue));
        }
        else
        {
            reserveChipsScreenHolder.SetActive(false);
            waitingForTransactionHolder.SetActive(false);
            //  Debug.LogError($"Failed to reserve vhips. Error: {signature.Reason}");
        }
        // GetGamerData();
    }


    IEnumerator waitReservedChipsToChange(double ChipsOldValue)
    {
        Signature.GamerData.reservedChips = 0.ToString();
        while (int.Parse(Signature.GamerData.reservedChips) < 1)
        {
            GetGamerData();
            yield return new WaitForSeconds(1f);
        }
        reserveChipsScreenHolder.SetActive(false);

        StartCoroutine(waitForChipsToChange(ChipsOldValue));
    }



    //USE AFTER REDEEM
    IEnumerator waitForSolanaBalanceToChange(double oldValue)
    {
        while (Signature.SolanaBalance == oldValue)
        {
            GetSolanaBalance();
            yield return new WaitForSeconds(1f);
        }
        GetAmountOfChipsWeb3Async(false);
        waitingForTransactionHolder.SetActive(false);
    }

    //USE AFTER CLAIM
    IEnumerator waitForChipsToChange(double oldValue)
    {
        while (Signature.StandardChipsAmount == oldValue)
        {
            GetAmountOfChipsWeb3Async(false);
            yield return new WaitForSeconds(1f);
        }
        GetAmountOfUnclaimedChipsWeb3Async();
        redeemChipsScreenHolder.SetActive(false);
        claimChipsScreenHolder.SetActive(false);
        waitingForTransactionHolder.SetActive(false);
    }

    //USE AFTER BUY
    IEnumerator waitForChipsToChangeAfterBuy(double oldValue)
    {
        while (Signature.StandardChipsAmount == oldValue)
        {
            GetAmountOfChipsWeb3Async(false);
            yield return new WaitForSeconds(1f);
        }
        GetSolanaBalance();
        waitingForTransactionHolder.SetActive(false);
        buyChipsScreenHolder.SetActive(false);
        redeemChipsScreenHolder.SetActive(false);
    }


    public async void Sell(ulong chipsAmount)
    {
        string programId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey globalConfig;
        PublicKey treasury;
        PublicKey chipMint;

        PublicKey publicKeyProgram = new PublicKey(programId);

        // Derive program addresses for GlobalConfig, Treasury, and Chip Mint
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("GLOBAL_CONFIG") }, publicKeyProgram, out globalConfig, out var bump1);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var bump2);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var bump3);

        // Prepare accounts for the SellChip instruction
        SellChipAccounts accounts = new SellChipAccounts
        {
            Seller = Web3.Account.PublicKey,
            GlobalConfig = globalConfig,
            Treasury = treasury,
            ChipMint = chipMint,
            SellerChipAccount = Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, TokenProgram22),
            TokenProgram = TokenProgram22,
            AssociatedTokenProgram = AssociatedTokenAccountProgram.ProgramIdKey
        };

        // Create the SellChip instruction
        TransactionInstruction sellChipInstruction = SolStrike.Program.SolStrikeProgram.SellChip(accounts, chipsAmount, publicKeyProgram);

        // Fetch the recent block hash
        string blockHash = await Web3.Base.GetBlockHash(Commitment.Confirmed);

        // Create and sign the transaction
        Transaction transaction = new Transaction
        {
            FeePayer = Web3.Account.PublicKey,
            RecentBlockHash = blockHash,
            Signatures = new List<SignaturePubKeyPair>(),
            Instructions = new List<TransactionInstruction> { sellChipInstruction }
        };

        Transaction signedTransaction = await Web3.Base.SignTransaction(transaction);

        waitingForTransactionHolder.SetActive(true);
        // Send the transaction
        RequestResult<string> signature = await Web3.Base.ActiveRpcClient.SendTransactionAsync(
            Convert.ToBase64String(signedTransaction.Serialize()),
            true, Commitment.Confirmed);

        if (signature.WasSuccessful)
        {
            //  Debug.Log($"Successfully sold {chipsAmount} chips. Transaction signature: {signature.Result}");
            //GetSolanaBalance();
            //GetAmountOfChipsWeb3Async(true);
            StartCoroutine(waitForChipsToChangeAfterBuy(Signature.StandardChipsAmount));



        }
        else
        {
            //Debug.LogError($"Failed to sell chips. Error: {signature.Reason}");
            waitingForTransactionHolder.SetActive(false);

        }
    }

    public async void Claim()
    {
        string programId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey treasury;
        PublicKey chipMint;
        PublicKey claimableRewardsAccount;

        PublicKey publicKeyProgram = new PublicKey(programId);

        // Derive program addresses for Treasury, Chip Mint, and Claimable Rewards
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var bump1);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var bump2);
        PublicKey.TryFindProgramAddress(new[] { Web3.Account.PublicKey.KeyBytes }, publicKeyProgram, out claimableRewardsAccount, out var bump3);

        // Prepare accounts for the ClaimChips instruction
        ClaimChipsAccounts accounts = new ClaimChipsAccounts
        {
            Signer = Web3.Account.PublicKey,
            ClaimableRewardsAccount = claimableRewardsAccount,
            ChipMint = chipMint,
            Treasury = treasury,
            TreasuryChipTokenAccount = treasury.DeriveAssociatedTokenAccount(chipMint, TokenProgram22),
            ClaimerChipAccount = Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, TokenProgram22),
            TokenProgram = TokenProgram22
        };

        // Create the ClaimChips instruction
        TransactionInstruction claimChipsInstruction = SolStrike.Program.SolStrikeProgram.ClaimChips(accounts, publicKeyProgram);

        // Fetch the recent block hash
        string blockHash = await Web3.Base.GetBlockHash(Commitment.Confirmed);

        // Create and sign the transaction
        Transaction transaction = new Transaction
        {
            FeePayer = Web3.Account.PublicKey,
            RecentBlockHash = blockHash,
            Signatures = new List<SignaturePubKeyPair>(),
            Instructions = new List<TransactionInstruction> { claimChipsInstruction }
        };

        Transaction signedTransaction = await Web3.Base.SignTransaction(transaction);
        waitingForTransactionHolder.SetActive(true);
        // Send the transaction
        RequestResult<string> signature = await Web3.Base.ActiveRpcClient.SendTransactionAsync(
            Convert.ToBase64String(signedTransaction.Serialize()),
            true, Commitment.Confirmed);

        if (signature.WasSuccessful)
        {
            //   Debug.Log($"Successfully claimed chips. Transaction signature: {signature.Result}");
            StartCoroutine(waitForChipsToChange(Signature.StandardChipsAmount));
            //GetSolanaBalance();
            //GetAmountOfChipsWeb3Async(true);
            // StartCoroutine(waitForChipsToChangeAfterBuy(Signature.StandardChipsAmount));
        }
        else
        {
            claimChipsScreenHolder.SetActive(false);
            waitingForTransactionHolder.SetActive(false);

            // Debug.LogError($"Failed to claim chips. Error: {signature.Reason}");
        }
        var now = System.DateTime.Now.ToString();
        // Debug.Log("Claimed chips at: " + now);
    }



    private IEnumerator GetLeaderboardCoroutine()
    {
        string url = Signature.baseUrl + "gamers/leaderboard/" + Web3.Account.PublicKey.Key;
        //  Debug.Log("Sending GET  leaderboard request to: " + url);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("accept", "application/json");

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"Error: {webRequest.error}\nURL: {url}");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"HTTP Error: {webRequest.error}\nCode: {webRequest.responseCode}\nURL: {url}");
                    break;
                case UnityWebRequest.Result.Success:
                    // Debug.Log($"Success! Response Code: {webRequest.responseCode}");
                    //  Debug.Log($"Response: {webRequest.downloadHandler.text}");
                    string wrappedJsonString = "{ \"players\": " + webRequest.downloadHandler.text + "}";
                    //   Debug.Log("wrappedJsonString JSON:\n" + wrappedJsonString);
                    PlayerListWrapper players = JsonUtility.FromJson<PlayerListWrapper>(wrappedJsonString);
                    // Now you can access the array of player data
                    PlayerData[] playersList = players.players;
                    // Example of how to access the parsed data
                    /*   if (players != null)
                       {
                           Debug.Log("Parsed " + players.players + " players.");
                           foreach (var player in playersList)
                           {
                               Debug.Log($"Account ID: {player.accountId}, Username: {player.username}, Points: {player.points}, Place: {player.place}");
                           }
                       }
                       else
                       {
                           Debug.LogError("Failed to parse player data.");
                       }*/
                    Signature.LeaderBoardPlayers = playersList;
                    // TODO: Parse responseJson here
                    break;
            }
        }
        SetLeaderboard();
        //SetChipsText();
        holder.SetActive(false);
    }


    public void SetChipsText()
    {
        chipsText.text = "CHIPS : " + Signature.StandardChipsAmount.ToString();
        unclaimedChipsText.text = "UNCLAIMED CHIPS : " + Signature.UnclaimedChipsAmount.ToString();
        //  matchCostText.text = "MATCH COST : 0.001 SOL";
        Points.text = "POINTS : " + Signature.GamerData.party.ToString();
        solanaBalanceText.text = "SOL BALANCE : " + Signature.SolanaBalance.ToString();
    }

    public void SetLeaderboard()
    {
        foreach (Transform child in lederboardElementsHolder.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject header = Instantiate(leaderboadHeaderPref, lederboardElementsHolder.transform);
        foreach (var player in Signature.LeaderBoardPlayers)
        {
            GameObject go = Instantiate(leaderboadElementPref, lederboardElementsHolder.transform);
            go.GetComponent<LeaderboardElement>().SetData(player);
        }
    }


    [Header("BUY CHIPS SCREEN")]
    public TMP_InputField buyChipsInputFieldBuyScreen;
    public Button buyChipsButtonBuyScreen;
    public TextMeshProUGUI InvalidInputText;
    public TextMeshProUGUI solToSpendTextBuyScreen;
    public TextMeshProUGUI balanceTextBuyScreen;
    public TextMeshProUGUI chipCostTextBuyScreen;
    private float chipCost = 0.01f;


    public void SetBuyChipsScreen()
    {
        chipCostTextBuyScreen.text = "Chip cost: " + chipCost.ToString() + " SOL";
        balanceTextBuyScreen.text = "BALANCE : " + Signature.SolanaBalance.ToString();
        solToSpendTextBuyScreen.text = "Amount to spend: 0 SOL";
        buyChipsInputFieldBuyScreen.text = "0";
    }


    private int chipsToBuywithSOL = 0;
    void OnInputFieldValueChanged(string newValue)
    {

        if (int.TryParse(newValue, out int chipsToBuy) && chipsToBuy > 0 && Signature.SolanaBalance >= chipsToBuy * chipCost)
        {
            solToSpendTextBuyScreen.text = "Amount to spend: " + (chipsToBuy * chipCost).ToString() + " SOL";
            buyChipsButtonBuyScreen.gameObject.SetActive(true);

            InvalidInputText.gameObject.SetActive(false);
            //   Debug.Log("Parsed integer: " + chipsToBuy);
            chipsToBuywithSOL = chipsToBuy;
        }
        else
        {

            if (int.TryParse(newValue, out int chipsToBuy2))
            {
                solToSpendTextBuyScreen.text = "Amount to spend: " + (chipsToBuy * chipCost).ToString() + " SOL";
            }

            buyChipsButtonBuyScreen.gameObject.SetActive(false);
            InvalidInputText.gameObject.SetActive(true);
            // Debug.Log("Invalid input, not an integer.");
        }
    }


    [Header("REDEEM SCREEN")]
    public TextMeshProUGUI chipBlanceRedeemText;
    public TMP_InputField redeemChipsInputField;
    public TextMeshProUGUI solToReciveText;
    public Button redeemChipsButton;
    public GameObject InvalidInputTextRedeem;
    private float chisToRedeem = 0;
    public void OnRedeemInputFieldValueChanged(string newValue)
    {
        //  Debug.Log(newValue);

        if (int.TryParse(newValue, out int chipsToSell))
        {
            // Debug.Log("1 Parsed integer: " + chipsToSell);

            if (chipsToSell > 0)
            {
                //   Debug.Log(Signature.StandardChipsAmount + " 2 Parsed integer: " + chipsToSell);


                if (Signature.StandardChipsAmount >= chipsToSell)
                {
                    solToReciveText.text = "SOL to receive: " + (chipsToSell * chipCost).ToString();
                    redeemChipsButton.gameObject.SetActive(true);

                    InvalidInputTextRedeem.gameObject.SetActive(false);
                    //     Debug.Log("3 Parsed integer: " + chipsToSell);
                    chisToRedeem = chipsToSell;
                }
            }
        }
        else if (IsNonNegativeNumberWithExactlyOneDecimalPlace(newValue))
        {
            float.TryParse(newValue, out float chipsToSell3);
            if (Signature.StandardChipsAmount <= chipsToSell3)
            {
                solToReciveText.text = "SOL to receive: " + (chipsToSell3 * chipCost).ToString();
                redeemChipsButton.gameObject.SetActive(true);
                InvalidInputTextRedeem.SetActive(false);
                //   Debug.Log("4 Parsed integer: " + chipsToSell3);
                chisToRedeem = chipsToSell3;
            }
            else
            {
                redeemChipsButton.gameObject.SetActive(false);
                InvalidInputTextRedeem.SetActive(true);
            }
        }
        else
        {
            redeemChipsButton.gameObject.SetActive(false);
            InvalidInputTextRedeem.gameObject.SetActive(true);
            //   Debug.Log("Invalid input, not an integer.");
        }

    }
    public void SetRedeeomChips()
    {
        redeemChipsInputField.text = "0";
        chipBlanceRedeemText.text = "CHIP BALANCE : " + Signature.StandardChipsAmount.ToString();

    }




    [Header("CLAIM CHIPS SCREEN")]
    public GameObject claimChipsButton;
    public TextMeshProUGUI chipsToClaim;
    public TextMeshProUGUI ChipsBalance;

    public void SetClaimScreen()
    {
        chipsToClaim.text = "Chips to claim: " + Signature.UnclaimedChipsAmount.ToString();
        // ChipsBalance.text = "CHIPS BALANCE: " + Signature.StandardChipsAmount.ToString();

    }


    [Header("Reserve SCREEN")]
    public GameObject reserveChipsButton;
    public TextMeshProUGUI reserveChipsWarningText;
    public TMP_InputField reserveChipsInputField;

    [Header("SCREEN HOLDERS")]
    public GameObject buyChipsScreenHolder;
    public GameObject redeemChipsScreenHolder;
    public GameObject claimChipsScreenHolder;
    public GameObject reserveChipsScreenHolder;

    public void SetReserveScreen()
    {
        ReserveScreenState();

    }

    public void ReserveScreenState()
    {
        if (Signature.StandardChipsAmount < 1)
        {
            reserveChipsWarningText.gameObject.SetActive(true);
            reserveChipsButton.SetActive(false);
            SetResrveChipsWarningText();
        }
        else
        {
            reserveChipsWarningText.gameObject.SetActive(false);
            reserveChipsButton.SetActive(true);
        }

    }


    public void SetResrveChipsWarningText()
    {
        if (Signature.StandardChipsAmount < 1)
        {
            reserveChipsWarningText.text = "BUY CHIPS FIRST";
        }
        else
        {
            reserveChipsWarningText.text = "INVALID INPUT";
        }
    }

    private int chipsToReserveAmount = 0;
    public void OnReserveInputfieldChanged(string newValue)
    {
        //  Debug.Log(newValue);
        if (int.TryParse(newValue, out int chipsToReserve))
        {
            if (chipsToReserve > 0 && Signature.StandardChipsAmount >= chipsToReserve)
            {

                reserveChipsButton.gameObject.SetActive(true);
                reserveChipsWarningText.gameObject.SetActive(false);
                chipsToReserveAmount = chipsToReserve;
            }
            else
            {
                reserveChipsButton.gameObject.SetActive(false);
                reserveChipsWarningText.gameObject.SetActive(true);
                SetResrveChipsWarningText();
            }
        }
        else
        {
            reserveChipsButton.gameObject.SetActive(false);
            reserveChipsWarningText.gameObject.SetActive(true);
            SetResrveChipsWarningText();
        }
    }



    public static bool IsNonNegativeNumberWithExactlyOneDecimalPlace(string s)
    {

        CultureInfo cultureInfo = CultureInfo.InvariantCulture; // Use InvariantCulture for consistent parsing

        if (s == null) // Guard against null input before trimming
        {
            return false;
        }
        string trimmedString = s.Trim(); // Trim whitespace once at the beginning

        if (string.IsNullOrEmpty(trimmedString)) // Check after trimming
        {
            return false;
        }

        string decimalSeparatorString = cultureInfo.NumberFormat.NumberDecimalSeparator;
        if (string.IsNullOrEmpty(decimalSeparatorString)) // Cannot have a decimal place if no separator defined
        {
            return false;
        }

        // NumberStyles.Float allows leading/trailing whitespace (already handled by Trim for structure checks),
        // sign (which we'll check via parsed value), decimal point, and exponent.
        // It does NOT allow thousands separators by default.
        NumberStyles styles = NumberStyles.Float;

        if (decimal.TryParse(trimmedString, styles, cultureInfo, out decimal parsedValue))
        {
            // New Check: Ensure the parsed value is not negative.
            if (parsedValue < 0)
            {
                return false;
            }

            int separatorIndex = trimmedString.IndexOf(decimalSeparatorString, StringComparison.Ordinal);

            if (separatorIndex == -1) // No decimal separator means it's an integer
            {
                return false; // Integers do not have "one decimal place"
            }

            // Ensure there isn't a second occurrence of the decimal separator
            if (trimmedString.IndexOf(decimalSeparatorString, separatorIndex + decimalSeparatorString.Length, StringComparison.Ordinal) != -1)
            {
                return false; // More than one decimal separator
            }

            string fractionPart = trimmedString.Substring(separatorIndex + decimalSeparatorString.Length);

            // Check if there is exactly one character after the decimal separator AND it's a digit.
            if (fractionPart.Length == 1 && char.IsDigit(fractionPart[0]))
            {
                return true;
            }
        }
        return false;
    }




    void OnDestroy()
    {
        // Remove the listener when the object is destroyed
        buyChipsInputFieldBuyScreen.onValueChanged.RemoveListener(OnInputFieldValueChanged);
    }

}