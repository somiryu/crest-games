using System;
using System.Collections.Generic;
using Tymski;
using UnityEngine;
using Random = UnityEngine.Random;

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
            prevGame = nextItem;
            SceneManagement.GoToScene(nextItem.scene);
        }
        else GoToNextSequence();
        Debug.Log(prevGame);
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
		prevGame = null;
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

}

public abstract class GameConfig : SimpleGameSequenceItem
{


}

