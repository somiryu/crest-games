using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppSkipSceneButton : MonoBehaviour
{
    public static string instancePrefabPath = "AppSkipSceneButton";
    static AppSkipSceneButton instance;
    public static AppSkipSceneButton Instance => instance;
    [SerializeField] Button skipSceneBtn;
    public TMP_Dropdown skinSelector;

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
        skinSelector.onValueChanged.AddListener(ForceSkinChange);
	}

    void ForceSkinChange(int skinIdx)
    {
        SceneManagement.currSkinType = (SkinType)skinIdx;
        var skinManager = SkinManager.Instance;
        if (skinManager == null) return;
        skinManager.forceSkinType = true;
        skinManager.skinTypeToForce = (SkinType)skinIdx;
        skinManager.SetSkin(skinManager.skinTypeToForce);
    }
}