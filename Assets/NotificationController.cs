using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationController : MonoBehaviour
{
    public GameObject notificationHolder;
    public Image notificationImage;
    private float duration = 5f;

    public GameObject warningHolder;


    public void ShowNotification(Sprite sprite)
    {
        notificationHolder.SetActive(true);
        notificationImage.sprite = sprite;  
        StartCoroutine(HideNotificationAfterDelay(duration));
    }


    public void ShowWarning()
    {
        warningHolder.SetActive(true);
        StartCoroutine(HideNotificationAfterDelay(duration));
    }

    IEnumerator HideNotificationAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        notificationHolder.SetActive(false);
        warningHolder.SetActive(false);
    }
}
