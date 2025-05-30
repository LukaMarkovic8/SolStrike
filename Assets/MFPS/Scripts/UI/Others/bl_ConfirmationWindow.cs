using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace MFPS.Runtime.UI
{
    public class bl_ConfirmationWindow : MonoBehaviour
    {

        public TMP_Dropdown dropdown;

        public Text descriptionText;
        public TextMeshProUGUI descriptionTextTMP;
        public TextMeshProUGUI confirmationTextTMP;
        public GameObject content;

        [Header("Events")]
        public bl_EventHandler.UEvent onConfirm;
        public bl_EventHandler.UEvent onCancel;

        private Action callback;
        private Action cancelCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="onAccept"></param>
        public void AskConfirmation(string description, Action onAccept, Action onCancel = null, bool isServer = false)
        {
            isFromServer = isServer;
            callback = onAccept;
            cancelCallback = onCancel;
            if (!string.IsNullOrEmpty(description))
            {
                if (descriptionText != null) descriptionText.text = description;
                if (descriptionTextTMP != null) descriptionTextTMP.text = description;
            }

            content.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="onAccept"></param>
        public void ShowMessage(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                if (descriptionText != null) descriptionText.text = description;
                if (descriptionTextTMP != null) descriptionTextTMP.text = description;
            }

            content.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void SetConfirmationText(string text)
        {
            if (confirmationTextTMP != null) confirmationTextTMP.text = text;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Confirm()
        {
            callback?.Invoke();
            onConfirm?.Invoke();
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        bool isFromServer = false;
        public void Cancel()
        {
          // Debug.Log("isFromServer:  " + isFromServer.ToString());

            if (isFromServer)
            {
            //    Debug.Log(PhotonNetwork.CloudRegion + "  " + PhotonNetwork.CloudRegion.ToString());
                if (PhotonNetwork.CloudRegion == "eu")
                {

                    dropdown.captionText.text = "EU - AMSTERDAM";
                    dropdown.value = 0;
                }
                else if (PhotonNetwork.CloudRegion == "us")
                {
                    dropdown.value = 1;

                    dropdown.captionText.text = "NA - US";
                }
                else if (PhotonNetwork.CloudRegion == "usw")
                {
                    dropdown.captionText.text = "NA - USW";
                }
                else if (PhotonNetwork.CloudRegion == "as")
                {
                    dropdown.value = 2;
                    dropdown.captionText.text = "AS - SINGAPORE";
                }
            }
            callback = null;
            cancelCallback?.Invoke();
            onCancel?.Invoke();
            cancelCallback = null;
            content.SetActive(false);
        }
    }
}