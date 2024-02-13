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
	bool userDeletionInProgress = false;

	string logInFailedWarning = string.Empty;

	public Pool<UsersListItem> userBtnsPool;
	public Dictionary<UsersListItem, string> currBtnsByDataID;

	[SerializeField] Button goToUserCreationPanel;
	[SerializeField] Button goToExistingUserPanel;
	[SerializeField] Transform selectUserContainer;
	[SerializeField] Transform loadingScreen;
	[SerializeField] Transform closeSessionPanel;
	[SerializeField] Transform headerPanel;
	[SerializeField] Button musicBtn;
	[SerializeField] Button wantsToExitSessionBtn;
	[SerializeField] Button exitSessionBtn;
	[SerializeField] Button cancelSessionBtn;
	[SerializeField] TMP_Text userNameHeader;
	[SerializeField] TMP_Text userNameExitPanel;

	[SerializeField] Transform uReadyPanel;
	[SerializeField] Button readyBtb;

	[Header("Search Bar")]
	[SerializeField] Transform searchUserContainer;
	[SerializeField] TMP_InputField searchField;
	[SerializeField] Button confirmUserBtn;
	[SerializeField] UsersListItem selectedUser;

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
	[Space(20)]
	[SerializeField] GameObject checkingInternetPanel;
	[SerializeField] GameObject NoInternetWarningIcon;

	 string logInsuccedID;

	bool continueSelectedFlag;

	private void Awake()
	{
		DatabaseManager.DisableFirebaseOfflineSave();
		loadingScreen.gameObject.SetActive(true);
		selectUserContainer.gameObject.SetActive(true);

        closeSessionPanel.gameObject.SetActive(false);
        wantsToExitSessionBtn.gameObject.SetActive(false);
		uReadyPanel.gameObject.SetActive(false);

		cancelBtn.onClick.AddListener(() => selectUserContainer.gameObject.SetActive(true));
		createBtn.onClick.AddListener(OnFinishedUserCreation);
		wantsToExitSessionBtn.onClick.AddListener(() => closeSessionPanel.gameObject.SetActive(true));
		exitSessionBtn.onClick.AddListener(OnExitSession);
		cancelSessionBtn.onClick.AddListener(() => closeSessionPanel.gameObject.SetActive(false));
        musicBtn.onClick.AddListener(MusicBtn);
        goToExistingUserPanel.onClick.AddListener(OnWantsToAccessExistingUser);
		
		goToUserCreationPanel.onClick.AddListener(OnWantsToCreateNewUser);
		wrongNewUserDataPopUp.onClick.AddListener(() => wrongNewUserDataPopUp.gameObject.SetActive(false));
		logInFailedPopUp.onClick.AddListener(() => logInFailedPopUp.gameObject.SetActive(false));
		afterLogInContinueBtn.onClick.AddListener(OnContinueGameBtnPressed);
		afterLogInNewGameBtn.onClick.AddListener(() => uReadyPanel.gameObject.SetActive(true));

		readyBtb.onClick.AddListener(OnReadyConfirmBtnPressed);

		continueSelectedFlag = false;
		searchField.onValueChanged.AddListener((_) => SearchUser());
		confirmUserBtn.onClick.AddListener(() => OnSelectedUser(selectedUser));

        correctlyLoggedInFlag = false;
		doneInitialization = false;
		userBtnsPool.Init(10);
		currBtnsByDataID = new Dictionary<UsersListItem, string>();
		checkingInternetPanel.SetActive(false);
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
			UserDataManager.Instance.HasInternet = false;
			OnFailedLogIn(taskResult);
			return;
		}

		AuthResult result = taskResult.Result;

		UserDataManager.Instance.HasInternet = true;

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
            loadingScreen.gameObject.SetActive(false);

            Debug.Log("Correctly logged in");
            logInsuccedIDUITxt.SetText(logInsuccedID);
			StartCoroutine(LoadUsers());
			correctlyLoggedInFlag = false;
			doneInitialization = true;
		}

		if(userDeletionInProgress && DatabaseManager.UserDeletionCompleted)
		{
			userDeletionInProgress = false;
			StartCoroutine(LoadUsers());
		}
	}
	void SearchUser()
	{
		Debug.Log("seaarching");
		string searchText = searchField.text;
		int textLenght = searchText.Length;
		int searchElements = 0;
		foreach (var item in currBtnsByDataID)
		{
			searchElements++;
			if(item.Key.label.text.Length >= textLenght)
			{
				if (item.Key.label.text.Contains(searchText, StringComparison.OrdinalIgnoreCase)) item.Key.gameObject.SetActive(true);
				else item.Key.gameObject.SetActive(false);
            }
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
			newBtn.gameObject.SetActive(false);
		}
	}

    public void ShowSelection(UsersListItem userSelected)
    {
		selectedUser = userSelected;
		searchField.text = userSelected.label.text;
		foreach (var item in currBtnsByDataID) item.Key.gameObject.SetActive(false);
    }

	IEnumerator LoadUsers()
	{
		checkingInternetPanel.SetActive(true);
		yield return UserDataManager.Instance.LoadDataFromRemoteDataBaseRoutine();
		Debug.Log("Corretly retrieved users from server");
		checkingInternetPanel.SetActive(false);
		NoInternetWarningIcon.SetActive(!UserDataManager.Instance.HasInternet);
		RebuildUsersList();
	}

	void OnWantsToCreateNewUser()
	{
		createNewUserPanel.SetActive(true);
        selectUserContainer.gameObject.SetActive(false);
    }	
	void OnWantsToAccessExistingUser()
	{
        createNewUserPanel.SetActive(false);
        selectUserContainer.gameObject.SetActive(false);
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
	void OnExitSession()
	{
        closeSessionPanel.gameObject.SetActive(false);
        wantsToExitSessionBtn.gameObject.SetActive(false);
        uReadyPanel.gameObject.SetActive(false);
		afterLogInPanel.gameObject.SetActive(false);

        nameField.text = string.Empty;
        ageField.text = string.Empty;
		gradeField.text = string.Empty;
		sexField.SetValueWithoutNotify(0);
		schoolTypeField.SetValueWithoutNotify(0);
		countryField.text = string.Empty;
        for (int i = 0; i < livingWithToggles.Count; i++) livingWithToggles[i].toggle.isOn = false;

        selectUserContainer.gameObject.SetActive(true);
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
		userDeletionInProgress = true;
	}

	public void OnSelectedUser(UsersListItem data)
	{
		if (data == null) return;
        userNameHeader.text = data.label.text;
        userNameExitPanel.text = data.label.text;

        wantsToExitSessionBtn.gameObject.SetActive(true);

        if (!currBtnsByDataID.TryGetValue(data, out var idFound)) Debug.LogError("Trying to delete a user but was not found on dictionary");
		UserDataManager.Instance.SetCurrUser(idFound);
		var storedCheckPoint = UserDataManager.CurrUser.CheckPointIdx;
		afterLogInPanel.SetActive(true);
		afterLogInContinueBtn.gameObject.SetActive(storedCheckPoint != -1);
	}


	void MusicBtn()
	{
		if (AudioListener.volume == 1) AudioListener.volume = 0;
		else if (AudioListener.volume == 0) AudioListener.volume = 1;
	}

	public void OnContinueGameBtnPressed()
	{
		continueSelectedFlag = true;
		uReadyPanel.gameObject.SetActive(true);
	}

	public void OnReadyConfirmBtnPressed()
	{
		UserDataManager.CurrTestID = Guid.NewGuid().ToString();
		DatabaseManager.AddPendingUserData(UserDataManager.CurrUser);
		if (continueSelectedFlag)
		{
			var targetSequence = GameSequencesList.Instance.gameSequences[UserDataManager.CurrUser.CheckPointIdx];
			if (targetSequence is MinigameGroups group)
			{
				group.SetItemsPlayedData(UserDataManager.CurrUser.itemsPlayedIdxs);
			}
			DialoguesDisplayerUI.CheckPointTreeToConsume = UserDataManager.CurrUser.narrativeNavCheckPointsNodes;
			GameSequencesList.Instance.GoToSequenceIdx(UserDataManager.CurrUser.CheckPointIdx, UserDataManager.CurrUser.CheckPointSubIdx);
		}
		else
		{
			UserDataManager.CurrUser.myCollectionMonsters.Clear();
			UserDataManager.CurrUser.Coins = 10;
			GameSequencesList.Instance.GoToNextSequence();
		}
	}

}
