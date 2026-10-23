using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "UserDataManager", menuName = "User Data/ UserDataManager")]
public class UserDataManager : ScriptableObject
{
	[SerializeField] int maxAgeEasyLevel = 4;
    [SerializeField] int maxAgeMediumLevel = 8;
	public bool HasInternet = true;

	private static string instancePath = "UserDataManager";
	private static string defaultUserID = "0000-0000-0000";

	private static string currTestID;
	private static string currInstitutionCode;

	public static string LastCollectionIDStored = null;
	public static List<string> LastDocumentIDsStored = null;


	public static string CurrTestID
	{
		get => string.IsNullOrEmpty(currTestID) ? "Default Test ID" : currTestID;
		set => currTestID = value;
	}

	public static string CurrInstitutionCode
	{
		get => string.IsNullOrEmpty(currInstitutionCode) ? "Default institution code" : currInstitutionCode;
		set => currInstitutionCode = value;
	}

	private static UserDataManager instance;

	public static UserDataManager Instance
	{
		get
		{
			if (!instance)
			{
				instance = Resources.Load<UserDataManager>(instancePath);
				Debug.Log(instance != null);
			}
			if(instance == null) Debug.LogError("No user data manager found");
			return instance;
		}
	}

	public static UserData CurrUser => Instance.CurrUserData;

	public static UserData DefaultUserData = new UserData();


	public List<UserData> usersDatas => DatabaseManager.userDatas;
    
	
    public static Dictionary<string, Dictionary<string, Dictionary<string, object>>> userAnayticsPerGame = 
		new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();


    static int currUserDataIdx = -1;

	public UserData CurrUserData
    {
        get
        {
			if (currUserDataIdx != -1 && currUserDataIdx < usersDatas.Count) return usersDatas[currUserDataIdx];
			DefaultUserData.id_jugador = "0000-0000-0000";
            return DefaultUserData;
        }
	}

	public bool HasInternetConnection()
	{
		return HasInternet;
	}

	public static void SaveUserAnayticsPerGame(
		string CollectionName, 
		Dictionary<string, object> itemAnalytics, 
		string documentID = null, 
		bool shouldUseTestID = true)
	{
		Debug.Log("SaveUserAnalyticsPerGame activated... building fields!!!");
		var analyticsWithExtraFields = new Dictionary<string, object>();
        analyticsWithExtraFields.Add(DataIds.institutionCode, CurrUser.id_proyecto);
        analyticsWithExtraFields.Add(DataIds.UserID, CurrUser.id_jugador);
        if (shouldUseTestID) analyticsWithExtraFields.Add(DataIds.TestID, CurrTestID);
        analyticsWithExtraFields.Add(DataIds.AppVersionField, AppVersion.Number);
		//analyticsWithExtraFields.Add(DataIds.GameOrderInSequence, GameSequencesList.Instance.goToGameGroupIdx);
		analyticsWithExtraFields.AddRange(itemAnalytics);

		foreach(var analytic in analyticsWithExtraFields)
		{
			Debug.Log("Wants to save analytic with key: " + analytic.Key + " " + analytic.Value);
		}

		if (string.IsNullOrEmpty(CollectionName))
		{
			Debug.LogError("Trying to save something with null collection name");
		}

		if(!userAnayticsPerGame.TryGetValue(CollectionName, out var analyticsDocsFound))
		{
			analyticsDocsFound = new Dictionary<string, Dictionary<string, object>>();
			userAnayticsPerGame.Add(CollectionName, analyticsDocsFound);
		}

		//Generating a new document ID each time if an explicit documentID was not passed in 
		var newDocumentID = string.IsNullOrEmpty(documentID)? Guid.NewGuid().ToString() : documentID;
		Debug.Log("will try to save for collection name: " + CollectionName);
		Debug.Log("doc id " + documentID + " game id " + CollectionName);
		analyticsDocsFound.Add(newDocumentID, analyticsWithExtraFields);
		foreach (var item in analyticsWithExtraFields)
		{
			Debug.Log("extrafields " + item.Key + " " + item.Value);
        }
    }

	/// <summary>
	/// The minigames session reached the closing scene: it is finished, so its documents (and the child's profile, which
	/// is only uploaded together with a finished session) are queued for upload.
	/// </summary>
	public static void CommitFinishedSession()
	{
		if (CurrUser.id_jugador == defaultUserID) return;
		//Saves the "prueba" document of the session with the state Finalizado, which is what marks its documents as uploadable
		TimeManager.Instance.ResetSessionTimerAndSave();
		DatabaseManager.AddPendingUserData(CurrUser);
		DatabaseManager.ForgetLocalOnlyProfile(CurrUser.id_jugador);
		DatabaseManager.SaveUserDatasList(Instance.usersDatas, userAnayticsPerGame, mustSaveInDataBase: false);
	}

	/// <summary>
	/// The minigames session was abandoned: everything it generated is discarded, nothing is uploaded and the child
	/// has to start the whole game again.
	/// </summary>
	public static void DiscardCurrentSession()
	{
		var removed = 0;
		//CurrTestID reads "Default Test ID" when no session is running: nothing to remove then
		if (CurrTestID != "Default Test ID")
		{
			removed += DatabaseManager.RemoveSessionDocs(DatabaseManager.pendingSessionsToUpload, CurrTestID);
			removed += DatabaseManager.RemoveSessionDocs(userAnayticsPerGame, CurrTestID);
		}
		Debug.Log("Abandoned session discarded, " + removed + " documents removed");
		CurrTestID = null;
		LastCollectionIDStored = null;
		LastDocumentIDsStored = null;
		TimeManager.timer = 0;
		TimeManager.Instance.gameState = SessionStateLeft.Abandonado;
		ResetResumePoint(CurrUser);
	}

