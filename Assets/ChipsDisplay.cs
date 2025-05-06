using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;

public class ChipsDisplay : MonoBehaviour
{
    public TextMeshProUGUI chipsText;
    // Update is called once per frame
    void Update()
    {
        chipsText.text = "Strike Chips: " + Math.Round(Signature.StandardChipsAmount, 2).ToString();
    }
}
