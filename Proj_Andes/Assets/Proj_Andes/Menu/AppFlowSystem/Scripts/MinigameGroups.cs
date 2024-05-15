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
    public bool hasNarrativeFixed;
    [Tooltip("How many games should we play before leaving the group (Only works if randomize is enable), if -1 we will play all items in the group")]
    public int maxItemsToPlayOnRandomize = -1;
    [NonSerialized] public int forcedScene;
    [NonSerialized]
    public int lastPlayedIdx = -1;

    [NonSerialized] SimpleGameSequenceItem lastRandomSequenceItemPicked;

    public SimpleGameSequenceItem GetNextMiniGame()
    {
        if (CanBeOverridenByDebugFlag)
        {
            var hasOverride = AppSkipSceneButton.RandomNarrativeOverride;
            if (hasOverride != -1)
            {
                if (GameSequencesList.Instance.prevGame == miniGamesInGroup[hasOverride])
                {
                    return null;
                }
                return miniGamesInGroup[hasOverride];
            }
        }
        if (hasNarrativeFixed)
        {
            if (GameSequencesList.Instance.prevGame == miniGamesInGroup[forcedScene - 1]) return null;
            else return miniGamesInGroup[forcedScene - 1];
        }
        if (randomize)
        {
            return GetRandomGame();
        }
        if (miniGamesInGroup.Contains(GameSequencesList.Instance.prevGame))
        {
            var lastGameIdx = miniGamesInGroup.IndexOf(GameSequencesList.Instance.prevGame);
            var newGameIdx = lastGameIdx + 1;
            Debug.Log(newGameIdx + " minigame " + lastGameIdx);

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
        }
		var maxItemsToPlay = maxItemsToPlayOnRandomize != -1 ? maxItemsToPlayOnRandomize : miniGamesInGroup.Count;
        if (itemsPlayed.Count >= maxItemsToPlay) return null;

        var newGame = miniGamesInGroup[Random.Range(0, miniGamesInGroup.Count)];
        if (!itemsPlayed.Contains(newGame))
        {
            lastPlayedIdx = miniGamesInGroup.IndexOf(newGame);
            lastRandomSequenceItemPicked = newGame;
            return newGame;
        }
        else return GetRandomGame();
    }
    public override void OnSequenceOver()
    {
        lastRandomSequenceItemPicked = null;
        GameSequencesList.Instance.GoToNextSequence();
    }

    public override SimpleGameSequenceItem GetNextItem()
    {
        //Make sure that the curr picked item is really done
        if(lastRandomSequenceItemPicked != null)
        {
            var nextInnerItem = lastRandomSequenceItemPicked.GetNextItem();
            if(nextInnerItem != null)
            {
                return nextInnerItem;
            }
        }

        var nextGame = GetNextMiniGame();
        if (nextGame is MinigameGroups group)
        {
            Debug.Log("going to minigame in minigame");
            var currGameToCheck = group.GetNextMiniGame();
            if (currGameToCheck != null) return currGameToCheck;
            else
            {
                return null;
            }
        }
        else return nextGame;
    }

	public override SimpleGameSequenceItem GetItemByIdx(int idx) => miniGamesInGroup[(int)idx];


	public override void OnReset()
    {
        lastRandomSequenceItemPicked = null;
        itemsPlayed.Clear();
        lastPlayedIdx = -1;
    }

    public override int GetCurrItemIdx() => lastPlayedIdx;

    public override Dictionary<string, object> GetAnalytics()
    {
        return itemAnalytics;
    }
}
