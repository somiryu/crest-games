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
	public static string LastDocumentIDStored = null;


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
			DefaultUserData.id = "0000-0000-0000";
            return DefaultUserData;
        }
	}

	public bool HasInternetConnection()
	{
		return HasInternet;
	}

	[RuntimeInitializeOnLoadMethod]
	static void RunOnStart()
	{
		Debug.Log("aplying callback");
		//Application.wantsToQuit += SaveToServer;
	}

	public static void SaveUserAnayticsPerGame(
		string CollectionName, 
		Dictionary<string, object> itemAnalytics, 
		string documentID = null, 
		string gameType = null, 
		bool shouldUseTestID = true)
	{
		var analyticsWithExtraFields = new Dictionary<string, object>();
		if (shouldUseTestID) analyticsWithExtraFields.Add(DataIds.TestID, CurrTestID);
		analyticsWithExtraFields.Add(DataIds.UserID, CurrUser.id);
		analyticsWithExtraFields.Add(DataIds.GameOrderInSequence, GameSequencesList.Instance.goToGameGroupIdx);
		if (gameType != null) analyticsWithExtraFields.Add(DataIds.GameType, gameType);
		analyticsWithExtraFields.AddRange(itemAnalytics);

		foreach(var analytic in analyticsWithExtraFields)
		{
			Debug.Log("Wants to save analytic with key: " + analytic);
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
    }

    public static bool SaveToServer()
	{
		OnUserQuit();
		return true;
	}

	public static void OnUserQuit()
	{
		CurrUser.CheckPointIdx = GameSequencesList.Instance.goToGameGroupIdx;
		var currSequence = GameSequencesList.Instance.GetGameSequence();
		CurrUser.CheckPointSubIdx = currSequence.GetCurrItemIdx();

		TimeManager.Instance.ResetSessionTimerAndSave();
		if (currSequence is MinigameGroups group)
		{
			CurrUser.itemsPlayedIdxs = group.GetItemsPlayedData();
		}
		else CurrUser.itemsPlayedIdxs.Clear();

		var dialogSystem = DialoguesDisplayerUI.Instance;
		if (dialogSystem != null && dialogSystem.SaveNavSequence)
		{
			//Store navigation info
			CurrUser.narrativeNavCheckPointsNodes = dialogSystem.GetCurrNavigationNodes();
		}
		else CurrUser.narrativeNavCheckPointsNodes = null;

		//Don't save if we are using the default user
		if (CurrUser.id == defaultUserID) return;

		//TODO ADD A Pause here so that the player can't leave if the data hasn't been fully saved yet
		UserDataManager.Instance.SaveDataToRemoteDataBase();
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
		newuserData.id = id;
		RegisterNewUser(newuserData);
	}

	public void RegisterNewUser(UserData user)
	{
		var alreadyInIndex = usersDatas.FindIndex(x => x.id == user.id);
        if (alreadyInIndex != -1)
		{
			usersDatas[alreadyInIndex] = user;
            Debug.Log("Trying to add user: " + user.pin + " " + user.id + " But ID already existed, replacing data " + user.age);
			return;
		}
		usersDatas.Add(user);
		SaveDataToRemoteDataBase();
	}

	public void RemoveUser(string id)
	{
		var data = usersDatas.Find(x => x.id == id);
		DatabaseManager.DeleteUserFromDataList(data);
	}

	public void SetCurrUser(string id)
	{
		if (id == null)
		{
			currUserDataIdx = -1;
			return;
		}

		var idx = usersDatas.FindIndex(x =>x.id == id);
		currUserDataIdx = idx;
	}

    public DifficultyLevel GetDifficultyLevelUser()
    {
		var currAge = CurrUser.age;
        if (currAge <= maxAgeEasyLevel) return DifficultyLevel.Easy;
        else if (currAge <= maxAgeMediumLevel) return DifficultyLevel.Medium;
        else return DifficultyLevel.Hard;        
    }

}

