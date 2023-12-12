using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DatabaseManager
{
    public static string UserDatasJSONKey = "UserDatasList";

    public static List<UserData> GetUserDatasList()
    {
       var jsonData = PlayerPrefs.GetString(UserDatasJSONKey);
        if(string.IsNullOrEmpty(jsonData)) return new List<UserData>();

       return JsonUtility.FromJson<List<UserData>>(jsonData);
    }

    public static void SaveUserDatasList(List<UserData> userDatas)
    {
        var jsonData = JsonUtility.ToJson(userDatas);
        PlayerPrefs.GetString(UserDatasJSONKey, jsonData);
    }
}
