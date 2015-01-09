using LitJson;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class LoginModel
{
    private LoginController m_loginController;

    private string m_requestUsername;
    private string m_requestPassword;

    public bool LoginRequestPending { get; private set; }

    public LoginModel(LoginController loginController)
    {
        m_loginController = loginController;
        LoginRequestPending = false;
    }

    public void Update()
    {
        if (LoginRequestPending)
        {
            if (m_requestUsername != "test")
            {
                m_loginController.OnLoginFailed("Bad Username");
            }
            else if (m_requestPassword != "password")
            {
                m_loginController.OnLoginFailed("Bad Password");
            }
            else
            {
                m_loginController.OnLoginSucceeded();
            }

            LoginRequestPending = false;
        }
    }

    public void RequestLogin(string uname, string password)
    {
        // Fake an asynchronous login request
        if (!LoginRequestPending)
        {
            m_requestUsername = uname;
            m_requestPassword = password;

            LoginRequestPending = true;
        }
    }
}
