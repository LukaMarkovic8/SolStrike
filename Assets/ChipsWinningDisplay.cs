using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChipsWinningDisplay : MonoBehaviour
{
    public TextMeshProUGUI chipsText;
    void Update()
    {
        chipsText.text = GetAmount();
    }
    public string GetAmount()
    {
        if(Signature.placeFinished == 1)
        {
            return "2.5";
        }
        else if (Signature.placeFinished == 2)
        {
            return "1";
        }
        else if (Signature.placeFinished == 3)
        {
            return "0.3";
        }
        else
        {
            return "0";
        }

        
    }

}


