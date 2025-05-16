using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointsDisplay : MonoBehaviour
{

    public TextMeshProUGUI text;


    // Update is called once per frame
    void Update()
    {
        decimal originalDecimal = decimal.Parse(Signature.GamerData.points);
        text.text = "Points: <color=white>" + Math.Round(originalDecimal, 2).ToString();
    }
}
