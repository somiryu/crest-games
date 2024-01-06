using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UserDataManager", menuName = "User Data/ UserDataManager")]
public class UserDataManager : ScriptableObject
{
	[SerializeField] int maxAgeEasyLevel = 4;
    [SerializeField] int maxAgeMediumLevel = 8;

	private static string instancePath = "UserDataManager";

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
			if(newData != null)
			{
                List<string> currDictionaryKeys = newData.Keys.ToList();
                for (int j = 0; j < currDictionaryKeys.Count; j++)
                {
                    if (CurrUser.userAnayticsResults.ContainsKey(currDictionaryKeys[j]))
                    {
                        CurrUser.userAnayticsResults[currDictionaryKeys[j]] = newData[currDictionaryKeys[j]];
                    }
                    else
					{
                        CurrUser.userAnayticsResults.Add(currDictionaryKeys[j], newData[currDictionaryKeys[j]]);
						Debug.Log(CurrUser.userAnayticsResults[currDictionaryKeys[j]]);
                    }
                }
            }
        }
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
		usersDatas = DatabaseManager.GetUserDatasList();
	}

	public void SaveDataToRemoteDataBase()
	{
		DatabaseManager.SaveUserDatasList(usersDatas);
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

