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

	public Pool<UsersListItem> userBtnsPool;
	public Dictionary<UsersListItem, string> currBtnsByDataID;

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
	[SerializeField] Button wrongNewUserDataPopUp;


	private void Awake()
	{
		cancelBtn.onClick.AddListener(() => createNewUserPanel.SetActive(false));
		createBtn.onClick.AddListener(OnFinishedUserCreation);
		goToUserCreationPanel.onClick.AddListener(OnWantsToCreateNewUser);
		wrongNewUserDataPopUp.onClick.AddListener(() => wrongNewUserDataPopUp.gameObject.SetActive(false));
		correctlyLoggedInFlag = false;
		doneInitialization = false;
		userBtnsPool.Init(10);
		currBtnsByDataID = new Dictionary<UsersListItem, string>();
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

			correctlyLoggedInFlag = true;
		});
	}

	public void Update()
	{
		if (correctlyLoggedInFlag && !doneInitialization)
		{
			UserDataManager.Instance.LoadDataFromRemoteDataBase();
			RebuildUsersList();
			correctlyLoggedInFlag = false;
			doneInitialization = true;
		}
	}

	void RebuildUsersList()
	{
		currBtnsByDataID.Clear();
		userBtnsPool.RecycleAll();
		var users = UserDataManager.Instance.usersDatas;
		for (int i = 0; i < users.Count; i++) 
		{
			var newBtn = userBtnsPool.GetNewItem();
			newBtn.Init(users[i].name, this);
			currBtnsByDataID.Add(newBtn, users[i].id);
		}
	}

	void OnWantsToCreateNewUser()
	{
		createNewUserPanel.SetActive(true);
	}

	void OnFinishedUserCreation()
	{
		var newUser = new UserData();
		newUser.name = nameField.text;
		newUser.age = int.TryParse(ageField.text, out var result)? result : -1;
		newUser.gender = Enum.TryParse<UserGender>(sexField.text, out var found)? found : UserGender.NONE;
		newUser.city = cityField.text;
		newUser.institution = institutionField.text;

		var validData = true;
		validData &= !string.IsNullOrEmpty(newUser.name);
		validData &= newUser.age != -1;
		validData &= newUser.gender != UserGender.NONE;
		validData &= !string.IsNullOrEmpty(newUser.city);
		validData &= !string.IsNullOrEmpty(newUser.institution);

		if(!validData) wrongNewUserDataPopUp.gameObject.SetActive(true);
		else
		{
			newUser.id = Guid.NewGuid().ToString();
			UserDataManager.Instance.RegisterNewUser(newUser);
			createNewUserPanel.SetActive(false);
			RebuildUsersList();
		}

	}

	public void RemoveUserOfBtn(UsersListItem item)
	{
		if (!currBtnsByDataID.TryGetValue(item, out var idFound)) Debug.LogError("Trying to delete a user but was not found on dictionary");
		UserDataManager.Instance.RemoveUser(idFound);
		RebuildUsersList();
	}

}
