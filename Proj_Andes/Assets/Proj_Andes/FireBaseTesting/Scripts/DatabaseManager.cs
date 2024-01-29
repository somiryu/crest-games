using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

public static class DatabaseManager
{
    public static string UserDatasJSONKey = "UserDatasList";

    public static UserDataListWrapper UserDataList = new UserDataListWrapper();
    public static List<UserData> userDatas = new List<UserData>();
    public static bool userListDone = false;

    static FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
    public static void GetUserDatasList()
    {

        userListDone = false;

        if(userDatas == null) userDatas = new List<UserData>();
        else userDatas.Clear();

		Query allCitiesQuery = db.Collection(DataIds.usersCollection);
        allCitiesQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
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
            userListDone = true;
        });
        
    }

  
        

    public static async void SaveUserDatasList(List<UserData> userDatas, Dictionary<string, Dictionary<string, object>> dataPerGame)
    {       
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

    public static async void SaveGameDatasList(string colletionGame, string userID, Dictionary<string, object> itemAnalytics)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection(colletionGame).Document(userID);
        Debug.Log("I am here");
        await docRef.SetAsync(itemAnalytics, SetOptions.MergeAll);
    }
}

//Wrapping the list so that the JSON serializer works :| 
[Serializable]
public class UserDataListWrapper
{
    public List<UserData> userDatas;
}

