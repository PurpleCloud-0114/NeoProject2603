
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SignupController : MonoBehaviour
{
    [SerializeField] private GameObject _login;
    [SerializeField] private LoginController _logincontroller;
    [SerializeField] private GameObject _signup_ob;
    [SerializeField] private TMP_InputField _name_input;
    [SerializeField] private TMP_InputField _pw_input;
    [SerializeField] private TMP_Text _log_text;
    [SerializeField] private Button _signup_button;
    private void Start()
    {
        LogTextViewing(string.Empty);
        _signup_button.onClick.AddListener(SignupEvent);
    }

    private void LogTextViewing(string text)
    {
        _log_text.text = text;
    }
    public void SignupEvent()
    {
        if (_name_input.text.Equals(string.Empty) || _pw_input.text.Equals(string.Empty))
        {
            LogTextViewing("Name, Nic or PassWord check Please");
            return;
        }

        if (SQLManager.Instance.SignupIDCheck(_name_input.text))
        {
            LogTextViewing("THis Name is already uesed");
            return;
        }
        
        if (SQLManager.Instance.Signup(_name_input.text, _pw_input.text))
        {
           
            _name_input.text = string.Empty;
            _pw_input.text = string.Empty;
            
            _login.gameObject.SetActive(true);
            _logincontroller.LogTextViewing("생성되었습니다.");
            _logincontroller.LogTextViewing("Success Sign UP.");
            _signup_ob.SetActive(false);
        }
        else
        {
            LogTextViewing("이름 또는 비밀번호를 확인 해주세요");
            LogTextViewing("Name or PassWord check Please");
        }
    }
}
