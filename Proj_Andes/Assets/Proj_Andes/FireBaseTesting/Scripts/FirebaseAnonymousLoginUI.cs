using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase.Auth;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using Tymski;

public class FirebaseAnonymousLoginUI : MonoBehaviour
{
	public static bool saveOnlyLocalForTesting = false;

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

	[Header("Delete User Confirmation")]
	[SerializeField] Transform deleteUserPanel;
	[SerializeField] Button deleteUserConfirmBtn;
	[SerializeField] Button deleteUserCancelBtn;
	[SerializeField] TMP_Text deleteUserNameLabel;
	string userIdPendingToDelete;
	[SerializeField] GameObject selectedUserLabelGO;
	[SerializeField] TMP_Text selectedUserLabel;


	[Header("Create new user panel")]
	public GameObject createNewUserPanel;
	[SerializeField] TMP_InputField nameField;
    //Open numeric answers: any number can be typed
    [SerializeField] TMP_InputField ageField;
    [SerializeField] TMP_InputField gradeField;
    [SerializeField] TMP_Dropdown sexField;
    [SerializeField] TMP_Dropdown levelField;
    [SerializeField] TMP_Dropdown schoolTypeField;
    [SerializeField] TMP_InputField countryField;
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
	[SerializeField] Slider syncingDataSlider;
	[SerializeField] GameObject NoInternetWarningIcon;

	[Header("PickANarrative Panel")]
	[SerializeField] Transform pickANarrativePanel;
    public List<BtnPerNarrative> btnsPerNarrative = new List<BtnPerNarrative>();
    [SerializeField] Button selectedNarrativeBtn;
    [SerializeField] MinigameGroups narratives;

	[Header("Loading panel")]
	[SerializeField] Transform loadingPanel;
	[SerializeField] Slider loadingSlider;
	[SerializeField] Image replacebleImg;
	SceneReference currNextNarr;
	int currNextNarrIdx;

    [Header("ConfirmStart Panel")]
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
		deleteUserPanel.gameObject.SetActive(false);
		deleteUserConfirmBtn.onClick.AddListener(OnDeleteUserConfirmed);
		deleteUserCancelBtn.onClick.AddListener(CloseDeleteUserPanel);
		musicBtn.onClick.AddListener(() => SwitchSoundActive());
		goToExistingUserPanel.onClick.AddListener(OnWantsToAccessExistingUser);

		goToUserCreationPanel.onClick.AddListener(OnWantsToCreateNewUser);
		wrongNewUserDataPopUp.onClick.AddListener(() => wrongNewUserDataPopUp.gameObject.SetActive(false));
		logInFailedPopUp.onClick.AddListener(() => logInFailedPopUp.gameObject.SetActive(false));
		afterLogInContinueBtn.onClick.AddListener(OnContinueGameBtnPressed);
		afterLogInNewGameBtn.onClick.AddListener(UReady);

		for (int i = 0; i < btnsPerNarrative.Count; i++)
		{
			var narrCount = i;
            btnsPerNarrative[i].btn.onClick.AddListener(() => OnSelectedNarrativeBtn(btnsPerNarrative[narrCount].narrative));
        }

		selectedNarrativeBtn.onClick.AddListener(OnSelectedNarrative);

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
			continueSelectedFlag = false;

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

