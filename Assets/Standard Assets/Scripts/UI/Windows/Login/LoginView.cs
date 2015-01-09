using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class LoginView
{
    public GameObject ServerNameTextInput;
    public GameObject LocalhostLabel;
    public GameObject UserNameTextInput;
    public GameObject PasswordTextInput;
    public GameObject HostServerToggle;
    public GameObject LoginButton;
    public GameObject ExitButton;
    public GameObject LoginStatusLabel;
    public GameObject VersionLabel;
    public GameObject StatusScrollView;

    public LoginController loginController { get; set; }

    private InputField m_serverNameTextComponent;
    private InputField m_userNameTextComponent;
    private InputField m_passwordTextComponent;
    private Toggle m_hostServerToggleComponent;
    private Text m_loginStatusLabelComponent;
    private Text m_versionTextComponent;
    private ScrollTextView m_statusScrollView;

	public bool IsLocallyHostedServer()
	{
        return m_hostServerToggleComponent.isOn;
	}

	public string GetServerAddress()
	{
        return !IsLocallyHostedServer() ? m_serverNameTextComponent.text : "localhost";
	}

    public string GetUserName()
    {
        return m_userNameTextComponent.text;
    }

    public string GetPassword()
    {
        return m_passwordTextComponent.text;
    }

    public void Start()
    {
        m_serverNameTextComponent = ServerNameTextInput.GetComponent<InputField>();
        m_userNameTextComponent = UserNameTextInput.GetComponent<InputField>();
        m_passwordTextComponent = PasswordTextInput.GetComponent<InputField>();
        m_hostServerToggleComponent = HostServerToggle.GetComponent<Toggle>();
        m_loginStatusLabelComponent = LoginStatusLabel.GetComponent<Text>();
        m_versionTextComponent = VersionLabel.GetComponent<Text>();
        m_statusScrollView = StatusScrollView.GetComponent<ScrollTextView>();

        // Set the initial state of the host server toggle
        m_hostServerToggleComponent.isOn = false;

        // Hide the login status label until there is status to post
        LoginStatusLabel.SetActive(false);

        // Show/Hide the :"localhost" label based on the host server toggle
        SetLocalhostMode(m_hostServerToggleComponent.isOn);

        // If we're debugging, auto-fill in the password
        m_userNameTextComponent.text = "test";
        m_passwordTextComponent.text = "password";

        m_versionTextComponent.text = "Version: 1.0";
   }

	public void SetLoginStatus(string message)
	{
        if (message.Length > 0)
        {
            m_loginStatusLabelComponent.text = message;
            LoginStatusLabel.SetActive(true);
        }
        else
        {
            LoginStatusLabel.SetActive(false);
        }
	}

    public void LogMessage(string message)
    {
        m_statusScrollView.AppendLine(message);
    }

    public void SetPanelInputsEnabled(bool enabled)
    {
        LoginButton.SetActive(enabled);
        ExitButton.SetActive(enabled);
        HostServerToggle.SetActive(enabled);
    }

    public void SetLocalhostMode(
        bool hostServerLocally)
    {
        // Toggle on/off the "localhost" label
        LocalhostLabel.SetActive(hostServerLocally);
        ServerNameTextInput.SetActive(!hostServerLocally);
    }
}