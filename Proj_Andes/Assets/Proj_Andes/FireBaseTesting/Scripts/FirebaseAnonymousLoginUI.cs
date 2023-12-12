using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FirebaseAnonymousLoginUI : MonoBehaviour
{
	bool correctlyLoggedInFlag = false;
	bool doneInitialization = false;

	public Pool<UsersListItem> currUserBtns;

	[SerializeField] Button goToUserCreationPanel;


	[Header("Create new user panel")]
	public GameObject createNewUserPanel;
	[SerializeField] TMP_InputField nameField;
	[SerializeField] TMP_InputField ageField;
	[SerializeField] TMP_InputField sexField;
	[SerializeField] TMP_InputField cityField;
	[SerializeField] TMP_InputField institutionField;
	[SerializeField] Button createBtn;
	[SerializeField] Button cancelBtn;


	private void Awake()
	{
		cancelBtn.onClick.AddListener(() => createNewUserPanel.SetActive(false));
		createBtn.onClick.AddListener(OnFinishedUserCreation);
		goToUserCreationPanel.onClick.AddListener(OnWantsToCreateNewUser);
	}

	private void Start()
	{
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

		auth.SignInAnonymouslyAsync().ContinueWith(task => {
			if (task.IsCanceled)
			{
				Debug.LogError("SignInAnonymouslyAsync was canceled.");
				return;
			}
			if (task.IsFaulted)
			{
				Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.AuthResult result = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				result.User.DisplayName, result.User.UserId);
		});
	}

	public void Update()
	{
		if (correctlyLoggedInFlag && !doneInitialization)
		{
			InitializeDataList();
			correctlyLoggedInFlag=false;
			doneInitialization = true;
		}
	}

	void InitializeDataList()
	{
		UserDataManager.Instance.LoadDataFromRemoteDataBase();
		var users = UserDataManager.Instance.usersDatas;
		for (int i = 0; i < users.Count; i++) 
		{
			var newBtn = currUserBtns.GetNewItem();
			newBtn.Init(users[i].name);
		}
	}

	void OnWantsToCreateNewUser()
	{
		createNewUserPanel.SetActive(true);
	}

	void OnFinishedUserCreation()
	{
		createNewUserPanel.SetActive(false);
		var newUser = new UserData();
		newUser.name = nameField.text;
		newUser.age = int.Parse(ageField.text);
		newUser.gender = (UserGender)Enum.Parse(typeof(UserGender), sexField.text);
		newUser.city = cityField.text;
		newUser.institution = institutionField.text;

	}

}
