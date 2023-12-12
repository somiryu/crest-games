using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;
using System;

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
	public Toggle rememberPasswordToggle;

	private string emailPrefID = "Email";
	private string rememberPasswordPrefID = "RememberPassword";
	private string passwordPrefID = "password";

	bool onCorrectlyLoggegFlag = false;
	bool logFailedFlag = false;
	string currLogUserID = "";

	IEnumerator currLogConfirmRoutine;

	private void Awake()
	{
		RegisterBtn.onClick.AddListener(OnRegisterBtnPressed);
		LogInBtn.onClick.AddListener(OnLogInBtnPressed);
		switchToLogIn.onClick.AddListener(OpenLogInPanel);
		switchToRegister.onClick.AddListener(OpenRegisterPanel);

		rememberPasswordToggle.onValueChanged.AddListener(
			(state) =>
			{
				if (rememberPasswordToggle.isOn)
				{
					PlayerPrefs.SetInt(rememberPasswordPrefID, 1);
				}
				else
				{
					PlayerPrefs.SetString(passwordPrefID, "");
					PlayerPrefs.SetInt(rememberPasswordPrefID, -1);
				}
			}
			);

		UserDataManager.Init();

		onCorrectlyLoggegFlag = false;
		logFailedFlag = false;
		currLogUserID = "";

		var savedEmail = PlayerPrefs.GetString(emailPrefID);
		if (!string.IsNullOrEmpty(savedEmail))
		{
			LogemailField.SetTextWithoutNotify(savedEmail);
		}
		var savePassword = PlayerPrefs.GetInt(rememberPasswordPrefID) == 1;
		if (savePassword)
		{
			var passwordSaved = PlayerPrefs.GetString(passwordPrefID);
			if (!string.IsNullOrEmpty(passwordSaved))
			{
				LogpasswordField.SetTextWithoutNotify(passwordSaved);
			}
		}
		rememberPasswordToggle.isOn = savePassword;
	}

	void OnLogInBtnPressed()
	{
		LogInUser(LogemailField.text, LogpasswordField.text);
		if (currLogConfirmRoutine == null)
		{
			currLogConfirmRoutine = CorrectlyLoggedRoutine();
			StartCoroutine(currLogConfirmRoutine);
		}
	}

	void OnRegisterBtnPressed()
	{
		CreateNewUser(emailField.text, passwordField.text);
		if (currLogConfirmRoutine == null)
		{
			currLogConfirmRoutine = CorrectlyLoggedRoutine();
			StartCoroutine(currLogConfirmRoutine);
		}
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
				logFailedFlag = true;
				return;
			}
			if (task.IsFaulted)
			{
				Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				logFailedFlag = true;
				return;
			}

			// Firebase user has been created.
			Firebase.Auth.AuthResult result = task.Result;
			Debug.LogFormat("Firebase user created successfully: {0} ({1})",
				result.User.DisplayName, result.User.UserId);

			onCorrectlyLoggegFlag = true;
		});
	}

	public void LogInUser(string email, string password)
	{
		FirebaseAuth FirebaseAuth = FirebaseAuth.DefaultInstance;
		FirebaseAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
			if (task.IsCanceled)
			{
				Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
				logFailedFlag = true;
				return;
			}
			if (task.IsFaulted)
			{
				Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				logFailedFlag = true;
				return;
			}

			AuthResult result = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				result.User.DisplayName, result.User.UserId);

			onCorrectlyLoggegFlag = true;
			currLogUserID = result.User.UserId;
		});
	}

	void OnCorrectlyLogIn(string email, string ID)
	{
		PlayerPrefs.SetString(emailPrefID, LogemailField.text);
		if (rememberPasswordToggle.isOn)
		{
			PlayerPrefs.SetString(passwordPrefID, LogpasswordField.text);
		}
		PlayerPrefs.Save();
		UserDataManager.Instance.SetCurrUser(email, ID);
	}

	IEnumerator CorrectlyLoggedRoutine()
	{
		while (!onCorrectlyLoggegFlag && !logFailedFlag) yield return null;

		if (onCorrectlyLoggegFlag) OnCorrectlyLogIn(LogemailField.text, currLogUserID);

		logFailedFlag = false;
		onCorrectlyLoggegFlag = false;
		currLogConfirmRoutine = null;
	}
}
