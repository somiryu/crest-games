using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;

public static class FireStoreManager 
{
    public static async void AddData(string colletionGame, string userID, Dictionary<string, object> itemAnalytics)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection(colletionGame).Document(userID);
        Debug.Log("I am here");
        await docRef.SetAsync(itemAnalytics, SetOptions.MergeAll);
    }


}
