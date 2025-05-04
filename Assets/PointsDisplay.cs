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
        text.text ="POINTS:"+ Signature.GamerData.points.ToString();
    }
}