		MonsterMarketConfig.marketAppearTimes = -1;

	}


	private IEnumerator Start()
	{
		var defaultSoundState = PlayerPrefs.GetInt(UserDataManager.CurrUser.id_jugador + " isTheSoundActive", defaultValue: 1);
		AssignSoundActive(defaultSoundState);

		FirebaseAuth auth = FirebaseAuth.DefaultInstance;

		if(auth.CurrentUser != null)
		{
			Debug.Log("Already signed in: " + auth.CurrentUser.UserId);
			logInsuccedID = ("Firebase ID:" + auth.CurrentUser.UserId);
			correctlyLoggedInFlag = true;
			yield break;
		}
		auth.SignInAnonymouslyAsync().ContinueWith(OnSingingResult);
	}


	void UReady()
	{
        var currClip = UserDataManager.CurrUser.UsesFeminineContent ? ureadyFAudio : ureadyMAudio;
        audioSource.clip = currClip;
        audioSource.Play();

        //finding which to deactivate
        var currWelcome = UserDataManager.CurrUser.UsesFeminineContent ? ureadyWelcomeM : ureadyWelcomeF;
        var currUready = UserDataManager.CurrUser.UsesFeminineContent ? ureadyM : ureadyF;
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
		else if (taskResult.IsFaulted)
		{
			OnFailedLogIn(taskResult);
		}
		else
		{
			AuthResult result = taskResult.Result;

			Debug.LogFormat("User signed in successfully: {0} ({1})",
				result.User.DisplayName, result.User.UserId);

			logInsuccedID = ("Firebase ID:" + result.User.UserId);
		}
		
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
			currBtnsByDataID.Add(newBtn, users[i].id_jugador);
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


	IEnumerator syncDataRoutineRef;

	IEnumerator LoadUsers()
	{
		checkingInternetPanel.SetActive(true);

		syncDataRoutineRef = SyncDataSliderRoutine();
		StartCoroutine(syncDataRoutineRef);
		yield return UserDataManager.Instance.LoadDataFromRemoteDataBaseRoutine();
		StopCoroutine(syncDataRoutineRef);
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
			var testAmounts = DatabaseManager.testAmountsSavedToDataBase;

			if(usersSynced > 0 || sessionsSynced > 0)
			{
				dataSyncedPopUp.SetActive(true);
				dataSyncedLabel.SetText(string.Format("Se subieron al servidor {0} usuario(s) cambiado(s) y {1} sesion(es), encontradas en el guardado local",
					usersSynced, testAmounts));
			}
		}
		RebuildUsersList();
	}

	IEnumerator SyncDataSliderRoutine()
	{
		while (true)
		{
			syncingDataSlider.SetValueWithoutNotify(DatabaseManager.currLoadProgress);
			yield return null;
		}
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
    void OnSelectedNarrativeBtn(int narIdx)
	{
		for (int i = 0; i < btnsPerNarrative.Count; i++)
		{
			if (btnsPerNarrative[i].narrative == narIdx)
			{
                btnsPerNarrative[i].highlight.gameObject.SetActive(true);
				currNextNarr = narratives.miniGamesInGroup[narIdx - 1].scene;
				currNextNarrIdx = narIdx - 1;
				replacebleImg.sprite = btnsPerNarrative[i].btnImg.sprite;
            }
            else btnsPerNarrative[i].highlight.gameObject.SetActive(false);
        }
		narratives.forcedScene = narIdx;
	}
	/// <summary>
	/// Only the student's pin is required here (the project code is asked for before, in its own panel).
	/// Everything else may be left empty and is saved as unknown (99).
	/// </summary>
	public static string GetNewUserError(string pin, bool isAlreadyRegistered)
	{
		if (string.IsNullOrEmpty(pin)) return "El pin de usuario está vacío";
		if (isAlreadyRegistered) return "El pin de usuario ya está registrado en esta institución";
		return "";
	}

	public const int UnknownNumber = 99;

	/// <summary>A number typed in an open numeric field; empty or not a number is unknown (99).</summary>
	public static int ParseOpenNumber(string text)
	{
		return int.TryParse(text?.Trim(), out var number) && number >= 0 ? number : UnknownNumber;
	}

	/// <summary>Commas become hyphens (they break the CSV downloads); an empty answer is unknown ("99").</summary>
	public static string CleanBirthplace(string text)
	{
		var clean = (text ?? string.Empty).Replace(",", "-").Trim();
		return string.IsNullOrEmpty(clean) ? "99" : clean;
	}

	void OnFinishedUserCreation()
	{
		Debug.Log("OnFinishedUserCreation in FirebaseAnonymousLoginUI > " + schoolTypeField.value);
		var newUser = new UserData();
		newUser.pin = nameField.text;
		newUser.id_proyecto = UserDataManager.CurrInstitutionCode;

		newUser.edad = ParseOpenNumber(ageField.text);
		newUser.grado = ParseOpenNumber(gradeField.text);

		newUser.gender = Enum.TryParse<UserGender>(sexField.options[sexField.value].text, true, out var genderFound) ? genderFound : UserGender.NONE;
		//Optional: stays unknown (99) when the placeholder is left selected
		newUser.level = Enum.TryParse<UserLevel>(levelField.options[levelField.value].text, true, out var levelFound) ? levelFound : UserLevel.NONE;
		//The code comes from the text, not from the position: the options aren't in the order of the codes
		newUser.schoolSector = UserData.ParseSector(schoolTypeField.options[schoolTypeField.value].text);
		newUser.lugar_nacimiento = CleanBirthplace(countryField.text);

		var validData = true;
		var isAlreadyRegistered = UserDataManager.Instance.usersDatas.FindIndex(x => x.pin == newUser.pin && x.id_proyecto == newUser.id_proyecto) != -1;
		var errMsg = GetNewUserError(newUser.pin, isAlreadyRegistered);

		Debug.Log("Error? >> " + errMsg);
		Debug.Log("USER INFO TO SAVE!:");
		Debug.Log("Genero >" + newUser.gender);
		Debug.Log("Genero cod >" + newUser.gen_cod);
		Debug.Log("Nivel >" + newUser.level + " cod " + newUser.nivel_cod);
		validData = string.IsNullOrEmpty(errMsg);
		wrongNewUserDataLabelPopUp.SetText(errMsg);

		if (!validData) wrongNewUserDataPopUp.gameObject.SetActive(true);
		else
		{
			var alreadyInIdx = 
				UserDataManager.Instance.usersDatas.FindIndex(x => x.pin == newUser.pin && x.id_proyecto == newUser.id_proyecto);

			if (alreadyInIdx != -1) newUser.id_jugador = UserDataManager.Instance.usersDatas[alreadyInIdx].id_jugador;
			else newUser.id_jugador = Guid.NewGuid().ToString();

			UserDataManager.Instance.RegisterNewUser(newUser);
			createNewUserPanel.SetActive(false);
			RebuildUsersList();
			OnSelectedUser(newUser.id_jugador);
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
        ageField.text = string.Empty;
		gradeField.text = string.Empty;
		sexField.SetValueWithoutNotify(0);
		levelField.SetValueWithoutNotify(0);
		schoolTypeField.SetValueWithoutNotify(0);
		countryField.text = string.Empty;

        selectUserContainer.gameObject.SetActive(true);
    }


	//Deleting a profile can't be undone, so it's always confirmed first
	public void RemoveUserOfBtn(UsersListItem item)
	{
		if (!currBtnsByDataID.TryGetValue(item, out var idFound))
		{
			Debug.LogError("Trying to delete a user but was not found on dictionary");
			return;
		}
		userIdPendingToDelete = idFound;
		deleteUserNameLabel.SetText(item.label.text);
		deleteUserPanel.gameObject.SetActive(true);
	}

	void OnDeleteUserConfirmed()
	{
		if (!string.IsNullOrEmpty(userIdPendingToDelete))
		{
			UserDataManager.Instance.RemoveUser(userIdPendingToDelete);
			userDeletionInProgress = true;
			OnRemoveSelectedUserBtnPressed();
		}
		CloseDeleteUserPanel();
	}

	void CloseDeleteUserPanel()
	{
		userIdPendingToDelete = null;
		deleteUserPanel.gameObject.SetActive(false);
	}

	public void OnSelectedUser(UsersListItem data)
	{
		if (data == null) return;
		if (!currBtnsByDataID.TryGetValue(data, out var idFound)) Debug.LogError("Trying to delete a user but was not found on dictionary");
		OnSelectedUser(idFound);
	}

	public void OnSelectedUser(string id)
	{
		var data = UserDataManager.Instance.usersDatas.Find(x => x.id_jugador == id);
		if(data == null) return;

		userNameHeader.text = data.pin;
		userNameExitPanel.text = data.pin;

		wantsToExitSessionBtn.gameObject.SetActive(true);

		UserDataManager.Instance.SetCurrUser(id);

		if (GameSequencesList.isTheNarrativeSequence) pickANarrativePanel.gameObject.SetActive(GameSequencesList.isTheNarrativeSequence);
		else ContinueOrNewGame();
    }
	void ContinueOrNewGame()
	{
		afterLogInPanel.SetActive(true);

		var currClip = UserDataManager.CurrUser.UsesFeminineContent ? welcomeFAudio : welcomeMAudio;
		audioSource.clip = currClip;
		audioSource.Play();

		var currUser = UserDataManager.CurrUser;

		var storedCheckPoint = currUser.CheckPointIdx; 

		contWelcomeF.gameObject.SetActive(currUser.UsesFeminineContent);
		contWelcomeM.gameObject.SetActive(!currUser.UsesFeminineContent);

		var soundPref = PlayerPrefs.GetInt(currUser.id_jugador + " isTheSoundActive", 1);
		Debug.Log("sound " + PlayerPrefs.GetInt(currUser.id_jugador + " isTheSoundActive"));
		AssignSoundActive(soundPref);
        afterLogInContinueBtn.gameObject.SetActive(storedCheckPoint != -1 && !GameSequencesList.isTheNarrativeSequence);
    }
	void OnSelectedNarrative()
	{
		pickANarrativePanel.gameObject.SetActive(false);
		ContinueOrNewGame();
    }
	void SwitchSoundActive()
	{
		var previousState = PlayerPrefs.GetInt(UserDataManager.CurrUser.id_jugador + " isTheSoundActive", defaultValue: 1);
		AssignSoundActive(previousState == 0 ? 1 : 0);
	}

	void AssignSoundActive(int active)
	{
		if (active == 0)
		{
			AudioManager.Instance.currentBkMusic.Stop();
			Debug.Log("deactivate");
			musicBtn.image.sprite = musicBtnInactive;
		}
		else
		{
			AudioManager.Instance.currentBkMusic.Play();
			musicBtn.image.sprite = musicBtnActive;
		}
		PlayerPrefs.SetInt(UserDataManager.CurrUser.id_jugador + " isTheSoundActive", active);
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

		MG_SizeRockets_GameConfigs.postFrustration = 0;
		MG_VoiceStarOrFlowerGameConfigs.postFrustration = 0;
		MG_HearthAndStarsGameConfigs.postFrustration = 0;
		Gratification_TurboRocket_GameConfig.postFrustration = 0;
		MonsterMarketConfig.marketAppearTimes = -1;

		MG_VoiceStarOrFlowerGameConfigs.CurrFlowersAndCloudsGameID = null;

        TimeManager.createDate = TimeManager.Instance.GetCurrDate();
		GameSequencesList.CleanCurrUserTutorial();
        TimeManager.timer = 0;
		if (continueSelectedFlag)
		{
			var targetSequence = GameSequencesList.Instance.gameSequences[UserDataManager.CurrUser.CheckPointIdx];
			if (targetSequence is MinigameGroups group)
			{
				group.SetItemsPlayedData(UserDataManager.CurrUser.itemsPlayedIdxs);
			}
			DialoguesDisplayerUI.CheckPointTreeToConsume = UserDataManager.CurrUser.narrativeNavCheckPointsNodes;
            if (!GameSequencesList.isTheNarrativeSequence) GameSequencesList.Instance.GoToSequenceIdx(UserDataManager.CurrUser.CheckPointIdx, UserDataManager.CurrUser.CheckPointSubIdx);
		}
		else
		{
			UserDataManager.CurrUser.myCollectionMonsters.Clear();
			UserDataManager.CurrUser.Coins = 10;
            GameSequencesList.Instance.GoToNextSequence(loadScene: !GameSequencesList.isTheNarrativeSequence);
		}
        if (GameSequencesList.isTheNarrativeSequence) StartCoroutine(LoadingSceneAsync(currNextNarr));
    }

	IEnumerator LoadingSceneAsync(SceneReference scene)
	{
		yield return Resources.UnloadUnusedAssets();

		loadingPanel.gameObject.SetActive(true); 
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        while (asyncLoad.progress < 0.9f)
        {
			loadingSlider.value = asyncLoad.progress;
			Debug.Log(asyncLoad.progress);
            yield return null;
        }

        //loadingPanel.gameObject.SetActive(false);
    }
}
[Serializable]
public struct BtnPerNarrative
{
    public int narrative;
    public Button btn;
    public Image highlight;
    public Image btnImg;
    public AudioClip narrativeAudio;
}