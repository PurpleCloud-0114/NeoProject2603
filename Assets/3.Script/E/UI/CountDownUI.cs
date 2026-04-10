
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDownUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text countdownText;
    void Awake()
    {
        Hide();
    }

    public void SetTime(int time)
    {
        panel.SetActive(true);
        countdownText.text = time.ToString();
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
