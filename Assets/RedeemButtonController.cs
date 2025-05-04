using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedeemButtonController : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject redeemButton;
    // Update is called once per frame
    void Update()
    {
        if (Signature.StandardChipsAmount < 1)
        {             // Redeem button clicked
            // Call the redeem function from the Signature class
            redeemButton.SetActive(false);
        }
        else
        {
            redeemButton.SetActive(true);
        }
    }
}
