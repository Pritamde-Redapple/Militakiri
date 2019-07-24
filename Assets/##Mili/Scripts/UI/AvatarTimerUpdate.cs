using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AvatarTimerUpdate : MonoBehaviour
{
    Image avatarTimerImage;

    private void Awake()
    {
        avatarTimerImage = GetComponent<Image>();
        avatarTimerImage.color = Color.yellow;
    }

    public void UpdateFill(float t)
    {
        if (!avatarTimerImage)
            return;
        avatarTimerImage.color = Color.green;
        avatarTimerImage.fillAmount = t;
    }

    public void ResetFill()
    {
        avatarTimerImage.fillAmount = 1;
        avatarTimerImage.color = Color.yellow;
    }
}
