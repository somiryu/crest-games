using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase.Auth;
using System.Threading.Tasks;

public class FirebaseAnonymousLoginUI : MonoBehaviour
{
	bool correctlyLoggedInFlag = false;
	bool doneInitialization = false;

	string logInFailedWarning = string.Empty;

	public Pool<UsersListItem> userBtnsPool;
	public Dictionary<UsersListItem, string> currBtnsByDataID;

	[SerializeField] Button goToUserCreationPanel;


	[Header("Create new user panel")]
	public GameObject createNewUserPanel;
	[SerializeField] TMP_InputField nameField;
	[SerializeField] TMP_InputField ageField;
	[SerializeField] TMP_Dropdown sexField;
	[SerializeField] TMP_InputField cityField;
	[SerializeField] TMP_InputField institutionField;
	[SerializeField] Button createBtn;
	[SerializeField] Button cancelBtn;
	[SerializeField] Button wrongNewUserDataPopUp;
	[SerializeField] Button logInFailedPopUp;
	[SerializeField] TMP_Text logInFailTxt;
	[SerializeField] TMP_Text logInsuccedIDUITxt;
	 string logInsuccedID;



	private void Awake()
	{
		cancelBtn.onClick.AddListener(() => createNewUserPanel.SetActive(false));
		createBtn.onClick.AddListener(OnFinishedUserCreation);
		goToUserCreationPanel.onClick.AddListener(OnWantsToCreateNewUser);
		wrongNewUserDataPopUp.onClick.AddListener(() => wrongNewUserDataPopUp.gameObject.SetActive(false));
		logInFailedPopUp.onClick.AddListener(() => logInFailedPopUp.gameObject.SetActive(false));
		correctlyLoggedInFlag = false;
		doneInitialization = false;
		userBtnsPool.Init(10);
		currBtnsByDataID = new Dictionary<UsersListItem, string>();
	}

	private void Start()
	{
		FirebaseAuth auth = FirebaseAuth.DefaultInstance;
		if(auth.CurrentUser != null)
		{
			Debug.Log("Already signed in: " + auth.CurrentUser.UserId);
			logInsuccedID = ("Firebase ID:" + auth.CurrentUser.UserId);
			correctlyLoggedInFlag = true;
			return;
		}
		auth.SignInAnonymouslyAsync().ContinueWith(OnSingingResult);
	}

	void OnSingingResult(Task<AuthResult> taskResult)
	{
		if (taskResult.IsCanceled)
		{
			Debug.LogError("SignInAnonymouslyAsync was canceled.");
			return;
		}
		if (taskResult.IsFaulted)
		{
			OnFailedLogIn(taskResult);
			return;
		}

		AuthResult result = taskResult.Result;

		Debug.LogFormat("User signed in successfully: {0} ({1})",
			result.User.DisplayName, result.User.UserId);

		logInsuccedID = ("Firebase ID:" + result.User.UserId);
		correctlyLoggedInFlag = true;
	}

	void OnFailedLogIn(Task<AuthResult> taskResult)
	{
		var errMsg = GetErrorMessage(taskResult.Exception.InnerExceptions[0]);
		logInFailedWarning = "Log in failed: " + errMsg;
		Debug.LogError("SignInAnonymouslyAsync encountered an error: " + errMsg);
	}

	public static string GetErrorMessage(Exception exception)
	{
		Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
		if (firebaseEx != null)
		{
			var errorCode = (AuthError)firebaseEx.ErrorCode;
			return errorCode.ToString();
		}
		return exception.ToString();
	}


	public void Update()
	{
		if (!string.IsNullOrEmpty(logInFailedWarning))
		{
			logInFailedPopUp.gameObject.SetActive(true);
			logInFailTxt.SetText(logInFailedWarning);
			logInFailedWarning = string.Empty;
		}

		if (correctlyLoggedInFlag && !doneInitialization)
		{
			Debug.Log("Correctly logged in");
			logInsuccedIDUITxt.SetText(logInsuccedID);
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
		newUser.gender = Enum.TryParse<UserGender>(sexField.options[sexField.value].text, true ,out var found)? found : UserGender.NONE;
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
