using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppSkipSceneButton : MonoBehaviour
{
    //Only in the editor and in development builds, release builds never show the debug UI
    public static bool ActiveDebugGlobalUI => Debug.isDebugBuild;

    public static string instancePrefabPath = "DebugGlobalUI";
    static AppSkipSceneButton instance;
    public static AppSkipSceneButton Instance => instance;
    [SerializeField] Button skipSceneBtn;
    public TMP_Dropdown skinSelector;
    public TMP_Dropdown narrativeSelector;
    public TMP_Dropdown voiceOrImageMixedOptionSelections;

    public static int RandomNarrativeOverride = - 1;

    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        if (!ActiveDebugGlobalUI) return;

		if (instance == null)
        {
            var instancePref = Resources.Load<AppSkipSceneButton>(instancePrefabPath);
            instance = GameObject.Instantiate(instancePref);
        }
    }
    private void Awake()
    {
		if (instance != null && instance != this) DestroyImmediate(instance);
        instance = this;
        Object.DontDestroyOnLoad(this);

        skipSceneBtn.onClick.AddListener(GameSequencesList.Instance.GoToNextItemInList);
        skinSelector.onValueChanged.AddListener(ForceSkinChange);
        narrativeSelector.onValueChanged.AddListener(OverrideNarrativeChanged);

        narrativeSelector.gameObject.SetActive(false);
        //The mixed mode always follows the image now, so there is nothing to choose
        voiceOrImageMixedOptionSelections.gameObject.SetActive(false);
        skinSelector.gameObject.SetActive(!GameSequencesList.isTheNarrativeSequence);
	}

    void OverrideNarrativeChanged(int newValue)
    {
        RandomNarrativeOverride = newValue - 1;
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