	//Sessions can't be resumed: a child that didn't finish starts the whole game again
	static void ResetResumePoint(UserData user)
	{
		user.CheckPointIdx = -1;
		user.CheckPointSubIdx = -1;
		user.itemsPlayedIdxs.Clear();
		user.narrativeNavCheckPointsNodes = null;
	}

	public static void OnUserQuit()
	{
		Debug.Log("Calling on user quit---!");
		if (GameSequencesList.isTheNarrativeSequence) SaveNarrativeResumePoint();
		else ResetResumePoint(CurrUser);

		if (TimeManager.Instance.gameState != SessionStateLeft.Finalizado) TimeManager.Instance.gameState = SessionStateLeft.Abandonado;
		TimeManager.Instance.ResetSessionTimerAndSave();

		//Don't save if we are using the default user
		if (CurrUser.id_jugador == defaultUserID) return;

		//TODO ADD A Pause here so that the player can't leave if the data hasn't been fully saved yet
		UserDataManager.Instance.SaveDataToRemoteDataBase();
	}


	//The narratives app still resumes where the child left
	static void SaveNarrativeResumePoint()
	{
		CurrUser.CheckPointIdx = GameSequencesList.Instance.goToGameGroupIdx;
		var currSequence = GameSequencesList.Instance.GetGameSequence();
		CurrUser.CheckPointSubIdx = currSequence.GetCurrItemIdx();
		if (currSequence is MinigameGroups group) CurrUser.itemsPlayedIdxs = group.GetItemsPlayedData();
		else CurrUser.itemsPlayedIdxs.Clear();

		var dialogSystem = DialoguesDisplayerUI.Instance;
		if (dialogSystem != null && dialogSystem.SaveNavSequence)
		{
			//Store navigation info
			CurrUser.narrativeNavCheckPointsNodes = dialogSystem.GetCurrNavigationNodes();
		}
		else CurrUser.narrativeNavCheckPointsNodes = null;
	}

	//Doing this to avoid an issue that happens if you call resources from a "Task" (Happening when user just logged in)
	public static void Init()
	{
		if (instance == null) instance = Resources.Load<UserDataManager>(instancePath);
	}

	public void LoadDataFromRemoteDataBase()
	{
		DatabaseManager.GetUserDatasList();
	}

	public IEnumerator LoadDataFromRemoteDataBaseRoutine()
	{
		yield return CheckInternetConnection();
		//Saving locally so that the get user data list already has all the merged local data
		DatabaseManager.SaveUserDatasList(usersDatas, userAnayticsPerGame, mustSaveInDataBase: false);
		DatabaseManager.GetUserDatasList();
		while (!DatabaseManager.userListDone) yield return null;
		if(DatabaseManager.pendingSyncronizedSessionsAmount > 0 || DatabaseManager.pendingSyncronizedUsersAmount > 0)
		{
			//Saving in case we had previousData
			DatabaseManager.SaveUserDatasList(usersDatas, userAnayticsPerGame, mustSaveInDataBase: true, ignoreEmptyUser: true);
			while (!DatabaseManager.savingIsDone) yield return null;
			Debug.Log("Done saving and loading updated data");
		}
	}

	public IEnumerator CheckInternetConnection() => RecursiveInternetCheck(0);


	public IEnumerator RecursiveInternetCheck(int tryNumber)
	{
		UnityWebRequest www = new UnityWebRequest("www.google.com");

		yield return www.SendWebRequest();

		if (www.result == UnityWebRequest.Result.ConnectionError) // Error
		{
			Debug.Log("Failed try: " + tryNumber);
			tryNumber++;
			if (tryNumber >= 5) HasInternet = false;
			else
			{
				//Little delay between tries
				yield return new WaitForSeconds(0.5f);
				yield return RecursiveInternetCheck(tryNumber);
			}
		}
		else // Success
		{
			HasInternet = true;
		}
	}

	public void SaveDataToRemoteDataBase()
	{
		DatabaseManager.SaveUserDatasList(usersDatas, userAnayticsPerGame, mustSaveInDataBase: false);
	}

	public void SetCurrUser(string email, string id)
	{
		var newuserData = new UserData();
		newuserData.pin = email;
		newuserData.id_jugador = id;
		RegisterNewUser(newuserData);
	}

	public void RegisterNewUser(UserData user)
	{
		var alreadyInIndex = usersDatas.FindIndex(x => x.id_jugador == user.id_jugador);
        if (alreadyInIndex != -1)
		{
			usersDatas[alreadyInIndex] = user;
            Debug.Log("Trying to add user: " + user.pin + " " + user.id_jugador + " But ID already existed, replacing data " + user.edad);
			return;
		}
		usersDatas.Add(user);
		SaveDataToRemoteDataBase();
	}

	public void RemoveUser(string id)
	{
		//The current user is stored as an index, so keep it pointing to the same user after the list shrinks
		var currUserID = currUserDataIdx != -1 ? CurrUserData.id_jugador : null;
		var data = usersDatas.Find(x => x.id_jugador == id);
		DatabaseManager.DeleteUserFromDataList(data);
		SetCurrUser(currUserID == id ? null : currUserID);
	}

	public void SetCurrUser(string id)
	{
		if (id == null)
		{
			currUserDataIdx = -1;
			return;
		}

		var idx = usersDatas.FindIndex(x =>x.id_jugador == id);
		currUserDataIdx = idx;
	}

    public DifficultyLevel GetDifficultyLevelUser()
    {
		var currAge = CurrUser.edad;
        if (currAge <= maxAgeEasyLevel) return DifficultyLevel.Easy;
        else if (currAge <= maxAgeMediumLevel) return DifficultyLevel.Medium;
        else return DifficultyLevel.Hard;        
    }

}

