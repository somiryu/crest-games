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
        Query allCitiesQuery = db.Collection(DataIds.usersCollection);
        allCitiesQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot allUsersQuerySnapshot = task.Result;
            foreach (DocumentSnapshot documentSnapshot in allUsersQuerySnapshot.Documents)
            {
                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));
                UserData currUserDate = documentSnapshot.ConvertTo<UserData>();
                userDatas.Add(currUserDate);
            }
            userListDone = true;
        });
        
    }
        

    public static async void SaveUserDatasList(List<UserData> userDatas)
    {
        UserDataList.userDatas = userDatas;
        var jsonData = JsonConvert.SerializeObject(UserDataList);
        Debug.Log("Saving data: " +  jsonData);
        PlayerPrefs.SetString(UserDatasJSONKey, jsonData);

        for (int i = 0; i < userDatas.Count; i++)
        {
            DocumentReference docRef = db.Collection(DataIds.usersCollection).Document(userDatas[i].name + " " + userDatas[i].id);      
            await docRef.SetAsync(userDatas[i]);
        }       
    }
}

//Wrapping the list so that the JSON serializer works :| 
[Serializable]
public class UserDataListWrapper
{
    public List<UserData> userDatas;
}
