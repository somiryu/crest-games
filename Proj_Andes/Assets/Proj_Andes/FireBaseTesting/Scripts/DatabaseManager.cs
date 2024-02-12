using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DatabaseManager
{
    public static string UserDatasJSONKey = "UserDatasList";
    public static string pendingUserJSONKey = "PendingUsersList";
    public static string pendingSessionsJSONKey = "PendingSessionsList";

    public static List<UserData> userDatas = new List<UserData>();

	public static List<UserData> pendingUserDatasToUpload = new List<UserData>();

    public static Dictionary<string, Dictionary<string, Dictionary<string, object>>> pendingSessionsToUpload 
        = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(); 

    public static bool userListDone = false;
    public static bool UserDeletionCompleted = false;

    static FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

#if UNITY_EDITOR
    [MenuItem("Hi Hat Games/CleanAllOfflineJSONData")]
    public static void CleanAllOfflineJSONData()
    {
        PlayerPrefs.DeleteKey(pendingSessionsJSONKey);
        PlayerPrefs.DeleteKey(pendingUserJSONKey);
        PlayerPrefs.DeleteKey(UserDatasJSONKey);
    }
#endif

   public static void AddPendingUserData(UserData userData)
    {
        var alreadyIn = pendingUserDatasToUpload.FindIndex(x => x.id == userData.id);
        if (alreadyIn != -1) pendingUserDatasToUpload[alreadyIn] = userData;
        else pendingUserDatasToUpload.Add(userData);
    }


public static void GetUserDatasList()
    {

        userListDone = false;

        if(userDatas == null) userDatas = new List<UserData>();
        else userDatas.Clear();

        pendingSessionsToUpload.Clear();
        pendingUserDatasToUpload.Clear();

        LoadFromLocal();

        if (!UserDataManager.Instance.HasInternetConnection())
        {
			userListDone = true;
			return;
        }

        userDatas.Clear();


		Query allUsers = db.Collection(DataIds.usersCollection);
        allUsers.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot allUsersQuerySnapshot = task.Result;
            foreach (DocumentSnapshot documentSnapshot in allUsersQuerySnapshot.Documents)
            {
                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));
                UserData currUserData = documentSnapshot.ConvertTo<UserData>();
				if (userDatas.Exists(x => x.id == currUserData.id))
				{
					Debug.Log("Trying to add user: " + currUserData.name + " " + currUserData.id + " But ID already existed");
					continue;
				}
				userDatas.Add(currUserData);
            }

            if(pendingUserDatasToUpload.Count > 0)
            {
                Debug.Log("Merged local users with remote users");
                for (int i = 0; i < pendingUserDatasToUpload.Count; i++)
                {
                    var currPending = pendingUserDatasToUpload[i];
                    var idxFound = userDatas.FindIndex(x => x.id == currPending.id);
                    if(idxFound != -1) userDatas[idxFound] = currPending;
                    else userDatas.Add(currPending);
				}
                pendingUserDatasToUpload.Clear();
			}


			userListDone = true;
        });
    }

    public static void LoadFromLocal()
    {
		if (PlayerPrefs.HasKey(UserDatasJSONKey))
		{
            var savedJSON = PlayerPrefs.GetString(UserDatasJSONKey);
			var savedUsers = JsonConvert.DeserializeObject<LocalSaveUsersList>(savedJSON);
			if (savedUsers != null && savedUsers.datas != null) userDatas.AddRange(savedUsers.datas);
		}

		if (PlayerPrefs.HasKey(pendingUserJSONKey))
		{
			var pendingUsers = JsonConvert.DeserializeObject<LocalSaveUsersList>(PlayerPrefs.GetString(pendingUserJSONKey));
			if (pendingUsers != null && pendingUsers.datas != null) pendingUserDatasToUpload.AddRange(pendingUsers.datas);
		}
		else pendingUserDatasToUpload = new List<UserData>();

        if (PlayerPrefs.HasKey(pendingSessionsJSONKey))
        {
            var foundPendingSessionsToUpload = JsonConvert.DeserializeObject<FullSessionData>(PlayerPrefs.GetString(pendingSessionsJSONKey));
            if (foundPendingSessionsToUpload != null && foundPendingSessionsToUpload.analytics != null) pendingSessionsToUpload.AddRange(foundPendingSessionsToUpload.analytics);
        }
        else pendingSessionsToUpload = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
	}

  public static async void DeleteUserFromDataList(UserData dataToDelete)
    {
        UserDeletionCompleted = false;
		await db.Collection(DataIds.usersCollection).Document(dataToDelete.name + " " + dataToDelete.id).DeleteAsync()
            .ContinueWithOnMainThread(task =>UserDeletionCompleted = true);
	}


    public static async void SaveUserDatasList(List<UserData> userDatas, 
        Dictionary<string, Dictionary<string, Dictionary<string, object>>> dataPerGame)
    {
        if (UserDataManager.CurrUser.name == "Unnamed")
        {
            Debug.LogWarning("Skipping saving because is a test run");
            return;
        }

        var currUserData = userDatas.Find(x => x.id == UserDataManager.CurrUser.id);
        if (currUserData == null)
        {
            Debug.LogError("No user data found to save for user with ID: " + UserDataManager.CurrUser.id + " and name " + UserDataManager.CurrUser.name);
            return;
        }

        var hasInternetConnection = UserDataManager.Instance.HasInternetConnection();


        foreach(var gameData in dataPerGame)
        {
            if (!pendingSessionsToUpload.TryGetValue(gameData.Key, out var sessionsDatas))
            {
                pendingSessionsToUpload.Add(gameData.Key, gameData.Value);
            }
            else
            {
               foreach(var sessionData in gameData.Value)
                {
                    sessionsDatas.Add(sessionData.Key, sessionData.Value);
                }
            }
        }


		if (!hasInternetConnection)
        {
            AddPendingUserData(currUserData);
        }

		//Save locally
		Debug.Log("Saving to local");
		//all users
		var allUserObj = new LocalSaveUsersList();
		allUserObj.datas = UserDataManager.Instance.usersDatas;
		var allUsersJSON = JsonConvert.SerializeObject(allUserObj);
		PlayerPrefs.SetString(UserDatasJSONKey, allUsersJSON);

		//Pending users
		var saveFile = new LocalSaveUsersList();
		saveFile.datas = pendingUserDatasToUpload;
		var jsonFile = JsonConvert.SerializeObject(saveFile);
		PlayerPrefs.SetString(pendingUserJSONKey, jsonFile);

		//Pending sessions
		var obj = new FullSessionData();
		obj.analytics = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(pendingSessionsToUpload);
		var sessionsJSON = JsonConvert.SerializeObject(obj);
		PlayerPrefs.SetString(pendingSessionsJSONKey, sessionsJSON);


		if (!hasInternetConnection) return;


        for (int i = 0; i < userDatas.Count; i++)
        {
            DocumentReference docRef = db.Collection(DataIds.usersCollection).Document(userDatas[i].name + " " + userDatas[i].id);      
            await docRef.SetAsync(userDatas[i]);
        }


        foreach(var GameDatas in pendingSessionsToUpload)
        {
            CollectionReference collRef = db.Collection(GameDatas.Key);
            Debug.Log("Syncing GAME ID: " + GameDatas.Key);
            foreach(var sessionData in GameDatas.Value)
            {
				Debug.Log("Syncing Test ID: " + GameDatas.Key);
				DocumentReference docRef = collRef.Document(sessionData.Key);
                await docRef.SetAsync(sessionData.Value);
            }
        }

        if (hasInternetConnection)
        {
            pendingSessionsToUpload.Clear();
            pendingUserDatasToUpload.Clear();
            PlayerPrefs.DeleteKey(pendingUserJSONKey);
            PlayerPrefs.DeleteKey(pendingSessionsJSONKey);
        }
    }
}

[Serializable]
public class LocalSaveUsersList
{
    public List<UserData> datas;

}

[Serializable]
public class LocalPendingUserIDsList
{
	public List<string> userIdS;

}



[Serializable]
public class FullSessionData
{
	public Dictionary<string, Dictionary<string, Dictionary<string, object>>> analytics;
}

[Serializable]
public class ScreenSesionData
{
    public Dictionary<string, AnalyticData> analytics;
    public ScreenSesionData(Dictionary<string, object> data)
    {
        analytics = new Dictionary<string, AnalyticData>();
        foreach(var indata in data)
        {
            analytics.Add(indata.Key, new AnalyticData(indata.Value));
        }
    }
}

[Serializable]
public class AnalyticData
{
    public int intValue = -1;
    public string stringValue = null;
    public bool boolValue = false;
    public bool wasBoolValue = false;
    public bool wasIntValue = false;

    public AnalyticData(object value)
    {
        wasBoolValue = false;
        stringValue = null;
        if(value is int newIntValue) 
        {
            wasIntValue = true;
            intValue = newIntValue;
            return;
        }
        else if(value is bool newBoolValue)
        {
            wasBoolValue = true;
            boolValue = newBoolValue;
            return;
        }

        stringValue = value as string;
    }

}


