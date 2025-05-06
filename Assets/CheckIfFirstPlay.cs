using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIfFirstPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Signature.isFirstTime)
        {
            gameObject.SetActive(false);
        }
    }
}
