using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClaimButtonController : MonoBehaviour
{
    public GameObject claimButton;

    // Update is called once per frame
    void Update()
    {
        if (Signature.UnclaimedChipsAmount > 0)
        {
      
            claimButton.SetActive(true);
        }
        else
        {
            claimButton.SetActive(false);
        }
    }
}
