using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM : MonoBehaviour
{
    [SerializeField] private bool is_lobby = true;
    private void Start()
    {
        if (is_lobby)
        {
           AudioManager.Instance.PlayBGM("Title");
        }
        else
        {
            AudioManager.Instance.PlayBGM("InGame");
        }
    }
}
