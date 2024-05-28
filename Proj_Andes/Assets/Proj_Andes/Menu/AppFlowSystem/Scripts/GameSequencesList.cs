using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


[CreateAssetMenu(fileName = "GameSequencesList  ", menuName = "GameSequencesList/GameSequencesList")]
public class GameSequencesList : ScriptableObject
{
    public static bool isTheNarrativeSequence = false;
    public static string gameInstancePath = "GameSequencesList";
    public static string narrativeInstancePath = "NarrativeSequencesList";
    static GameSequencesList instance;

    public static GameSequencesList Instance
    {
        get
        {
            if (!instance)
            {
                if(isTheNarrativeSequence) instance = Resources.Load<GameSequencesList>(narrativeInstancePath);
                else instance = Resources.Load<GameSequencesList>(gameInstancePath);
            }
            return instance;
        }
    }

    public List<SimpleGameSequenceItem> gameSequences = new List<SimpleGameSequenceItem>();
    [NonSerialized] public SimpleGameSequenceItem prevGame;
    [HideInInspector] public SimpleGameSequenceItem currItem => gameSequences[goToGameGroupIdx];
    [NonSerialized] public int goToGameGroupIdx;
    public bool continueToNextItem;

    public static int firstFrustrationScreenIdx;
    public static int LastFrustrationScreenIdx;

    public void OnValidate()
    {
        if (continueToNextItem)
        {
            continueToNextItem = false;
            GoToNextItemInList();
        }
    }

    public void GoToNextItemInList() => GoToNextItemInList(true);

	public void GoToNextItemInList(bool loadScene = true)
    {
        var nextItem = GetGameSequence().GetNextItem();
        if (nextItem != null)
        {
			if (prevGame != null)
            {
                prevGame.SaveAnalytics();
                if (prevGame.shouldTryToSaveGeneralAnalytics)
                {
                    Debug.Log("Called try to save general game analytics");
                    prevGame.SaveGeneralGameAnalytics();
                    prevGame.AferGeneralAnalyticsSaved();
                }
                DatabaseManager.SaveUserDatasList(UserDataManager.Instance.usersDatas, UserDataManager.userAnayticsPerGame, false);
            }

			prevGame = nextItem;

            AudioManager.Instance.PlayMusic();
            TimeManager.Instance.ResetUsers();
			if(loadScene) SceneManagement.GoToScene(nextItem.scene);
        }
        else GoToNextSequence();
    }


    public void GoToItemIdx(int idx)
    {
        AudioManager.Instance.PlayMusic();
        var nextItem = GetGameSequence().GetItemByIdx(idx);
        SceneManagement.GoToScene(nextItem.scene);
    }

    [ContextMenu("ResetSequence")]
    public void ResetSequence()
    { 
        prevGame = null;
        goToGameGroupIdx = 0;
        for (int i = 0; i < gameSequences.Count; i++) gameSequences[i].OnReset();
        MonsterMarketConfig.marketAppearTimes = -1;
		MG_SizeRockets_GameConfigs.postFrustration = 0;
		MG_VoiceStarOrFlowerGameConfigs.postFrustration = 0;
		MG_HearthAndStarsGameConfigs.postFrustration = 0;
		Gratification_TurboRocket_GameConfig.postFrustration = 0;
		CleanCurrUserTutorial();
    }

    public SimpleGameSequenceItem GetGameSequence()
    {
        return gameSequences[goToGameGroupIdx];
    }

    public void ForceAdvanceGameGroupID()
    {
        goToGameGroupIdx++;
	}


    public void GoToNextSequence(bool loadScene = true)
    {
		goToGameGroupIdx++;
		if (goToGameGroupIdx >= gameSequences.Count)
        {
            goToGameGroupIdx = gameSequences.Count-1;
            EndSequence();
            Debug.LogWarning("Game sequence done, restarting the app");
        }
        else GoToNextItemInList(loadScene);
    }

    public void GoToSequenceIdx(int idx, int subIdx)
    {
        goToGameGroupIdx = idx;
        var targetSequence = GetGameSequence();
        if(subIdx != -1 && targetSequence is MinigameGroups group)
        {
            group.lastPlayedIdx = subIdx;
            if (subIdx >= 0) prevGame = group.miniGamesInGroup[subIdx];
        }
        else prevGame = targetSequence;
        GoToItemIdx(subIdx);
	}
    public void EndSequence()
    {
        if (prevGame != null)
        {
            prevGame.SaveAnalytics();
            prevGame.SaveGeneralGameAnalytics();
        }
		Debug.Log("Calling end sequence");
		UserDataManager.OnUserQuit();
        ResetSequence();
        GoToNextItemInList();
    }


    public bool IsLastMarket(SimpleGameSequenceItem item)
    {
        if (item is not MonsterMarketConfig marketItem) return false;
        var lastMarketIdx = -1;
		for (var i = 0; i < gameSequences.Count; i++)
        {
            var curr = gameSequences[i];
            if (curr is not MonsterMarketConfig currMarketToCheck) continue;
			lastMarketIdx += 1;
        }

		var currMarketIdx = MonsterMarketConfig.marketAppearTimes;
		if (lastMarketIdx == currMarketIdx) return true;
        return false;
    }



#if UNITY_EDITOR
    [MenuItem("Hi Hat Games/ Clean tutorial data")]
#endif
    public static void CleanCurrUserTutorial()
    {
        UserDataManager.CurrUser.tutorialStepsDone.Clear();
    }
}

public abstract class GameConfig : SimpleGameSequenceItem
{
}

