using System;
using System.Collections;
using System.Collections.Generic;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using SolStrike.Program;
using System.Text;
using UnityEngine;
using Solana.Unity.Wallet;

public class ReserveChipsController : MonoBehaviour
{



    public void ReserveChip()
    {
        Reserve(1000); // Example chips amount
    }

    public async void Reserve(ulong chipsAmount)
    {
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
            TreasuryChipTokenAccount = treasury.DeriveAssociatedTokenAccount(chipMint, Signature.TokenProgram22),
            UserChipAccount = Web3.Account.PublicKey.DeriveAssociatedTokenAccount(chipMint, Signature.TokenProgram22),
            TokenProgram = Signature.TokenProgram22
        };

        Debug.Log($"Signer: {accounts.Signer}");
        Debug.Log($"Treasury: {accounts.Treasury}");
        Debug.Log($"ChipMint: {accounts.ChipMint}");
        Debug.Log($"TreasuryChipTokenAccount: {accounts.TreasuryChipTokenAccount}");
        Debug.Log($"UserChipAccount: {accounts.UserChipAccount}");
        Debug.Log($"TokenProgram: {accounts.TokenProgram}");

        // Create the ReserveChips instruction
        TransactionInstruction reserveChipsInstruction = SolStrike.Program.SolStrikeProgram.ReserveChips(accounts, chipsAmount, publicKeyProgram);

        // Fetch the recent block hash
        string blockHash = await Web3.Base.GetBlockHash();

        // Create and sign the transaction
        Transaction transaction = new Transaction
        {
            FeePayer = Web3.Account.PublicKey,
            RecentBlockHash = blockHash,
            Signatures = new List<SignaturePubKeyPair>(),
            Instructions = new List<TransactionInstruction> { reserveChipsInstruction }
        };

        Transaction signedTransaction = await Web3.Base.SignTransaction(transaction);

        // Send the transaction
        RequestResult<string> signature = await Web3.Base.ActiveRpcClient.SendTransactionAsync(
            Convert.ToBase64String(signedTransaction.Serialize()),
            true, Commitment.Confirmed);

        if (signature.WasSuccessful)
        {
            Debug.Log($"Successfully reserved {chipsAmount} chips. Transaction signature: {signature.Result}");
        }
        else
        {
            Debug.LogError($"Failed to reserve chips. Error: {signature.Reason}");
        }
    }
}
