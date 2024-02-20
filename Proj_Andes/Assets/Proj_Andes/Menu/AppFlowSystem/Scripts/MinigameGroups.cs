using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MiniGameGroup", menuName = "GameSequencesList/MiniGameGroup")]
public class MinigameGroups : SimpleGameSequenceItem
{
    public List<SimpleGameSequenceItem> miniGamesInGroup;
    [NonSerialized] List<SimpleGameSequenceItem> itemsPlayed = new List<SimpleGameSequenceItem>();
    [Space]
    public bool randomize;
    [Space]
    public bool CanBeOverridenByDebugFlag;
    [Tooltip("How many games should we play before leaving the group (Only works if randomize is enable), if -1 we will play all items in the group")]
    public int maxItemsToPlayOnRandomize = -1;

    [NonSerialized]
    public int lastPlayedIdx = -1;

    public SimpleGameSequenceItem GetNextMiniGame()
    {
        if (CanBeOverridenByDebugFlag)
        {
            var hasOverride = AppSkipSceneButton.RandomNarrativeOverride;
            if(hasOverride != -1)
            {
                if(GameSequencesList.Instance.prevGame == miniGamesInGroup[hasOverride])
                {
                    return null;
                }
                return miniGamesInGroup[hasOverride];
            }
        }
        if (randomize)
        {
            return GetRandomGame();
        }
        if(miniGamesInGroup.Contains(GameSequencesList.Instance.prevGame))
        {
            var lastGameIdx = miniGamesInGroup.IndexOf(GameSequencesList.Instance.prevGame);
            var newGameIdx = lastGameIdx + 1;

            if (newGameIdx >= miniGamesInGroup.Count) return null;

			lastPlayedIdx = newGameIdx;
            var newGame = miniGamesInGroup[lastPlayedIdx];
            return newGame;
        }
        else return miniGamesInGroup[0];
    }

    public void SetItemsPlayedData(List<int> itemsIdsPlayed)
    {
        itemsPlayed.Clear();
        for (int i = 0; i < itemsIdsPlayed.Count; i++)
        {
            var item = miniGamesInGroup[itemsIdsPlayed[i]];
            itemsPlayed.Add(item);
		}
	}

    public List<int> GetItemsPlayedData()
    {
        var itemsPlayedIds = new List<int>();
        for (int i = 0; i < itemsPlayed.Count; i++)
        {
            var idx = miniGamesInGroup.IndexOf(itemsPlayed[i]);
            itemsPlayedIds.Add(idx);
        }
        return itemsPlayedIds;
    }

    SimpleGameSequenceItem GetRandomGame()
    {
        if(lastPlayedIdx != -1 && !itemsPlayed.Contains(GameSequencesList.Instance.prevGame))
        {
            itemsPlayed.Add(GameSequencesList.Instance.prevGame);
            Debug.Log("Saved played ID: " +  lastPlayedIdx);
        }
		var maxItemsToPlay = maxItemsToPlayOnRandomize != -1 ? maxItemsToPlayOnRandomize : miniGamesInGroup.Count;
        if(itemsPlayed.Count >= maxItemsToPlay) return null;

        var newGame = miniGamesInGroup[Random.Range(0, miniGamesInGroup.Count)];
        if (!itemsPlayed.Contains(newGame))
        {
            lastPlayedIdx = miniGamesInGroup.IndexOf(newGame);
            return newGame;
        }
        else return GetRandomGame();
    }
    public override void OnSequenceOver()
    {
        GameSequencesList.Instance.GoToNextSequence();
    }

    public override SimpleGameSequenceItem GetNextItem()
    {
        return GetNextMiniGame();
    }

	public override SimpleGameSequenceItem GetItemByIdx(int idx) => miniGamesInGroup[(int)idx];


	public override void OnReset()
    {
        itemsPlayed.Clear();
        lastPlayedIdx = -1;
    }

    public override int GetCurrItemIdx() => lastPlayedIdx;

    public override Dictionary<string, object> GetAnalytics()
    {
        return itemAnalytics;
    }
}
