using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchCostDisplay : MonoBehaviour
{

    public TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        text.text = "Match Cost: <color=white>" + Signature.matchCost.ToString()+" Sol";
    }
}
