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
using Solana.Unity.Programs.Models;

public class SolanaUIHandler : MonoBehaviour
{
    #region UI References

    [Header("Notification Sprites")]
    public Sprite successBuySprite;
    public Sprite successSellSprite;
    public Sprite successReserveSprite;
    public Sprite successClaimSprite;
    public Sprite errorSprite;

    [Header("Notification Controllers")]
    public NotificationController notificationController;

    [Header("UI Holders")]
    public GameObject waitingForTransactionHolder;
    public GameObject holder;
    public GameObject ConncetAgainHolder;
    public GameObject SignAgainHolder;

    [Header("Text Fields")]
    public TextMeshProUGUI unclaimedChipsText;
    public TextMeshProUGUI chipsText;
    public TextMeshProUGUI Points;
    public TextMeshProUGUI solanaBalanceText;
    public TextMeshProUGUI TextMeshProUGUI;
    public TextMeshProUGUI balance;

    [Header("Wallet UI")]
    public GameObject walletElementPref;
    public GameObject walletElementHolder;
    public GameObject closeButton;

    [Header("Leaderboard UI")]
    public GameObject lederboardElementsHolder;
    public GameObject leaderboadElementPref;
    public GameObject leaderboadHeaderPref;

    [Header("Buy Chips Screen")]
    public TMP_InputField buyChipsInputFieldBuyScreen;
    public Button buyChipsButtonBuyScreen;
    public TextMeshProUGUI InvalidInputText;
    public TextMeshProUGUI solToSpendTextBuyScreen;
    public TextMeshProUGUI balanceTextBuyScreen;
    public TextMeshProUGUI chipCostTextBuyScreen;
    private float chipCost = 0.01f;

    [Header("Redeem Screen")]
    public TextMeshProUGUI chipBlanceRedeemText;
    public TMP_InputField redeemChipsInputField;
    public TextMeshProUGUI solToReciveText;
    public Button redeemChipsButton;
    public GameObject InvalidInputTextRedeem;
    private float chisToRedeem = 0;

    [Header("Claim Chips Screen")]
    public GameObject claimChipsButton;
    public TextMeshProUGUI chipsToClaim;
    public TextMeshProUGUI ChipsBalance;

    [Header("Reserve Screen")]
    public GameObject reserveChipsButton;
    public TextMeshProUGUI reserveChipsWarningText;
    public TMP_InputField reserveChipsInputField;

    [Header("Screen Holders")]
    public GameObject buyChipsScreenHolder;
    public GameObject redeemChipsScreenHolder;
    public GameObject claimChipsScreenHolder;
    public GameObject reserveChipsScreenHolder;

    #endregion

    #region Fields

    private PublicKey TokenProgram22 = new PublicKey("TokenzQdBNbLqP5VEhdkAS6EPFLC1PHnBqCXEpPxuEb");
    public bl_Lobby bl_Lobby;
    private string baseUrl = "https://api.solstrike.xyz/api/gamers/";
    private int chipsToBuywithSOL = 0;
    private int chipsToReserveAmount = 0;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        buyChipsInputFieldBuyScreen.onValueChanged.AddListener(OnInputFieldValueChanged);
        redeemChipsInputField.onValueChanged.AddListener(OnRedeemInputFieldValueChanged);
        reserveChipsInputField.onValueChanged.AddListener(OnReserveInputfieldChanged);

