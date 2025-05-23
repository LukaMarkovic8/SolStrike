using Solana.Unity.Programs;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.Rpc.Models;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Solana.Unity.SDK.Editor 
{
    public static class SolanaEditorUtility 
    { 

        #region Styling Constants

        public static readonly RectOffset standardMargin = new(5, 5, 2, 2);
        public static readonly RectOffset standardPadding = new(10, 10, 2, 2);
        public static readonly RectOffset scrollViewPadding = new(15, 15, 2, 2);

        public static readonly int headingFontSize = 16;

        #endregion
        
        #region GUIStyles

        public static readonly GUIStyle propLabelStyle = new(GUI.skin.label) {
            stretchWidth = false,
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            padding = standardPadding,
            margin = standardMargin
        };

        public static readonly GUIStyle headingLabelStyle = new(GUI.skin.label) {
            stretchWidth = true,
            fontStyle = FontStyle.Bold,
            fontSize = headingFontSize,
            padding = standardPadding,
            margin = standardMargin,
            wordWrap = true
        };

        public static readonly GUIStyle selectButtonStyle = new(GUI.skin.button) {
            stretchWidth = false,
            padding = standardPadding,
            margin = standardMargin
        };

        public static readonly GUIStyle fileSelectPathStyle = new(GUI.skin.box) {
            stretchWidth = true,
            wordWrap = false,
            alignment = TextAnchor.MiddleLeft,
            padding = standardPadding,
            margin = standardMargin,
            clipping = TextClipping.Clip
        };

        public static readonly GUIStyle textFieldStyle = new(GUI.skin.textField) {
            stretchWidth = true,
            alignment = TextAnchor.MiddleLeft,
            padding = standardPadding,
            margin = standardMargin
        };

        public static readonly GUIStyle scrollViewStyle = new(GUI.skin.scrollView) {
            padding = scrollViewPadding,
            stretchHeight = false,
        };

        #endregion

        /*
        public void a()
        {
            Wallet.Wallet wallet = new Wallet.Wallet(MnemonicWords);

            Account fromAccount = wallet.GetAccount(10);
            Account toAccount = wallet.GetAccount(8);

            RequestResult<ResponseValue<BlockHash>> blockHash = rpcClient.GetRecentBlockHash();
            Console.WriteLine($"BlockHash >> {blockHash.Result.Value.Blockhash}");

            byte[] tx = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(fromAccount)
                .AddInstruction(SystemProgram.Transfer(fromAccount.PublicKey, toAccount.PublicKey, 10000000))
                .AddInstruction(MemoProgram.NewMemo(fromAccount.PublicKey, "Hello from Sol.Net :)"))
             // .AddInstruction(TokenProgram.SyncNative("PABLIK KI OD ASOSHIJEJTED ACCA");
                .Build(fromAccount);
           

            Console.WriteLine($"Tx base64: {Convert.ToBase64String(tx)}");
            RequestResult<ResponseValue<SimulationLogs>> txSim = rpcClient.SimulateTransaction(tx);
            string logs = Examples.PrettyPrintTransactionSimulationLogs(txSim.Result.Value.Logs);
            Console.WriteLine($"Transaction Simulation:\n\tError: {txSim.Result.Value.Error}\n\tLogs: \n" + logs);
            RequestResult<string> firstSig = rpcClient.SendTransaction(tx);
            Console.WriteLine($"First Tx Signature: {firstSig.Result}");
        }*/
        #region Controls

        public static string RPCField(string currentRPC) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("RPC URL", propLabelStyle);
            var newRPC = EditorGUILayout.TextField(currentRPC, textFieldStyle);
            EditorGUILayout.EndHorizontal();
            return newRPC;
        }

        public static string FileSelectField(
            string labelText,
            string currentPath,
            bool inProject,
            string explorerTitle = null,
            string extension = null
        ) {
            // Layout:
            var basePath = Path.GetDirectoryName(Application.dataPath);
            string newPath = null;
            StaticTextProperty(labelText, currentPath, "Select", delegate {
                if (extension == null) 
                {
                    newPath = EditorUtility.OpenFolderPanel(explorerTitle, basePath, "");
                }
                else 
                {
                    newPath = EditorUtility.OpenFilePanel(explorerTitle, basePath, extension);
                }
            });
            var relativePath = Path.GetRelativePath(basePath, newPath ?? basePath);
            if (newPath == null)
            {
                return currentPath;
            } 
            else if (inProject && relativePath.StartsWith(".."))
            {
                Debug.LogError("Path must be inside the project folder.");
                return currentPath;
            }
            return inProject ? relativePath : newPath;
        }

        public static void StaticTextProperty(
            string label, 
            string value,
            string buttonText = null,
            Action buttonAction = null
        ) {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(label, propLabelStyle);
                if (buttonText != null && buttonAction != null) {
                    if (GUILayout.Button(buttonText, selectButtonStyle)) {
                        buttonAction?.Invoke();
                    }
                }
                GUILayout.Box(value, fileSelectPathStyle);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static int DropdownField(
            string label,
            int value,
            string[] options
        )
        {
            int index;
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(label, propLabelStyle);
                index = EditorGUILayout.Popup(value, options);
            }
            EditorGUILayout.EndHorizontal();
            return index;
        }

        public static int RangeField(
            string label,
            int value,
            int lowerBound,
            int upperBound
        )
        {
            int newValue;
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(label, propLabelStyle);
                newValue = EditorGUILayout.IntSlider(value, lowerBound, upperBound);
            }
            EditorGUILayout.EndHorizontal();
            return newValue;
        }

        public static void Heading(string text, TextAnchor alignment)
        {
            var style = headingLabelStyle;
            style.alignment = alignment;
            GUILayout.Label(text, style);
        }

        #endregion
    }
}
