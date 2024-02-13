using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "UserDataManager", menuName = "User Data/ UserDataManager")]
public class UserDataManager : ScriptableObject
{
	[SerializeField] int maxAgeEasyLevel = 4;
    [SerializeField] int maxAgeMediumLevel = 8;
	public bool HasInternet = true;

	private static string instancePath = "UserDataManager";
	private static string defaultUserID = "DefaultUserId";

	private static string currTestID;


	public static string CurrTestID
	{
		get => string.IsNullOrEmpty(currTestID) ? "Default Test ID" : currTestID;
		set => currTestID = value;
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

	public UserData DefaultUserData = new UserData();


	public List<UserData> usersDatas => DatabaseManager.userDatas;
    
	
    public static Dictionary<string, Dictionary<string, Dictionary<string, object>>> userAnayticsPerGame = 
		new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();


    int currUserDataIdx = -1;

	public UserData CurrUserData
    {
        get
        {
			if (currUserDataIdx != -1 && currUserDataIdx < usersDatas.Count) return usersDatas[currUserDataIdx];
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
		Application.wantsToQuit += SaveToServer;
	}

    static void GetAllAnalyticsData()
    {
        for (int i = 0; i < GameSequencesList.Instance.gameSequences.Count; i++)
        {
            var newData = GameSequencesList.Instance.gameSequences[i].GetAnalytics();
			var sceneID = GameSequencesList.Instance.gameSequences[i].GetSceneID();
			if (string.IsNullOrEmpty(sceneID)) continue;
			if(CurrUser.userAnalytics.ContainsKey(sceneID)) CurrUser.userAnalytics[sceneID] = newData;
			else CurrUser.userAnalytics.Add(sceneID, newData);			
        }
    }

	public static void SaveUserAnayticsPerGame(string gameKey, Dictionary<string, object> itemAnalytics, string collectionID = null)
	{
		var analyticsWithExtraFields = new Dictionary<string, object>();
		analyticsWithExtraFields.Add(DataIds.TestID, CurrTestID);
		analyticsWithExtraFields.Add(DataIds.GameID, gameKey);
		analyticsWithExtraFields.Add(DataIds.UserID, CurrUser.id);
		if (collectionID != null) analyticsWithExtraFields.Add(DataIds.GameType, collectionID);
		analyticsWithExtraFields.AddRange(itemAnalytics);

		if(!userAnayticsPerGame.TryGetValue(gameKey, out var analyticsDocsFound))
		{
			analyticsDocsFound = new Dictionary<string, Dictionary<string, object>>();
			userAnayticsPerGame.Add(gameKey, analyticsDocsFound);
		}

		//Generating a new document ID each time
		var newDocumentID = Guid.NewGuid().ToString();
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

		GetAllAnalyticsData();

		Debug.Log("Saving to server");
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

		DatabaseManager.GetUserDatasList();
		while (!DatabaseManager.userListDone) yield return null;
	}

	public IEnumerator CheckInternetConnection()
	{
		UnityWebRequest www = new UnityWebRequest("http://unity3d.com/");

		yield return www.SendWebRequest();

		if (www.result == UnityWebRequest.Result.ConnectionError) // Error
		{
			HasInternet = false;
		}
		else // Success
		{
			HasInternet = true;
		}
	}

	public void SaveDataToRemoteDataBase()
	{
		DatabaseManager.SaveUserDatasList(usersDatas, userAnayticsPerGame);
	}

	public void SetCurrUser(string email, string id)
	{
		var newuserData = new UserData();
		newuserData.name = email;
		newuserData.id = id;
		RegisterNewUser(newuserData);
	}

	public void RegisterNewUser(UserData user)
	{
		if(usersDatas.Exists(x => x.id == user.id))
		{
			Debug.Log("Trying to add user: " + user.name + " " + user.id + " But ID already existed");
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

