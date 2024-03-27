using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase.Auth;
using System.Threading.Tasks;
using Unity.VisualScripting;

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
	[SerializeField] Sprite musicBtnActive;
	[SerializeField] Sprite musicBtnInactive;
	[SerializeField] Button wantsToExitSessionBtn;
	[SerializeField] Button exitSessionBtn;
	[SerializeField] Button cancelSessionBtn;
	[SerializeField] TMP_Text userNameHeader;
	[SerializeField] TMP_Text userNameExitPanel;



	[Header("Search Bar")]
	[SerializeField] Transform searchUserContainer;
	[SerializeField] TMP_InputField searchField;
	[SerializeField] Button confirmUserBtn;
	[SerializeField] UsersListItem selectedUser;
	[SerializeField] Button cancelSearchBarBtn;
	[SerializeField] Button removeSelectedUser;
	[SerializeField] GameObject selectedUserLabelGO;
	[SerializeField] TMP_Text selectedUserLabel;


	[Header("Create new user panel")]
	public GameObject createNewUserPanel;
	[SerializeField] TMP_InputField nameField;
    [SerializeField] TMP_Dropdown ageField;
    [SerializeField] TMP_Dropdown gradeField;
    [SerializeField] TMP_Dropdown sexField;
    [SerializeField] TMP_Dropdown schoolTypeField;
    [SerializeField] TMP_InputField countryField;
	[SerializeField] List<LivingWithType> livingWithToggles;
	[SerializeField] Button createBtn;
	[SerializeField] Button cancelBtn;
	[SerializeField] Button wrongNewUserDataPopUp;
	[SerializeField] TMP_Text wrongNewUserDataLabelPopUp;
	[SerializeField] Button logInFailedPopUp;
	[SerializeField] TMP_Text logInFailTxt;
	[SerializeField] TMP_Text logInsuccedIDUITxt;

	[Header("After Log in Panel")]
	[SerializeField] GameObject afterLogInPanel;
	[SerializeField] Button afterLogInContinueBtn;
	[SerializeField] Button afterLogInNewGameBtn;
	[SerializeField] Button cancelAfterLogBtn;
	[Space(20)]
	[SerializeField] GameObject checkingInternetPanel;
	[SerializeField] GameObject NoInternetWarningIcon;

	[Header("COnfirmStart Panel")]
	[SerializeField] Transform uReadyPanel;
	[SerializeField] Button readyBtb;
	[SerializeField] Button cancelConfirmPanel;
	[SerializeField] TMP_Text contWelcomeM; 
	[SerializeField] TMP_Text contWelcomeF; 

	[Header("No Internet Connection Warnings")]
	[SerializeField] Button noInternetConnectionPopUpOkBtn;
	[SerializeField] GameObject noInternetConnectionPopUp;

	[Header("Data synced warning")]
	[SerializeField] Button dataSyncedPopUpOkBtn;
	[SerializeField] GameObject dataSyncedPopUp;
	[SerializeField] TMP_Text dataSyncedLabel;

	[Header("Insert Code screen")]
	[SerializeField] GameObject insertCodePanel;
	[SerializeField] TMP_InputField insertCodeInputF;
	[SerializeField] Button insertCodeConfirm;

	AudioSource audioSource;
	[SerializeField] AudioClip welcomeFAudio;
	[SerializeField] AudioClip ureadyFAudio;	
	[SerializeField] AudioClip welcomeMAudio;
	[SerializeField] AudioClip ureadyMAudio;
	[SerializeField] TMP_Text ureadyWelcomeM;
	[SerializeField] TMP_Text ureadyWelcomeF;
	[SerializeField] TMP_Text ureadyM;
	[SerializeField] TMP_Text ureadyF;

	string logInsuccedID;

	bool continueSelectedFlag;

	private void Awake()
	{
		TryGetComponent(out audioSource);
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
		afterLogInNewGameBtn.onClick.AddListener(UReady);

		cancelSearchBarBtn.onClick.AddListener(() => selectUserContainer.gameObject.SetActive(true));

		cancelAfterLogBtn.onClick.AddListener(() =>
		{
			OnExitSession();
			selectUserContainer.gameObject.SetActive(false);
		}
		);

		cancelConfirmPanel.onClick.AddListener(() =>
		{
			uReadyPanel.gameObject.SetActive(false);
			afterLogInPanel.gameObject.SetActive(true);

		});


		readyBtb.onClick.AddListener(OnReadyConfirmBtnPressed);

		continueSelectedFlag = false;
		searchField.onValueChanged.AddListener((_) => SearchUser());
		removeSelectedUser.onClick.AddListener(OnRemoveSelectedUserBtnPressed);
		confirmUserBtn.onClick.AddListener(() => OnSelectedUser(selectedUser));

        correctlyLoggedInFlag = false;
		doneInitialization = false;
		userBtnsPool.Init(10);
		currBtnsByDataID = new Dictionary<UsersListItem, string>();
		checkingInternetPanel.SetActive(false);

		noInternetConnectionPopUpOkBtn.onClick.AddListener(() => noInternetConnectionPopUp.SetActive(false));

		dataSyncedPopUpOkBtn.onClick.AddListener(() => dataSyncedPopUp.SetActive(false));

		confirmUserBtn.gameObject.SetActive(false);

		insertCodePanel.gameObject.SetActive(true);
		insertCodeConfirm.onClick.AddListener(OnInsertCodeFinished);
		insertCodeInputF.onValueChanged.AddListener(OnInsertedCodeChanged);
		insertCodeConfirm.interactable = false;
		UserDataManager.Instance.SetCurrUser(null);
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
	void UReady()
	{
        var currClip = UserDataManager.CurrUser.gender == UserGender.Femenino ? ureadyFAudio : ureadyMAudio;
        audioSource.clip = currClip;
        audioSource.Play();

        //finding which to deactivate
        var currWelcome = UserDataManager.CurrUser.gender == UserGender.Femenino ? ureadyWelcomeM : ureadyWelcomeF;
        var currUready = UserDataManager.CurrUser.gender == UserGender.Femenino ? ureadyM : ureadyF;
		currWelcome.gameObject.SetActive(false); 
		currUready.gameObject.SetActive(false);

        uReadyPanel.gameObject.SetActive(true);
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

	void OnInsertedCodeChanged(string newValue)
	{
		insertCodeConfirm.interactable = !string.IsNullOrEmpty(newValue);
	}

	void OnInsertCodeFinished()
	{
		UserDataManager.CurrInstitutionCode = insertCodeInputF.text;
		insertCodePanel.gameObject.SetActive(false);
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
			newBtn.Init(users[i].pin, this);
			currBtnsByDataID.Add(newBtn, users[i].id);
			newBtn.gameObject.SetActive(false);
		}
	}

    public void ShowSelection(UsersListItem userSelected)
    {
		selectedUser = userSelected;
		selectedUserLabelGO.gameObject.SetActive(true);
		selectedUserLabel.SetText(userSelected.label.text);
		confirmUserBtn.gameObject.SetActive(true);
		foreach (var item in currBtnsByDataID) item.Key.gameObject.SetActive(false);
    }

	public void OnRemoveSelectedUserBtnPressed()
	{
		selectedUser = null;
		searchField.text = "";
		selectedUserLabelGO.gameObject.SetActive(false);
		confirmUserBtn.gameObject.SetActive(false);
	}

	IEnumerator LoadUsers()
	{
		checkingInternetPanel.SetActive(true);
		yield return UserDataManager.Instance.LoadDataFromRemoteDataBaseRoutine();
		Debug.Log("Corretly retrieved users from server");
		checkingInternetPanel.SetActive(false);
		NoInternetWarningIcon.SetActive(!UserDataManager.Instance.HasInternet);
		if (!UserDataManager.Instance.HasInternet)
		{
			noInternetConnectionPopUp.SetActive(true);
		}
		else
		{
			var usersSynced = DatabaseManager.pendingSyncronizedUsersAmount;
			var sessionsSynced = DatabaseManager.pendingSyncronizedSessionsAmount;

			if(usersSynced > 0 || sessionsSynced > 0)
			{
				dataSyncedPopUp.SetActive(true);
				dataSyncedLabel.SetText(string.Format("Se subieron al servidor {0} usuario(s) cambiado(s) y {1} sesion(es), encontradas en el guardado local", usersSynced, sessionsSynced));
			}

		}
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
		newUser.pin = nameField.text;
		newUser.institutionCode = UserDataManager.CurrInstitutionCode;
        newUser.age = int.TryParse(ageField.options[ageField.value].text, out var ageResult)? ageResult : -1;
        newUser.grade = int.TryParse(gradeField.options[gradeField.value].text, out var gradeResult) ? gradeResult : -1;
		newUser.gender = Enum.TryParse<UserGender>(sexField.options[sexField.value].text, true ,out var genderFound)? genderFound : UserGender.NONE;
		newUser.schoolType =  (UserSchoolType) schoolTypeField.value ;
		newUser.country = countryField.text;
		newUser.livingWith = GetUserLivingWith();

		var validData = true;
		var errMsg = "";

		if (string.IsNullOrEmpty(newUser.pin))
		{
			errMsg = "El nombre de usuario está vacío o es inválido";
		}
		else if(newUser.age == -1)
		{
			errMsg = "El campo de edad está vacío o es inválido";
		}
		else if (newUser.gender == UserGender.NONE)
		{
			errMsg = "El género está vacío o es inválido";
		}
		else if (newUser.schoolType == UserSchoolType.NONE)
		{
			errMsg = "El tipo de escuela está vacía o es inválida";
		}
		else if (newUser.grade == -1)
		{
			errMsg = "El campo de grado está vacío o es inválido";
		}
		else if (string.IsNullOrEmpty(newUser.country))
		{
			errMsg = "El lugar de nacimiento está vacío o es inválido";
		}
		else if (newUser.livingWith == UserLivingWith.NONE)
		{
			errMsg = "El campo 'Con quien vives' está vacío o es inválido";
		}


		validData = string.IsNullOrEmpty(errMsg);
		wrongNewUserDataLabelPopUp.SetText(errMsg);

		if(!validData) wrongNewUserDataPopUp.gameObject.SetActive(true);
		else
		{
			newUser.id = Guid.NewGuid().ToString();
			UserDataManager.Instance.RegisterNewUser(newUser);
			createNewUserPanel.SetActive(false);
			RebuildUsersList();
			OnSelectedUser(newUser.id);
		}
	}
	void OnExitSession()
	{
        closeSessionPanel.gameObject.SetActive(false);
        wantsToExitSessionBtn.gameObject.SetActive(false);
        uReadyPanel.gameObject.SetActive(false);
		afterLogInPanel.gameObject.SetActive(false);
		UserDataManager.Instance.SetCurrUser(null);

        nameField.text = string.Empty;
        ageField.SetValueWithoutNotify(0);
		gradeField.SetValueWithoutNotify(0);
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
		if (!currBtnsByDataID.TryGetValue(data, out var idFound)) Debug.LogError("Trying to delete a user but was not found on dictionary");
		OnSelectedUser(idFound);
	}

	public void OnSelectedUser(string id)
	{
		var data = UserDataManager.Instance.usersDatas.Find(x => x.id == id);
		if(data == null) return;

		userNameHeader.text = data.pin;
		userNameExitPanel.text = data.pin;

		wantsToExitSessionBtn.gameObject.SetActive(true);

		UserDataManager.Instance.SetCurrUser(id);
		var storedCheckPoint = UserDataManager.CurrUser.CheckPointIdx;

		var currClip = UserDataManager.CurrUser.gender == UserGender.Femenino ? welcomeFAudio : welcomeMAudio;
		audioSource.clip = currClip;
		audioSource.Play();

		afterLogInPanel.SetActive(true);

        var currWelcome = UserDataManager.CurrUser.gender == UserGender.Femenino ? contWelcomeM : contWelcomeF;
		currWelcome.gameObject.SetActive(false);
        afterLogInContinueBtn.gameObject.SetActive(storedCheckPoint != -1);
	}


	void MusicBtn()
	{
		if (AudioManager.Instance.currentBkMusic.isPlaying)
		{
			AudioManager.Instance.currentBkMusic.Stop();
			musicBtn.image.sprite = musicBtnInactive;
		}
		else
		{
			AudioManager.Instance.currentBkMusic.Play();
			musicBtn.image.sprite = musicBtnActive;
		}
		bool activeStae = AudioManager.Instance.currentBkMusic.isPlaying;
		PlayerPrefs.SetInt(UserDataManager.CurrUser.id + " isTheSoundActive", activeStae ? 1 : 0);

	}

	public void OnContinueGameBtnPressed()
	{
		continueSelectedFlag = true;

		UReady();
	}

    public void OnReadyConfirmBtnPressed()
	{
		UserDataManager.CurrTestID = Guid.NewGuid().ToString();
		DatabaseManager.AddPendingUserData(UserDataManager.CurrUser);

		TimeManager.timer = 0;
		
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
