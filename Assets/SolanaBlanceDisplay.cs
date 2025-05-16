using System;
using System.Collections;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.Ocsp;
using TMPro;
using UnityEngine;

public class SolanaBlanceDisplay : MonoBehaviour
{
   public TextMeshProUGUI balanceText;

    // Update is called once per frame
    void Update()
    {
        balanceText.text = "Balance: <color=white>" + Math.Round(Signature.SolanaBalance, 2).ToString() + " SOL";
    }
}
