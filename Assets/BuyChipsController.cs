using UnityEngine;
using UnityEngine.UI; // Required for Button
using TMPro;          // Required for TMP_InputField
using System;
using System.Threading.Tasks;
using Solana.Unity.Wallet;           // Required for PublicKey, WalletBase
using Solana.Unity.Rpc;              // Required for IRpcClient, Commitment
using Solana.Unity.Rpc.Builders;     // Required for TransactionBuilder
using Solana.Unity.Rpc.Models;       // Required for Transaction
using Solana.Unity.Programs;         // Required for AssociatedTokenAccountProgram, TokenProgram, SystemProgram
using SolStrike;                     // Your main namespace
using SolStrike.Program;
using Solana.Unity.SDK;
using Solana.Unity.Rpc.Types;
using Unity.VisualScripting;             // Namespace containing SolStrikeProgram and account definitions

public class BuyChipsController : MonoBehaviour
{
    /*
    public static void Buy()
    {
       // TransactionInstruction.BuyChipWithSol();
        // This method is called when the object becomes enabled and active
        // You can initialize or reset variables here if needed
    }

    
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField amountInputField;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI feedbackText; // Optional: For displaying messages

    [Header("SolStrike Program Addresses")]
    [Tooltip("Public key of the GlobalConfig account")]
    [SerializeField] private string globalConfigAddress = "YOUR_GLOBAL_CONFIG_ADDRESS_HERE"; // <-- SET THIS IN INSPECTOR

    [Tooltip("Public key of the Treasury account")]
    [SerializeField] private string treasuryAddress = "YOUR_TREASURY_ADDRESS_HERE"; // <-- SET THIS IN INSPECTOR

    [Tooltip("Public key of the Chip SPL Token Mint")]
    [SerializeField] private string chipMintAddress = "YOUR_CHIP_MINT_ADDRESS_HERE"; // <-- SET THIS IN INSPECTOR

    private SolStrikeClient _solStrikeClient;
    private WalletBase _wallet; // Assuming you get this from your Web3 setup
    private IRpcClient _rpcClient; // Assuming you get this from your Web3 setup

    void Start()
    {
        // --- IMPORTANT: Get Wallet and RpcClient ---
        // This assumes you have a central place (like Web3.Instance) providing these.
        // Adapt this part based on your Solana.Unity SDK setup.
        if (Web3.Instance == null)
        {
            Debug.LogError("Web3.Instance is not initialized. Cannot proceed.");
            SetUIState(false, "Error: Web3 not ready");
            return;
        }
        _wallet = Web3.Instance.WalletBase;
        _rpcClient = Web3.Instance.WalletBase.ActiveRpcClient;

        if (_wallet == null || _rpcClient == null)
        {
            Debug.LogError("Wallet or RpcClient is null. Cannot proceed.");
            SetUIState(false, "Error: Wallet/RPC not ready");
            return;
        }
        // --- Initialize SolStrikeClient ---
        // Use the same RpcClient your wallet uses. You might need a StreamingRpcClient too if your setup requires it.
        _solStrikeClient = new SolStrikeClient(_rpcClient, Web3.Instance.WalletBase.ActiveStreamingRpcClient); // Adjust if needed

        // --- Setup Button Listener ---
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyChipsClicked);
        }
        else
        {
            Debug.LogError("Buy Button is not assigned in the Inspector.");
        }

        if (amountInputField == null)
        {
            Debug.LogError("Amount Input Field is not assigned in the Inspector.");
            SetUIState(false);
        }

        SetUIState(true); // Enable UI initially
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyChipsClicked);
        }
    }

    private async void OnBuyChipsClicked()
    {
        if (_wallet.Account == null)
        {
            Debug.LogError("Wallet is not connected.");
            SetFeedback("Error: Wallet not connected.", true);
            return;
        }

        // 1. Read and Validate Input
        if (!ulong.TryParse(amountInputField.text, out ulong amountToBuy) || amountToBuy == 0)
        {
            Debug.LogError("Invalid amount entered.");
            SetFeedback("Error: Please enter a valid positive number of chips.", true);
            return;
        }

        Debug.Log($"Attempting to buy {amountToBuy} chips...");
        SetUIState(false, "Processing purchase..."); // Disable UI during transaction

        try
        {
            // 2. Perform the Purchase Logic
            bool success = await PerformBuyChips(amountToBuy);

            if (success)
            {
                SetFeedback($"Successfully purchased {amountToBuy} chips!", false);
                // Optionally clear the input field
                amountInputField.text = "";
            }
            // Error message is set within PerformBuyChips on failure
        }
        catch (Exception ex)
        {
            Debug.LogError($"An exception occurred during purchase: {ex}");
            SetFeedback($"Error: {ex.Message}", true);
        }
        finally
        {
            SetUIState(true); // Re-enable UI
        }
    }

    private async Task<bool> PerformBuyChips(ulong amount)
    {

        if (_wallet.Account == null) return false; // Should be checked before calling, but double-check

        try
        {
            // 3. Prepare Account Public Keys
            PublicKey buyerKey = _wallet.Account.PublicKey;
            PublicKey globalConfigKey = new PublicKey(globalConfigAddress);
            PublicKey treasuryKey = new PublicKey(treasuryAddress);
            PublicKey chipMintKey = new PublicKey(chipMintAddress);

            // Calculate the buyer's Associated Token Account (ATA) for the chip mint
            PublicKey buyerChipAccountKey = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                buyerKey,
                chipMintKey
            );

            Debug.Log($"Buyer: {buyerKey}");
            Debug.Log($"Buyer Chip ATA: {buyerChipAccountKey}");
            Debug.Log($"Global Config: {globalConfigKey}");
            Debug.Log($"Treasury: {treasuryKey}");
            Debug.Log($"Chip Mint: {chipMintKey}");

            // 4. Create the Accounts object for the instruction
            var accounts = new BuyChipWithSolAccounts
            {
                Buyer = buyerKey,
                GlobalConfig = globalConfigKey,
                Treasury = treasuryKey,
                ChipMint = chipMintKey,
                BuyerChipAccount = buyerChipAccountKey,
                // Standard program IDs are usually included by default, but set explicitly if needed
                TokenProgram = TokenProgram.ProgramIdKey,
                AssociatedTokenProgram = AssociatedTokenAccountProgram.ProgramIdKey,
                SystemProgram = SystemProgram.ProgramIdKey
            };

            // 5. Build the Instruction
            TransactionInstruction buyInstruction = SolStrikeProgram.BuyChipWithSol(accounts, amount);

            // 6. Build the Transaction
            var blockhashResult = await _rpcClient.GetRecentBlockHashAsync();
            if (!blockhashResult.WasSuccessful)
            {
                Debug.LogError("Failed to get recent blockhash.");
                SetFeedback("Error: Could not get network blockhash.", true);
                return false;
            }

            var transaction = new TransactionBuilder()
                .SetRecentBlockHash(blockhashResult.Result.Value.Blockhash)
                .SetFeePayer(buyerKey) // The user pays the transaction fee
                .AddInstruction(buyInstruction) // Add our specific instruction
                .Build(_wallet.Account); // Build requires the fee payer Account, used for estimates/simulation if needed


            SolStrikeClient

            // 7. Sign the Transaction
            var signedTransaction = await Web3.Base.SignTransaction(transaction);
            if (signedTransaction == null)
            {
                Debug.LogError("Transaction signing failed or was cancelled.");
                SetFeedback("Error: Transaction signing failed.", true);
                return false;
            }

            // 8. Send the Transaction
            Debug.Log("Sending transaction...");
            var txSignatureResult = await _rpcClient.SendTransactionAsync(
                Convert.ToBase64String(signedTransaction.Serialize()), // Send serialized, base64 encoded tx
                commitment: Commitment.Confirmed // Wait for confirmation
            );

            // 9. Handle the Result
            if (txSignatureResult.WasSuccessful)
            {
                Debug.Log($"Transaction successful! Signature: {txSignatureResult.Result}");
                // Optional: You might want to wait for finalization or further confirmations here
                // depending on your application's needs.
                return true;
            }
            else
            {
                Debug.LogError($"Transaction failed: {txSignatureResult.Reason}");
                string errorMsg = txSignatureResult.Reason;
                if (txSignatureResult.ServerErrorCode != 0)
                {
                    errorMsg += $" (Code: {txSignatureResult.ServerErrorCode})";
                    // You could potentially try to parse the error code using the Errors namespace if it matches a program error
                }
                SetFeedback($"Error: Transaction failed. {errorMsg}", true);
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during PerformBuyChips: {e}");
            SetFeedback($"Error: {e.Message}", true);
            return false;
        }
    }

    // Helper to manage UI interactability and feedback
    private void SetUIState(bool interactable, string message = "")
    {
        if (buyButton != null) buyButton.interactable = interactable;
        if (amountInputField != null) amountInputField.interactable = interactable;
        SetFeedback(message, false); // Clear errors when enabling/disabling generally
    }

    // Helper to show feedback (can be expanded)
    private void SetFeedback(string message, bool isError)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isError ? Color.red : Color.black; // Or your preferred colors
        }
        if (isError && !string.IsNullOrEmpty(message))
        {
            Debug.LogError($"Feedback Error: {message}");
        }
        else if (!string.IsNullOrEmpty(message))
        {
            Debug.Log($"Feedback: {message}");
        }
    }*/
}