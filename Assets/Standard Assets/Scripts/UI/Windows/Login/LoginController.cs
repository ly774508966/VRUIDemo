using UnityEngine;
using System.Collections;
using LitJson;

[RequireComponent(typeof(AudioSource))]
public class LoginController : MonoBehaviour 
{
    public LoginView loginView;
    public AudioClip successSound;

    private LoginModel m_loginModel;

	// Use this for initialization
	public void Start () 
    {
        m_loginModel = new LoginModel(this);
        
        loginView.loginController = this;
		loginView.Start();
	}

	// Update is called once per frame
    public void Update() 
    {
        m_loginModel.Update();
	}


    public void OnLoginClicked()
    {
		// Set the server address before attempting login
		string serverAddress= loginView.GetServerAddress().Trim();

		if (serverAddress.Length > 0)
		{
            loginView.SetPanelInputsEnabled(false);
            loginView.SetLoginStatus("Authenticating...");
            
            m_loginModel.RequestLogin(loginView.GetUserName(), loginView.GetPassword());
		}
		else
		{
			loginView.SetLoginStatus("Please specify a server address");
		}
    }

    public void OnExitClicked()
    {
        audio.PlayOneShot(successSound, 1.0f);

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
         Application.Quit();
        #endif
    }

    public void OnLoginSucceeded()
    {
        loginView.SetLoginStatus("Success!");
        loginView.SetPanelInputsEnabled(true);
        loginView.LogMessage("Login Succeeded!");

        audio.PlayOneShot(successSound, 1.0f);
    }		

	public void OnLocalHostToggled(
        bool ignored)
	{
        // See if the toggle is set in the UI
        bool hostServerLocally = loginView.IsLocallyHostedServer();

        // Show/Hide the "localhost" label
        loginView.SetLocalhostMode(hostServerLocally);

        // Start up or shutdown the server
		if (hostServerLocally)
		{
            loginView.LogMessage("Start Server!");
            OnServerStartupComplete();
		}
		else if (!hostServerLocally)
		{
            loginView.LogMessage("Stop Server!");
		}
	}

	public void OnServerStartupComplete()
	{
		audio.PlayOneShot(successSound, 1.0f);
	}
	
    public void OnLoginFailed(string reason)
    {
		loginView.SetLoginStatus(reason);
        loginView.SetPanelInputsEnabled(true);
        loginView.LogMessage("Login Failed: " + reason);
    }
}
