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

    public List<GameSequence> gameSequences;
    [NonSerialized] public GameSequenceItem prevGame;

    [NonSerialized] public int goToGameGroupIdx;
    public bool continueToNextItem;
    public void OnValidate()
    {
        if (continueToNextItem)
        {
            continueToNextItem = false;
            if (goToGameGroupIdx >= gameSequences.Count) return;
            if (gameSequences[goToGameGroupIdx] is SimpleGameSequenceItem) goToGameGroupIdx++;

            var newItem = GetGameSequence().GetNextItem();
            if (newItem == null) GoToNextSequence();
            else SceneManagement.GoToScene(newItem.scene);
        }
    }
    public void GoToNextItemInList()
    {
        var nextItem = GetGameSequence().GetNextItem();
        if (nextItem != null) SceneManagement.GoToScene(nextItem.scene);
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

    public GameSequence GetGameSequence()
    {
        return gameSequences[goToGameGroupIdx];
    }

    public void GoToNextSequence()
    {
		goToGameGroupIdx++;

		if (goToGameGroupIdx >= gameSequences.Count)
        {
            goToGameGroupIdx = 0;
            Debug.LogWarning("Game sequence done, restarting the app");
        }

		prevGame = null;

        var newScene = GetGameSequence().GetNextItem().scene;
		SceneManagement.GoToScene(newScene);
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
        GoToItemIdx(subIdx);
	}

}
public abstract class GameSequence : GameSequenceItem
{
    public abstract void OnReset();
    public abstract GameSequenceItem GetNextItem();
    public abstract GameSequenceItem GetItemByIdx(int idx);
	public abstract int GetCurrItemIdx();
    public abstract void OnSequenceOver();
}

public class GameSequenceItem : ScriptableObject
{
    public SceneReference scene;
}

public abstract class GameConfig : GameSequenceItem
{


}