        if (!Signature.isFirstTime)
            LoadData();
    }

    private void OnEnable()
    {
#if !UNITY_EDITOR
        if (!string.IsNullOrEmpty(Web3.Account.PublicKey.Key))
        {
            holder.SetActive(false);
        }
        else
        {
            _ = DoLogin();
        }
#else
        bl_Lobby.GPN();
        holder.SetActive(false);
#endif
    }

    private void OnDisable()
    {
        Web3.OnLogin -= OnLogin;
        Web3.OnBalanceChange -= OnBalanceChange;
    }

    void OnDestroy()
    {
        buyChipsInputFieldBuyScreen.onValueChanged.RemoveListener(OnInputFieldValueChanged);
    }

    #endregion

    #region Wallet & Authentication

    public async Task DoLogin()
    {
        try
        {
            Web3.OnLogin += OnLogin;
            Web3.OnBalanceChange += OnBalanceChange;
            await Web3.Instance.LoginWalletAdapter();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("User cancelled wallet login.");
        }
        catch (Exception ex)
        {
            ConncetAgainHolder.SetActive(true);
            notificationController.ShowWarning();
            Debug.LogWarning($"Wallet login failed: {ex.Message}");
        }
    }

    public void LoginAgain()
    {
        ConncetAgainHolder.SetActive(false);
        Web3.OnLogin -= OnLogin;
        Web3.OnBalanceChange -= OnBalanceChange;
        _ = DoLogin();
    }

    public void OnLogin(Account account)
    {
        Signature.PublicKey = account.PublicKey.Key;
        TextMeshProUGUI.text = "SIGN MESSAGE";
        Signature.PublicKeyBytes = Encoding.UTF8.GetBytes(Signature.PublicKey);
        _ = SignMessageAsync();
    }

    public async Task SignMessageAsync()
    {
        try
        {
            string signedMessageId = Signature.Poruka + " - " + ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            byte[] bytes = Encoding.UTF8.GetBytes(signedMessageId);

            byte[] response = await Web3.Wallet.SignMessage(bytes);
            string base58Result = Solana.Unity.Wallet.Utilities.Encoders.Base58.EncodeData(response);

            Signature.SignedMessage = signedMessageId;
            Signature.SignatureString = base58Result;

            bool verified = Web3.Account.Verify(bytes, response);
            await GetAmountOfChipsWeb3Async();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("User rejected the sign message operation.");
        }
        catch (Exception ex)
        {
            SignAgainHolder.SetActive(true);
            notificationController.ShowWarning();
            Debug.LogWarning($"Wallet sign message failed: {ex.Message}");
        }
    }

    public void SingMessageAgain()
    {
        SignAgainHolder.SetActive(false);
        _ = SignMessageAsync();
    }

    public void OnBalanceChange(double amount)
    {
        Signature.SolanaBalance = amount;
        solanaBalanceText.text = $"SOL BALANCE : {Signature.SolanaBalance}";
        Debug.Log("sol balance: " + amount);
    }

    #endregion

    #region Data Loading

    public async Task LoadData()
    {
        GetGamerData();
        await GetAmountOfChipsWeb3Async(true);
        await GetSolanaBalance();
        StartCoroutine(GetUnclaimedChipsAfterSeconds(5));
    }

    [ContextMenu("GetGamerData")]
    public void GetGamerData() => StartCoroutine(GetGamerDataCoroutine());

    private IEnumerator GetGamerDataCoroutine()
    {
        string url = baseUrl + Web3.Account.PublicKey.Key;
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
                    try
                    {
                        GamerData gamerData = JsonUtility.FromJson<GamerData>(webRequest.downloadHandler.text);
                        Signature.GamerData = gamerData;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error parsing JSON: " + ex.Message);
                    }
                    break;
            }
        }
    }

    public async Task GetSolanaBalance()
    {
        Signature.SolanaBalance = await Web3.Wallet.GetBalance();
    }

    public async Task GetAmountOfChipsWeb3Async(bool updateUnclaimed = true)
    {
        PublicKey chipMint;
        PublicKey publicKeyProgram = new PublicKey(SolStrike.Program.SolStrikeProgram.ID);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var _);
        var tokenABalance = await Web3.Rpc.GetTokenAccountBalanceAsync(Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, TokenProgram22), Commitment.Confirmed);

        if (tokenABalance.WasSuccessful)
        {
            Signature.StandardChipsAmount = tokenABalance.Result.Value.AmountDouble;
            chipsText.text = $"CHIPS BALANCE : {Signature.StandardChipsAmount}";
            solanaBalanceText.text = $"SOL BALANCE : {Signature.SolanaBalance}";
        }
        else
        {
            chipsText.text = $"CHIPS BALANCE : {Signature.StandardChipsAmount}";
            Signature.StandardChipsAmount = 0;
        }

        if (updateUnclaimed)
        {
            bl_Lobby.GPN();
            await GetAmountOfUnclaimedChipsWeb3Async();
        }
    }

    private IEnumerator GetUnclaimedChipsAfterSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        _ = GetAmountOfUnclaimedChipsWeb3Async();
    }

    public async Task GetAmountOfUnclaimedChipsWeb3Async()
    {
        PublicKey claimableRewardsPDA;
        SolStrikeClient solStrikeClient = new SolStrikeClient(Web3.Rpc, Web3.Base.ActiveStreamingRpcClient, new PublicKey(SolStrike.Program.SolStrikeProgram.ID));
        PublicKey.TryFindProgramAddress(new[] { Web3.Base.Account.PublicKey.KeyBytes }, new PublicKey(SolStrike.Program.SolStrikeProgram.ID), out claimableRewardsPDA, out var _);

        try
        {
            var a = await solStrikeClient.GetClaimableRewardsAsync(claimableRewardsPDA, Commitment.Confirmed);
            if (!a.WasSuccessful)
            {
                Signature.UnclaimedChipsAmount = 0;
                unclaimedChipsText.text = $"UNCLAIMED CHIPS : {Signature.UnclaimedChipsAmount}";
                return;
            }
            ulong amountUlong = a.ParsedResult.Amount;
            double amountInDesiredUnit = (double)amountUlong / 1_000_000_000.0;
            Signature.UnclaimedChipsAmount = amountInDesiredUnit;
            unclaimedChipsText.text = $"UNCLAIMED CHIPS : {Signature.UnclaimedChipsAmount}";
        }
        catch (Exception ex)
        {
            Debug.LogError("ERR GetAmountOfUnclaimedChipsWeb3Async:" + ex);
        }

        StartCoroutine(GetLeaderboardCoroutine());
    }

    #endregion

    #region Transaction Methods

    public async void Buy(ulong chipsAmount)
    {
        double oldValue = Signature.StandardChipsAmount;
        string ProgramId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey globalConfig, treasury, chipMint;
        PublicKey publicKeyProgram = new PublicKey(ProgramId);

        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("GLOBAL_CONFIG") }, publicKeyProgram, out globalConfig, out var _);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var _);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var _);

        BuyChipWithSolAccounts account = new BuyChipWithSolAccounts
        {
            Buyer = Web3.Account.PublicKey,
            GlobalConfig = globalConfig,
            Treasury = treasury,
            ChipMint = chipMint,
            BuyerChipAccount = Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, TokenProgram22),
            TokenProgram = TokenProgram22,
            AssociatedTokenProgram = AssociatedTokenAccountProgram.ProgramIdKey,
            SystemProgram = SystemProgram.ProgramIdKey
        };

        TransactionInstruction buyChipInstruction = SolStrike.Program.SolStrikeProgram.BuyChipWithSol(account, chipsAmount, publicKeyProgram);
        string blockHash = await Web3.Base.GetBlockHash(Commitment.Confirmed);

        Transaction transaction = new Transaction
        {
            FeePayer = Web3.Account.PublicKey,
            RecentBlockHash = blockHash,
            Signatures = new List<SignaturePubKeyPair>(),
            Instructions = new List<TransactionInstruction> { buyChipInstruction }
        };

        waitingForTransactionHolder.SetActive(true);
        try
        {
            Transaction signedTransaction = await Base.SignTransaction(transaction);
            RequestResult<string> signature = await Base.ActiveRpcClient.SendTransactionAsync(
                Convert.ToBase64String(signedTransaction.Serialize()), true, Commitment.Confirmed);

            if (signature.WasSuccessful)
            {
                StartCoroutine(waitForChipsToChangeAfterBuy(oldValue));
                notificationController.ShowNotification(successBuySprite);
            }
            else
            {
                Debug.LogError($"RPC error: {signature.Reason}");
                waitingForTransactionHolder.SetActive(false);
                buyChipsScreenHolder.SetActive(false);
                notificationController.ShowNotification(errorSprite);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("User rejected the transaction.");
            waitingForTransactionHolder.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Wallet rejected: {ex.Message}");
            waitingForTransactionHolder.SetActive(false);
        }
    }

    public async void Reserve(ulong vhipsAmount)
    {
        double oldValue = Signature.StandardChipsAmount;
        string programId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey treasury, chipMint;
        PublicKey publicKeyProgram = new PublicKey(programId);

        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var _);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var _);

        ReserveChipsAccounts accounts = new ReserveChipsAccounts
        {
            Signer = Web3.Account.PublicKey,
            Treasury = treasury,
            ChipMint = chipMint,
            TreasuryChipTokenAccount = treasury.DeriveAssociatedTokenAccount(chipMint, TokenProgram22),
            UserChipAccount = Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, TokenProgram22),
            TokenProgram = TokenProgram22
        };

        TransactionInstruction reserveVhipsInstruction = SolStrike.Program.SolStrikeProgram.ReserveChips(accounts, vhipsAmount, publicKeyProgram);
        waitingForTransactionHolder.SetActive(true);

        try
        {
            string blockHash = await Web3.Base.GetBlockHash(Commitment.Confirmed);
            Transaction transaction = new Transaction
            {
                FeePayer = Web3.Account.PublicKey,
                RecentBlockHash = blockHash,
                Signatures = new List<SignaturePubKeyPair>(),
                Instructions = new List<TransactionInstruction> { reserveVhipsInstruction }
            };

            Transaction signedTransaction = await Web3.Base.SignTransaction(transaction);
            RequestResult<string> signature = await Web3.Base.ActiveRpcClient.SendTransactionAsync(
                Convert.ToBase64String(signedTransaction.Serialize()), true, Commitment.Confirmed);

            if (signature.WasSuccessful)
            {
                StartCoroutine(waitReservedChipsToChange(oldValue));
                notificationController.ShowNotification(successReserveSprite);
            }
            else
            {
                Debug.LogError($"RPC error: {signature.Reason}");
                reserveChipsScreenHolder.SetActive(false);
                waitingForTransactionHolder.SetActive(false);
                notificationController.ShowNotification(errorSprite);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("User rejected the transaction.");
            waitingForTransactionHolder.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Wallet rejected: {ex.Message}");
            waitingForTransactionHolder.SetActive(false);
        }
    }

    public async void Sell(ulong chipsAmount)
    {
        double oldValue = Signature.StandardChipsAmount;
        string programId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey globalConfig, treasury, chipMint;
        PublicKey publicKeyProgram = new PublicKey(programId);

        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("GLOBAL_CONFIG") }, publicKeyProgram, out globalConfig, out var _);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var _);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var _);

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

        TransactionInstruction sellChipInstruction = SolStrike.Program.SolStrikeProgram.SellChip(accounts, chipsAmount, publicKeyProgram);
        string blockHash = await Web3.Base.GetBlockHash(Commitment.Confirmed);

        Transaction transaction = new Transaction
        {
            FeePayer = Web3.Account.PublicKey,
            RecentBlockHash = blockHash,
            Signatures = new List<SignaturePubKeyPair>(),
            Instructions = new List<TransactionInstruction> { sellChipInstruction }
        };

        waitingForTransactionHolder.SetActive(true);

        try
        {
            Transaction signedTransaction = await Web3.Base.SignTransaction(transaction);
            RequestResult<string> signature = await Web3.Base.ActiveRpcClient.SendTransactionAsync(
                Convert.ToBase64String(signedTransaction.Serialize()), true, Commitment.Confirmed);

            if (signature.WasSuccessful)
            {
                StartCoroutine(waitForChipsToChangeAfterBuy(oldValue));
                notificationController.ShowNotification(successSellSprite);
            }
            else
            {
                Debug.LogError($"RPC error: {signature.Reason}");
                waitingForTransactionHolder.SetActive(false);
                redeemChipsScreenHolder.SetActive(false);
                notificationController.ShowNotification(errorSprite);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("User rejected the transaction.");
            waitingForTransactionHolder.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Wallet rejected: {ex.Message}");
            waitingForTransactionHolder.SetActive(false);
        }
    }

    public async void Claim()
    {
        double oldValue = Signature.StandardChipsAmount;
        string programId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey treasury, chipMint, claimableRewardsAccount;
        PublicKey publicKeyProgram = new PublicKey(programId);

        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var _);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var _);
        PublicKey.TryFindProgramAddress(new[] { Web3.Account.PublicKey.KeyBytes }, publicKeyProgram, out claimableRewardsAccount, out var _);

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

        TransactionInstruction claimChipsInstruction = SolStrike.Program.SolStrikeProgram.ClaimChips(accounts, publicKeyProgram);
        string blockHash = await Web3.Base.GetBlockHash(Commitment.Confirmed);

        Transaction transaction = new Transaction
        {
            FeePayer = Web3.Account.PublicKey,
            RecentBlockHash = blockHash,
            Signatures = new List<SignaturePubKeyPair>(),
            Instructions = new List<TransactionInstruction> { claimChipsInstruction }
        };

        waitingForTransactionHolder.SetActive(true);

        try
        {
            Transaction signedTransaction = await Web3.Base.SignTransaction(transaction);
            RequestResult<string> signature = await Web3.Base.ActiveRpcClient.SendTransactionAsync(
                Convert.ToBase64String(signedTransaction.Serialize()), true, Commitment.Confirmed);

            if (signature.WasSuccessful)
            {
                StartCoroutine(waitForChipsToChange(oldValue));
                notificationController.ShowNotification(successClaimSprite);
            }
            else
            {
                Debug.LogError($"RPC error: {signature.Reason}");
                notificationController.ShowNotification(errorSprite);
                claimChipsScreenHolder.SetActive(false);
                waitingForTransactionHolder.SetActive(false);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("User rejected the transaction.");
            waitingForTransactionHolder.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Wallet rejected: {ex.Message}");
            waitingForTransactionHolder.SetActive(false);
        }
    }

    #endregion

    #region UI/Leaderboard/Helpers

    private IEnumerator GetLeaderboardCoroutine()
    {
        string url = Signature.baseUrl + "gamers/leaderboard/" + Web3.Account.PublicKey.Key;
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
                    string wrappedJsonString = "{ \"players\": " + webRequest.downloadHandler.text + "}";
                    PlayerListWrapper players = JsonUtility.FromJson<PlayerListWrapper>(wrappedJsonString);
                    Signature.LeaderBoardPlayers = players.players;
                    break;
            }
        }
        SetLeaderboard();
        holder.SetActive(false);
    }

    public void SetChipsText()
    {
        chipsText.text = $"CHIPS : {Signature.StandardChipsAmount}";
        unclaimedChipsText.text = $"UNCLAIMED CHIPS : {Signature.UnclaimedChipsAmount}";
        Points.text = $"POINTS : {Signature.GamerData.party}";
        solanaBalanceText.text = $"SOL BALANCE : {Signature.SolanaBalance}";
    }

    public void SetLeaderboard()
    {
        foreach (Transform child in lederboardElementsHolder.transform)
            Destroy(child.gameObject);

        Instantiate(leaderboadHeaderPref, lederboardElementsHolder.transform);

        foreach (var player in Signature.LeaderBoardPlayers)
        {
            GameObject go = Instantiate(leaderboadElementPref, lederboardElementsHolder.transform);
            go.GetComponent<LeaderboardElement>().SetData(player);
        }
    }

    public void SetBuyChipsScreen()
    {
        chipCostTextBuyScreen.text = "Chip cost: <color=white>" + chipCost.ToString() + " SOL";
        balanceTextBuyScreen.text = "BALANCE : " + Signature.SolanaBalance.ToString();
        solToSpendTextBuyScreen.text = "Amount to spend:<color=white> 0 SOL";
        buyChipsInputFieldBuyScreen.text = "0";
    }

    void OnInputFieldValueChanged(string newValue)
    {
        if (int.TryParse(newValue, out int chipsToBuy) && chipsToBuy > 0 && Signature.SolanaBalance >= chipsToBuy * chipCost)
        {
            solToSpendTextBuyScreen.text = "Amount to spend:<color=white> " + (chipsToBuy * chipCost).ToString() + " SOL";
            buyChipsButtonBuyScreen.gameObject.SetActive(true);
            InvalidInputText.gameObject.SetActive(false);
            chipsToBuywithSOL = chipsToBuy;
        }
        else
        {
            if (int.TryParse(newValue, out int chipsToBuy2))
            {
                solToSpendTextBuyScreen.text = "Amount to spend:<color=white> " + (chipsToBuy2 * chipCost).ToString() + " SOL";
            }
            buyChipsButtonBuyScreen.gameObject.SetActive(false);
            InvalidInputText.gameObject.SetActive(true);
        }
    }

    public void SetRedeeomChips()
    {
        redeemChipsInputField.text = "0";
        chipBlanceRedeemText.text = "CHIP BALANCE : <color=white>\"" + Signature.StandardChipsAmount.ToString();
    }

    public void OnRedeemInputFieldValueChanged(string newValue)
    {
        bool valid = false;
        float value = 0;

        // Try integer
        if (int.TryParse(newValue, out int intValue))
        {
            value = intValue;
            valid = intValue > 0 && value <= Signature.StandardChipsAmount;
            solToReciveText.text = $"SOL to receive: <color=white>{value * chipCost}";

        }
        // Try decimal with exactly one decimal place
        else if (IsNonNegativeNumberWithExactlyOneDecimalPlace(newValue))
        {
            float.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            valid = value > 0 && value <= (float)Signature.StandardChipsAmount;
            solToReciveText.text = $"SOL to receive: <color=white>{value * chipCost}";

        }

        if (valid)
        {
            solToReciveText.text = $"SOL to receive: <color=white>{value * chipCost}";
            redeemChipsButton.gameObject.SetActive(true);
            InvalidInputTextRedeem.gameObject.SetActive(false);
            chisToRedeem = value;
        }
        else
        {
            redeemChipsButton.gameObject.SetActive(false);
            InvalidInputTextRedeem.gameObject.SetActive(true);
        }
    }


    public void SetClaimScreen()
    {
        chipsToClaim.text = "Chips to claim: <color=white>" + Signature.UnclaimedChipsAmount.ToString();
    }

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
            reserveChipsInputField.text = 1.ToString();
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

    public void OnReserveInputfieldChanged(string newValue)
    {
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
        if (string.IsNullOrWhiteSpace(s))
            return false;

        var culture = CultureInfo.InvariantCulture;
        var trimmed = s.Trim();

        // Must contain a single decimal separator
        int sep = trimmed.IndexOf(culture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal);
        if (sep == -1)
            return false;

        // Only one decimal separator allowed
        if (trimmed.IndexOf(culture.NumberFormat.NumberDecimalSeparator, sep + 1, StringComparison.Ordinal) != -1)
            return false;

        // Must be a valid float
        if (!float.TryParse(trimmed, NumberStyles.Float, culture, out float value))
            return false;

        if (value < 0)
            return false;

        // Check for exactly one digit after the decimal separator
        string[] parts = trimmed.Split(culture.NumberFormat.NumberDecimalSeparator[0]);
        if (parts.Length != 2)
            return false;

        return parts[1].Length == 1 && int.TryParse(parts[1], out _);
    }

    #endregion

    #region Coroutines

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

    #endregion

    #region ContextMenu Test Methods

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

    #endregion
}