using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameValidityCheck : MonoBehaviour
{
    public GameObject ButtonHolder;
    public GameObject WarningHolder;
    public TMP_InputField inputField;

    private void Start()
    {
        inputField.onValueChanged.AddListener(ChnagedInput);
    }
    public void ChnagedInput(string value)
    {
        if (value.Length < 17)
        {
            ButtonHolder.SetActive(true);
            WarningHolder.SetActive(false);

        }
        else
        {
            ButtonHolder.SetActive(false);
            WarningHolder.SetActive(true);
        }
    }
}
