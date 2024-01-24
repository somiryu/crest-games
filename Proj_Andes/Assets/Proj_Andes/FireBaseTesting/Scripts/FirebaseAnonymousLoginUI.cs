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
	bool checkUserList = false;

	string logInFailedWarning = string.Empty;

	public Pool<UsersListItem> userBtnsPool;
	public Dictionary<UsersListItem, string> currBtnsByDataID;

	[SerializeField] Button goToUserCreationPanel;


	[Header("Create new user panel")]
	public GameObject createNewUserPanel;
	[SerializeField] TMP_InputField nameField;
	[SerializeField] TMP_InputField ageField;
    [SerializeField] TMP_InputField gradeField;
    [SerializeField] TMP_Dropdown sexField;
    [SerializeField] TMP_Dropdown schoolTypeField;
    [SerializeField] TMP_InputField countryField;
	[SerializeField] List<LivingWithType> livingWithToggles;
	[SerializeField] Button createBtn;
	[SerializeField] Button cancelBtn;
	[SerializeField] Button wrongNewUserDataPopUp;
	[SerializeField] Button logInFailedPopUp;
	[SerializeField] TMP_Text logInFailTxt;
	[SerializeField] TMP_Text logInsuccedIDUITxt;

	[Header("After Log in Panel")]
	[SerializeField] GameObject afterLogInPanel;
	[SerializeField] Button afterLogInContinueBtn;
	[SerializeField] Button afterLogInNewGameBtn;

	 string logInsuccedID;

	private void Awake()
	{
		cancelBtn.onClick.AddListener(() => createNewUserPanel.SetActive(false));
		createBtn.onClick.AddListener(OnFinishedUserCreation);
		goToUserCreationPanel.onClick.AddListener(OnWantsToCreateNewUser);
		wrongNewUserDataPopUp.onClick.AddListener(() => wrongNewUserDataPopUp.gameObject.SetActive(false));
		logInFailedPopUp.onClick.AddListener(() => logInFailedPopUp.gameObject.SetActive(false));
		afterLogInContinueBtn.onClick.AddListener(OnContinueGameBtnPressed);
		afterLogInNewGameBtn.onClick.AddListener(OnNewGameBtnPressed);
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

		if (!DatabaseManager.userListDone && !checkUserList) { 
			UserDataManager.Instance.LoadDataFromRemoteDataBase();
            checkUserList = true;
        }

        if (correctlyLoggedInFlag && !doneInitialization && DatabaseManager.userListDone)
		{
			Debug.Log("Correctly logged in");

            logInsuccedIDUITxt.SetText(logInsuccedID);
			RebuildUsersList();
			correctlyLoggedInFlag = false;
			doneInitialization = true;
			checkUserList = false;
		}		
	}

	void RebuildUsersList()
	{
		currBtnsByDataID.Clear();
		userBtnsPool.RecycleAll();
		var users = UserDataManager.Instance.usersDatas;
		Debug.Log("Rebuilding user datas: user amounts found: " + users.Count);
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
		newUser.age = int.TryParse(ageField.text, out var ageResult)? ageResult : -1;
        newUser.grade = int.TryParse(gradeField.text, out var gradeResult) ? gradeResult : -1;
		newUser.gender = Enum.TryParse<UserGender>(sexField.options[sexField.value].text, true ,out var genderFound)? genderFound : UserGender.NONE;
		newUser.schoolType = Enum.TryParse<UserSchoolType>(schoolTypeField.options[schoolTypeField.value].text, true ,out var schoolFound)? schoolFound : UserSchoolType.NONE;
		newUser.country = countryField.text;
		newUser.livingWith = GetUserLivingWith();

		var validData = true;
		validData &= !string.IsNullOrEmpty(newUser.name);
		validData &= newUser.age != -1;
        validData &= newUser.grade != -1;
        validData &= newUser.gender != UserGender.NONE;
        validData &= newUser.schoolType != UserSchoolType.NONE;
        validData &= !string.IsNullOrEmpty(newUser.country);
		validData &= newUser.livingWith != UserLivingWith.NONE;
		

		if(!validData) wrongNewUserDataPopUp.gameObject.SetActive(true);
		else
		{
			newUser.id = Guid.NewGuid().ToString();
			UserDataManager.Instance.RegisterNewUser(newUser);
			createNewUserPanel.SetActive(false);
			RebuildUsersList();
		}
	}

	public UserLivingWith GetUserLivingWith()
	{
		var currUserLivingWith = UserLivingWith.NONE;

		for (int i = 0; i < livingWithToggles.Count; i++)
		{
			if (livingWithToggles[i].GetValue())
				currUserLivingWith |= livingWithToggles[i].livingWithType;			
		}
		return currUserLivingWith;
	}

	public void RemoveUserOfBtn(UsersListItem item)
	{
		if (!currBtnsByDataID.TryGetValue(item, out var idFound)) Debug.LogError("Trying to delete a user but was not found on dictionary");
		UserDataManager.Instance.RemoveUser(idFound);
		RebuildUsersList();
	}

	public void OnSelectedUser(UsersListItem data)
	{
		if (!currBtnsByDataID.TryGetValue(data, out var idFound)) Debug.LogError("Trying to delete a user but was not found on dictionary");
		UserDataManager.Instance.SetCurrUser(idFound);
		var storedCheckPoint = UserDataManager.CurrUser.CheckPointIdx;
		afterLogInPanel.SetActive(true);
		afterLogInContinueBtn.gameObject.SetActive(storedCheckPoint != -1);
	}

	public void OnContinueGameBtnPressed()
	{
		var targetSequence = GameSequencesList.Instance.gameSequences[UserDataManager.CurrUser.CheckPointIdx];
        if (targetSequence is MinigameGroups group)
        {
			group.SetItemsPlayedData(UserDataManager.CurrUser.itemsPlayedIdxs);
        }
		DialoguesDisplayerUI.CheckPointTreeToConsume = UserDataManager.CurrUser.narrativeNavCheckPointsNodes;
        GameSequencesList.Instance.GoToSequenceIdx(UserDataManager.CurrUser.CheckPointIdx, UserDataManager.CurrUser.CheckPointSubIdx);
	}

	void OnNewGameBtnPressed()
	{
		GameSequencesList.Instance.GoToNextSequence();
	}
}
