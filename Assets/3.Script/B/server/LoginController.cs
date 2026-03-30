using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _name_input;
    [SerializeField] private TMP_InputField _pw_input;

    [SerializeField] private TMP_Text _log_text;
    [SerializeField] private Button _login_button;

    private void Start()
    {
        _login_button.onClick.AddListener(LoginEvent);
    }

    private void LogTextViewing(string text) { _log_text.text = text; }

    public void LoginEvent()
    {
        if(_name_input.text.Equals(string.Empty)|| _pw_input.text.Equals(string.Empty))
        {
            LogTextViewing("이름이나 비밀번호를 입력하세요");
            return;
        }

        if (SQLManager.Instance.Login(_name_input.text, _pw_input.text))
        {
            GameObject manager = NetworkManager.singleton.gameObject;
            //if(manager.TryGetComponent(out ServerChecker checker))
            //{
            //    checker.StartClient();
            //}
            gameObject.SetActive(false);
        }
        else
        {
            LogTextViewing("아이디와 비밀번호를 입력하세요");
        }
    }
}
