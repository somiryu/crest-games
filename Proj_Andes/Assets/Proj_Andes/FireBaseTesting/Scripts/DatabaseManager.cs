using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class DatabaseManager
{
    public static string UserDatasJSONKey = "UserDatasList";

    public static UserDataListWrapper UserDataList = new UserDataListWrapper();

    public static List<UserData> GetUserDatasList()
    {
       var jsonData = PlayerPrefs.GetString(UserDatasJSONKey);
        Debug.Log("Data retrieved: " + jsonData);
        if(string.IsNullOrEmpty(jsonData)) return new List<UserData>();


       return JsonConvert.DeserializeObject<UserDataListWrapper>(jsonData).userDatas;
    }

    public static void SaveUserDatasList(List<UserData> userDatas)
    {
        UserDataList.userDatas = userDatas;
        var jsonData = JsonConvert.SerializeObject(UserDataList);
        Debug.Log("Saving data: " +  jsonData);
        PlayerPrefs.SetString(UserDatasJSONKey, jsonData);
    }
}

//Wrapping the list so that the JSON serializer works :| 
[Serializable]
public class UserDataListWrapper
{
    public List<UserData> userDatas;
}
