using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;

public class FirebaseLoginUI : MonoBehaviour
{

	public TMP_InputField emailField;
	public TMP_InputField passwordField;

	public Button enterBtn;


	private void Awake()
	{
		enterBtn.onClick.AddListener(() => CreateNewUser(emailField.text, passwordField.text));
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
}
