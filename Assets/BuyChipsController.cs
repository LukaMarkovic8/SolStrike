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
using Unity.VisualScripting;
using System.Text;
using System.Collections.Generic;
using Solana.Unity.Rpc.Core.Http;             // Namespace containing SolStrikeProgram and account definitions

public class BuyChipsController : MonoBehaviour
{
    

    public async void Buy(ulong chipsAmount)
    {
        string ProgramId = SolStrike.Program.SolStrikeProgram.ID;
        PublicKey globalConfig;
        PublicKey treasury;
        PublicKey chipMint;

        PublicKey publicKeyProgram = new PublicKey(ProgramId);

        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("GLOBAL_CONFIG") }, publicKeyProgram, out globalConfig, out var bump1);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("TREASURY") }, publicKeyProgram, out treasury, out var bump2);
        PublicKey.TryFindProgramAddress(new[] { Encoding.UTF8.GetBytes("CHIP_MINT") }, publicKeyProgram, out chipMint, out var bump3);

        BuyChipWithSolAccounts account = new BuyChipWithSolAccounts
        {
            Buyer = Web3.Account.PublicKey,
            GlobalConfig = globalConfig,
            Treasury = treasury,
            ChipMint = chipMint,
            BuyerChipAccount = Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, Signature.TokenProgram22),
            TokenProgram = Signature.TokenProgram22,
            AssociatedTokenProgram = AssociatedTokenAccountProgram.ProgramIdKey,
            SystemProgram = SystemProgram.ProgramIdKey
        };

        TransactionInstruction buyChipInstruction = SolStrike.Program.SolStrikeProgram.BuyChipWithSol(account, chipsAmount, publicKeyProgram);

        string blockHash = await Web3.Base.GetBlockHash();

        Transaction transaction = new Transaction
        {
            FeePayer = Web3.Account.PublicKey,
            RecentBlockHash = blockHash,
            Signatures = new List<SignaturePubKeyPair>(),
            Instructions = new List<TransactionInstruction> { buyChipInstruction }
        };

        Transaction signedTransaction = await Web3.Base.SignTransaction(transaction);

        RequestResult<string> signature = await Web3.Base.ActiveRpcClient.SendTransactionAsync(
            Convert.ToBase64String(signedTransaction.Serialize()),
            true, Commitment.Confirmed);

        if (signature.WasSuccessful)
        {
            Debug.Log($"Successfully bought {chipsAmount} chips. Transaction signature: {signature.Result}");
        }
        else
        {
            Debug.LogError($"Failed to buy chips. Error: {signature.Reason}");
        }
    }
}