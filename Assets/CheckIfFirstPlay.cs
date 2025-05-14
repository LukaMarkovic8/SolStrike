using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIfFirstPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Update()
    {
        Debug.Log("CheckIfFirstPlay");
        if (!Signature.isFirstTime)
        {
            Debug.Log("CheckIfFirstPlay: Not first time playing");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("CheckIfFirstPlay: First time playing");
        }

    }

 
}
