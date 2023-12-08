using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;

public class FirebaseLoginUI : MonoBehaviour
{

	[Header("RegisterPanel")]
	public GameObject registerNewPanel;
	public TMP_InputField emailField;
	public TMP_InputField passwordField;
	public Button RegisterBtn;
	public Button switchToLogIn;

	[Header("Log in Panel")]
	public GameObject logInPanel;
	public TMP_InputField LogemailField;
	public TMP_InputField LogpasswordField;
	public Button LogInBtn;
	public Button switchToRegister;


	private void Awake()
	{
		RegisterBtn.onClick.AddListener(() => CreateNewUser(emailField.text, passwordField.text));
		LogInBtn.onClick.AddListener(() => LogInUser(LogemailField.text, LogpasswordField.text));
		switchToLogIn.onClick.AddListener(OpenLogInPanel);
		switchToRegister.onClick.AddListener(OpenRegisterPanel);
	}

	public void OpenLogInPanel()
	{
		registerNewPanel.SetActive(false);
		logInPanel.SetActive(true);
	}

	public void OpenRegisterPanel()
	{
		registerNewPanel.SetActive(true);
		logInPanel.SetActive(false);
	}

	public void CreateNewUser(string email, string password)
    {
		FirebaseAuth FirebaseAuth = FirebaseAuth.DefaultInstance;
		FirebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
			if (task.IsCanceled)
			{
				Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
				return;
			}
			if (task.IsFaulted)
			{
				Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				return;
			}

			// Firebase user has been created.
			Firebase.Auth.AuthResult result = task.Result;
			Debug.LogFormat("Firebase user created successfully: {0} ({1})",
				result.User.DisplayName, result.User.UserId);
		});
	}

	public void LogInUser(string email, string password)
	{
		FirebaseAuth FirebaseAuth = FirebaseAuth.DefaultInstance;
		FirebaseAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
			if (task.IsCanceled)
			{
				Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
				return;
			}
			if (task.IsFaulted)
			{
				Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				return;
			}

			AuthResult result = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				result.User.DisplayName, result.User.UserId);
		});
	}
}
