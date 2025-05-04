using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SolanaBlanceDisplay : MonoBehaviour
{
   public TextMeshProUGUI balanceText;

    // Update is called once per frame
    void Update()
    {
        balanceText.text = "Balance: " + Signature.SolanaBalance.ToString() + " SOL";
    }
}
