using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UserDataManager", menuName = "User Data/ UserDataManager")]
public class UserDataManager : ScriptableObject
{
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
	[NonSerialized] static List<Dictionary<string, object>> anayticsResults = new List<Dictionary<string, object>>();

    [SerializeField] UserData currUserData;

	public UserData CurrUserData
	{
		get 
		{
			if (currUserData != null) return currUserData;
			return DefaultUserData;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	static void RunOnStart()
	{
		Debug.Log("aplying callback");
		Application.wantsToQuit += WantsToQuit;
	}
	public static void GetAllAnalyticsData()
	{
		for (int i = 0; i < GameSequencesList.Instance.gameSequences.Count; i++)
		{
			var newData = GameSequencesList.Instance.gameSequences[i].GetAnalytics();
            anayticsResults.Add(newData);
        }
		CurrUser.userAnayticsResults = anayticsResults;
	}
	public static void OnUserQuit()
	{
        CurrUser.CheckPointIdx = GameSequencesList.Instance.goToGameGroupIdx;
        var currSequence = GameSequencesList.Instance.GetGameSequence();
        CurrUser.CheckPointSubIdx = currSequence.GetCurrItemIdx();
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

        GetAllAnalyticsData();
        //TODO ADD A Pause here so that the player can't leave if the data hasn't been fully saved yet
        UserDataManager.Instance.SaveDataToRemoteDataBase();
    }
	static bool WantsToQuit()
	{
		OnUserQuit();
		return true;
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
		currUserData = new UserData();
		currUserData.name = email;
		currUserData.id = id;
	}

	public void RegisterNewUser(UserData user)
	{
		currUserData = user;
		usersDatas.Add(currUserData);
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
		var data = usersDatas.Find(x =>x.id == id);
		currUserData = data;
	}

}

