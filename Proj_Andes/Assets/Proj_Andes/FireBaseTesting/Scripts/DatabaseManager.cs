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

public static class DatabaseManager
{
    public static string UserDatasJSONKey = "UserDatasList";
    public static string pendingUserJSONKey = "PendingUsersList";
    public static string pendingSessionsJSONKey = "PendingSessionsList";

    public static List<UserData> userDatas = new List<UserData>();

	public static List<UserData> pendingUserDatasToUpload = new List<UserData>();
    public static Dictionary<string, Dictionary<string, object>> pendingSessionsToUpload = new Dictionary<string, Dictionary<string, object>>();
    public static bool userListDone = false;
    public static bool UserDeletionCompleted = false;

    static FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
    public static void GetUserDatasList()
    {

        userListDone = false;

        if(userDatas == null) userDatas = new List<UserData>();
        else userDatas.Clear();

        if (!UserDataManager.Instance.HasInternetConnection())
        {
            if(PlayerPrefs.HasKey(UserDatasJSONKey))
            {
                userDatas = JsonConvert.DeserializeObject<List<UserData>>(PlayerPrefs.GetString(UserDatasJSONKey));
            }

            if (PlayerPrefs.HasKey(pendingUserJSONKey))
            {
                pendingUserDatasToUpload = JsonConvert.DeserializeObject<List<UserData>>(PlayerPrefs.GetString(pendingUserJSONKey));
            }
            else pendingUserDatasToUpload = new List<UserData>();

            if (PlayerPrefs.HasKey(pendingSessionsJSONKey))
            {
                pendingSessionsToUpload = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(PlayerPrefs.GetString(pendingSessionsJSONKey));
            }
            else pendingSessionsToUpload = new Dictionary<string, Dictionary<string, object>>();

			userListDone = true;
			return;
        }

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
            var json = JsonConvert.SerializeObject(userDatas);
            PlayerPrefs.SetString(UserDatasJSONKey, json);
            userListDone = true;
        });
    }

  public static async void DeleteUserFromDataList(UserData dataToDelete)
    {
        UserDeletionCompleted = false;
		await db.Collection(DataIds.usersCollection).Document(dataToDelete.name + " " + dataToDelete.id).DeleteAsync()
            .ContinueWithOnMainThread(task =>UserDeletionCompleted = true);
	}


    public static async void SaveUserDatasList(List<UserData> userDatas, Dictionary<string, Dictionary<string, object>> dataPerGame)
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

        if (!UserDataManager.Instance.HasInternetConnection())
        {
            //Save locally
            var idx = pendingUserDatasToUpload.FindIndex(x => x.id == currUserData.id);
            if(idx != -1) pendingUserDatasToUpload[idx] = currUserData;
            else pendingUserDatasToUpload.Add(currUserData);

            var jsonData = JsonConvert.SerializeObject(currUserData);
            PlayerPrefs.SetString(pendingUserJSONKey, jsonData);

            pendingSessionsToUpload.AddRange(dataPerGame);
            var sessionsJSON = JsonConvert.SerializeObject(pendingSessionsToUpload);
            PlayerPrefs.SetString(pendingSessionsJSONKey, sessionsJSON);
            return;
        }


        for (int i = 0; i < userDatas.Count; i++)
        {
            DocumentReference docRef = db.Collection(DataIds.usersCollection).Document(userDatas[i].name + " " + userDatas[i].id);      
            await docRef.SetAsync(userDatas[i]);
        }

        foreach (string gameKey in dataPerGame.Keys)
        {
            CollectionReference collRef = db.Collection(gameKey);
            foreach (string playerId in dataPerGame[gameKey].Keys)
            {
                DocumentReference docRef = collRef.Document(playerId);
                await docRef.SetAsync(dataPerGame[gameKey][playerId]);
            }            
        }
    }
}



[Serializable]
public class FullSessionData
{
    public string TestID;
    public string UserID;
    public List<GameSessionData> gamesDatas;
}

[Serializable]
public class GameSessionData
{
    public string GameID;
    public Dictionary<string, object> analytics;
}


