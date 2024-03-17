using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


[CreateAssetMenu(fileName = "GameSequencesList", menuName = "GameSequencesList/GameSequencesList")]
public class GameSequencesList : ScriptableObject
{
    public static string instancePath = "GameSequencesList";
    static GameSequencesList instance;

    public static GameSequencesList Instance
    {
        get
        {
            if (!instance) instance = Resources.Load<GameSequencesList>(instancePath);
            return instance;
        }
    }

    public List<SimpleGameSequenceItem> gameSequences;
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

	public void GoToNextItemInList()
    {
        var nextItem = GetGameSequence().GetNextItem();
        if (nextItem != null)
        {
            Debug.Log("saving");
            if (prevGame != null)
            {
                prevGame.SaveAnalytics();
                prevGame.SaveGeneralGameAnalytics();
            }
            prevGame = nextItem;
            AudioManager.Instance.PlayMusic();
            TimeManager.Instance.ResetUsers();
            SceneManagement.GoToScene(nextItem.scene);
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
    }

    public SimpleGameSequenceItem GetGameSequence()
    {
        return gameSequences[goToGameGroupIdx];
    }

    public void GoToNextSequence()
    {
		goToGameGroupIdx++;

		if (goToGameGroupIdx >= gameSequences.Count)
        {
            goToGameGroupIdx = gameSequences.Count-1;
            EndSequence();
            Debug.LogWarning("Game sequence done, restarting the app");
        }
        else GoToNextItemInList();
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
    private static void CleanCurrUserTutorial()
    {
        UserDataManager.CurrUser.tutorialStepsDone.Clear();
    }
}

public abstract class GameConfig : SimpleGameSequenceItem
{
}

