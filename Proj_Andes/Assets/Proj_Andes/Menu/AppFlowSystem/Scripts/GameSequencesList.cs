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
            if (prevGame != null) prevGame.SaveAnalytics();
            prevGame = nextItem;
            AudioManager.Instance.PlayMusic();
            SceneManagement.GoToScene(nextItem.scene);
        }
        else GoToNextSequence();
    }
    public void GoToItemIdx(int idx)
    {
        var nextItem = GetGameSequence().GetItemByIdx(idx);
        SceneManagement.GoToScene(nextItem.scene);
    }

    [ContextMenu("ResetSequence")]
    public void ResetSequence()
    {
        prevGame = null;
        goToGameGroupIdx = 0;
        for (int i = 0; i < gameSequences.Count; i++) gameSequences[i].OnReset();
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
            goToGameGroupIdx = 0;
            for (int i = 0; i < gameSequences.Count; i++) gameSequences[i].OnReset();
            Debug.LogWarning("Game sequence done, restarting the app");
        }
		//prevGame = null;
        GoToNextItemInList();
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
        if(prevGame != null) prevGame.SaveAnalytics();
		UserDataManager.OnUserQuit();
        ResetSequence();

        GoToNextItemInList();
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

