using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReservedChipsDisplay : MonoBehaviour
{
    public TextMeshProUGUI reservedChipsText;
    // Update is called once per frame
    void Update()
    {
        reservedChipsText.text = "Reserved Chips: <color=white>" + Signature.GamerData.reservedChips.ToString();
    }
}
