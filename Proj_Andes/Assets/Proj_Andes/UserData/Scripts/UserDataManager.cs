using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
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


	public UserData DefaultUserData = new UserData();

	[NonSerialized]
	public List<UserData> usersDatas = new List<UserData>();

	[SerializeField] UserData currUserData;

	public UserData CurrUserData
	{
		get 
		{
			if (currUserData != null) return currUserData;
			return DefaultUserData;
		}
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

}

