using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings.SplashScreen;

public class AppSkipSceneButton : MonoBehaviour
{
    public static string instancePrefabPath = "AppSkipSceneButton";
    static AppSkipSceneButton instance;
    public static AppSkipSceneButton Instance => instance;
    [SerializeField] Button skipSceneBtn;

    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        if(instance == null)
        {
            var instancePref = Resources.Load<AppSkipSceneButton>(instancePrefabPath);
            instance = GameObject.Instantiate(instancePref);
        }
    }
    private void Awake()
    {
        if(instance != null && instance != this) DestroyImmediate(instance);
        instance = this;
        Object.DontDestroyOnLoad(this);

        skipSceneBtn.onClick.AddListener(GameSequencesList.Instance.GoToNextItemInList);
    }
}