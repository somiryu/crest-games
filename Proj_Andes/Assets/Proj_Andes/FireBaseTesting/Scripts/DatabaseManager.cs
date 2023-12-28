using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DatabaseManager
{
    public static string UserDatasJSONKey = "UserDatasList";

    public static UserDataListWrapper UserDataList = new UserDataListWrapper();

    public static List<UserData> GetUserDatasList()
    {
       var jsonData = PlayerPrefs.GetString(UserDatasJSONKey);
        Debug.Log(jsonData);
        if(string.IsNullOrEmpty(jsonData)) return new List<UserData>();

       return JsonUtility.FromJson<UserDataListWrapper>(jsonData).userDatas;
    }

    public static void SaveUserDatasList(List<UserData> userDatas)
    {
        UserDataList.userDatas = userDatas;
        var jsonData = JsonUtility.ToJson(UserDataList);
        PlayerPrefs.SetString(UserDatasJSONKey, jsonData);
    }
}

//Wrapping the list so that the JSON serializer works :| 
[Serializable]
public class UserDataListWrapper
{
    public List<UserData> userDatas;
}
