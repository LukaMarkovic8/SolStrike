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
        chipsText.text ="STRIKE CHIPS:"+Signature.StandardChipsAmount.ToString();
    }
}
