using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnclaimedChipsDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI unclaimedChipsText;

    // Update is called once per frame
    void Update()
    {
        unclaimedChipsText.text = "Unclaimed Chips: <color=white>" + Signature.UnclaimedChipsAmount.ToString();
    }
}
