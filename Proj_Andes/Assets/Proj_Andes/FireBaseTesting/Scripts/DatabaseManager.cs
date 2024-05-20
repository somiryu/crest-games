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
    public static bool savingIsDone = false;
    public static bool UserDeletionCompleted = false;
    public static float currLoadProgress = 0;

    public static int testAmountsSavedToDataBase = 0;
    public static int pendingSyncronizedUsersAmount = 0;
    public static int pendingSyncronizedSessionsAmount = 0;

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

    public static void DisableFirebaseOfflineSave()
    {
        db.Settings.PersistenceEnabled = false;
        db.ClearPersistenceAsync();
	}

   public static void AddPendingUserData(UserData userData)
    {
        if (userData == null) return;
        var alreadyIn = pendingUserDatasToUpload.FindIndex(x => x.id_jugador == userData.id_jugador);
        if (alreadyIn != -1) pendingUserDatasToUpload[alreadyIn] = userData;
        else pendingUserDatasToUpload.Add(userData);
    }


    public static void GetUserDatasList()
    {

        userListDone = false;

        if (userDatas == null) userDatas = new List<UserData>();
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
            if (task.IsFaulted) Debug.LogError("FailedToGetUsersDataList");

            foreach (DocumentSnapshot documentSnapshot in allUsersQuerySnapshot.Documents)
            {
                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));
                UserData currUserData = documentSnapshot.ConvertTo<UserData>();
                if (userDatas.Exists(x => x.id_jugador == currUserData.id_jugador))
                {
                    Debug.Log("Trying to add user: " + currUserData.pin + " " + currUserData.id_jugador + " But ID already existed");
                    continue;
                }
                userDatas.Add(currUserData);
            }

            if (pendingUserDatasToUpload.Count > 0)
            {
                for (int i = 0; i < pendingUserDatasToUpload.Count; i++)
                {
                    var currPending = pendingUserDatasToUpload[i];
                    var idxFound = userDatas.FindIndex(x => x.id_jugador == currPending.id_jugador);
                    if (idxFound != -1) userDatas[idxFound] = currPending;
                    else userDatas.Add(currPending);
                }
			}

			userListDone = true;
        });
    }

    public static void LoadFromLocal()
    {
        pendingSyncronizedSessionsAmount = 0;
        pendingSyncronizedUsersAmount = 0;

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
            pendingSyncronizedUsersAmount = pendingUserDatasToUpload.Count;
		}
		else pendingUserDatasToUpload = new List<UserData>();

        if (PlayerPrefs.HasKey(pendingSessionsJSONKey))
        {
            var foundPendingSessionsToUpload = JsonConvert.DeserializeObject<FullSessionData>(PlayerPrefs.GetString(pendingSessionsJSONKey));
            if (foundPendingSessionsToUpload != null && foundPendingSessionsToUpload.analytics != null) pendingSessionsToUpload.AddRange(foundPendingSessionsToUpload.analytics);
            pendingSyncronizedSessionsAmount = pendingSessionsToUpload.Count;
        }
        else pendingSessionsToUpload = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
	}

  public static async void DeleteUserFromDataList(UserData dataToDelete)
    {
        UserDeletionCompleted = false;
		await db.Collection(DataIds.usersCollection).Document(dataToDelete.pin + " " + dataToDelete.id_jugador).DeleteAsync()
            .ContinueWithOnMainThread(task =>UserDeletionCompleted = true);
	}


    public static async void SaveUserDatasList(List<UserData> userDatas, 
        Dictionary<string, Dictionary<string, Dictionary<string, object>>> newCollections, 
        bool mustSaveInDataBase = true,
        bool ignoreEmptyUser = false)
    {
        savingIsDone = false;
        var shouldSkipSaving = false;
#if UNITY_EDITOR
        shouldSkipSaving = FirebaseAnonymousLoginUI.saveOnlyLocalForTesting;
#endif

        if ((UserDataManager.CurrUser.pin == "Unnamed" && !ignoreEmptyUser) || shouldSkipSaving)
        {
            Debug.LogWarning("Skipping saving because is a test run");
			savingIsDone = true;
			return;
        }

        var currUserData = userDatas.Find(x => x.id_jugador == UserDataManager.CurrUser.id_jugador);
        if (currUserData == null && !ignoreEmptyUser)
        {
            Debug.LogError("No user data found to save for user with ID: " + UserDataManager.CurrUser.id_jugador + " and name " + UserDataManager.CurrUser.pin);
            savingIsDone = true;
            return;
        }

        var hasInternetConnection = UserDataManager.Instance.HasInternetConnection();

        AddSessionsToPendingSessions(newCollections);

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


        if (!mustSaveInDataBase)
        {
			savingIsDone = true;
			return;
        }
        if (!hasInternetConnection)
        {
			savingIsDone = true;
			return;
        }

        Debug.LogWarning("IS SAVING TO DATABASE");

        currLoadProgress = 0;
        var currLoadedObjs = 0;
        var totalLoadNeeded = pendingUserDatasToUpload.Count + pendingSessionsToUpload.Count;

		testAmountsSavedToDataBase = 0;


		for (int i = 0; i < pendingUserDatasToUpload.Count; i++)
        {
            currLoadedObjs++;
            currLoadProgress = Mathf.InverseLerp(0, totalLoadNeeded, currLoadedObjs);
            var updatedData = DatabaseManager.userDatas.Find(x => x.id_jugador == pendingUserDatasToUpload[i].id_jugador);
            Debug.Log("Will save user: " + updatedData.pin);

            DocumentReference docRef = db.Collection(DataIds.usersCollection).Document(updatedData.id_jugador);      
            await docRef.SetAsync(updatedData);
        }


        foreach(var GameDatas in pendingSessionsToUpload)
        {
			currLoadedObjs++;
			currLoadProgress = Mathf.InverseLerp(0, totalLoadNeeded, currLoadedObjs);
			CollectionReference collRef = db.Collection(GameDatas.Key);
            Debug.Log("Syncing GAME ID: " + GameDatas.Key);
            if(GameDatas.Key == DataIds.test)
            {
                testAmountsSavedToDataBase = GameDatas.Value.Count;
                Debug.Log("Amount of sesions saved: " + testAmountsSavedToDataBase);
            }
            foreach(var sessionData in GameDatas.Value)
            {
				//Debug.Log("Syncing DOC ID: " + sessionData.Key);
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

        currLoadProgress = 1;

		savingIsDone = true;
        Debug.Log("Finished saving to database");

	}

    //Each time we add something to the list, we need to redo the method, since the lists change causes an error on the iteration
    static void AddSessionsToPendingSessions(Dictionary<string, Dictionary<string, Dictionary<string, object>>> newSessions)
    {
		foreach (var newCollection in newSessions)
		{
			//If the new collection main key is not found
			if (!pendingSessionsToUpload.TryGetValue(newCollection.Key, out var previousPendingCollections))
			{
				if (string.IsNullOrEmpty(newCollection.Key))
				{
					Debug.LogError("trying to add something null " + newCollection.Value);
					continue;
				}
				//Add the new collection
				Debug.Log("Adding new to pending sessions: " + newCollection.Key);
				pendingSessionsToUpload.Add(newCollection.Key, newCollection.Value);
                newSessions.Remove(newCollection.Key);
                AddSessionsToPendingSessions(newSessions);
                return;
			}
			//If the main key of the new collection already existed
			else
			{
				Debug.Log("Adding to already existent pending session: " + newCollection.Key);
				//Go thro all the documents, to see if they already existed
				var documents = newCollection.Value;
				foreach (var newDocument in documents)
				{
					//If the document didn't exist, add it to the existent collection
					if (!previousPendingCollections.TryGetValue(newDocument.Key, out var sessionDat))
					{
						Debug.Log("Adding new document ID: " + newDocument.Key + " to old session: " + newCollection.Key);
						previousPendingCollections.Add(newDocument.Key, newDocument.Value);
                        documents.Remove(newDocument.Key);
                        AddSessionsToPendingSessions(newSessions);
                        return;
					}
					//If it already existed, then override it
					else
					{
						previousPendingCollections[newDocument.Key] = newDocument.Value;
					}
				}
			}
		}
	}

}

[Serializable]
public class LocalSaveUsersList
{
    public List<UserData> datas;

}

[Serializable]
public class FullSessionData
{
	public Dictionary<string, Dictionary<string, Dictionary<string, object>>> analytics;
}


