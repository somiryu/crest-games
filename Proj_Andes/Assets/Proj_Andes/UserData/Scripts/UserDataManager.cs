using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "UserDataManager", menuName = "User Data/ UserDataManager")]
public class UserDataManager : ScriptableObject
{
	[SerializeField] int maxAgeEasyLevel = 4;
    [SerializeField] int maxAgeMediumLevel = 8;

	private static string instancePath = "UserDataManager";
	private static string defaultUserID = "DefaultUserId";

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


	[NonSerialized]
	public List<UserData> usersDatas = new List<UserData>();
    
	
    public static Dictionary<string, Dictionary<string, object>> userAnayticsPerGame = new Dictionary<string, Dictionary<string, object>>();


    int currUserDataIdx = -1;

	public UserData CurrUserData
    {
        get
        {
			if (currUserDataIdx != -1 && currUserDataIdx < usersDatas.Count) return usersDatas[currUserDataIdx];
			return DefaultUserData;
		}
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

	public static void SaveUserAnayticsPerGame(string gameKey, Dictionary<string, object> itemAnalytics)
	{
        var userID = CurrUser.name + " " + CurrUser.id;
        var playerItemAnalytics = new Dictionary<string, object>();
        playerItemAnalytics.Add(userID, itemAnalytics);


        if (userAnayticsPerGame.ContainsKey(gameKey))
		{
			if (userAnayticsPerGame[gameKey].ContainsKey(userID)) userAnayticsPerGame[gameKey][userID] = playerItemAnalytics;			
			else userAnayticsPerGame[gameKey].Add(userID, playerItemAnalytics);
		}
		else userAnayticsPerGame.Add(gameKey, playerItemAnalytics);
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
		DatabaseManager.GetUserDatasList();
	}


	//Doing this to avoid an issue that happens if you call resources from a "Task" (Happening when user just logged in)
	public static void Init()
	{
		if (instance == null) instance = Resources.Load<UserDataManager>(instancePath);
	}

	public void LoadDataFromRemoteDataBase()
	{
		DatabaseManager.GetUserDatasList();
		usersDatas = DatabaseManager.userDatas;
	}

	public void Update()
	{

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
		usersDatas.Add(user);
		SaveDataToRemoteDataBase();
	}

	public void RemoveUser(string id)
	{
		var data = usersDatas.Find(x => x.id == id);
		usersDatas.Remove(data);
		SaveDataToRemoteDataBase();
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

