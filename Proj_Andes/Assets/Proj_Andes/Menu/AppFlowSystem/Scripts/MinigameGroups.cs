using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MiniGameGroup", menuName = "GameSequencesList/MiniGameGroup")]
public class MinigameGroups : GameSequence
{
    public List<GameSequenceItem> miniGamesInGroup;
    [NonSerialized] List<GameSequenceItem> itemsPlayed = new List<GameSequenceItem>();
    [Space]
    public bool randomize;
    [Tooltip("How many games should we play before leaving the group (Only works if randomize is enable), if -1 we will play all items in the group")]
    public int maxItemsToPlayOnRandomize = -1;

    [NonSerialized]
    public int lastPlayedIdx = -1;

    public GameSequenceItem GetNextMiniGame()
    {
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
            GameSequencesList.Instance.prevGame = newGame;

            return newGame;
        }
        else
        {
            GameSequencesList.Instance.prevGame = miniGamesInGroup[0];
            return miniGamesInGroup[0];
        }
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

    GameSequenceItem GetRandomGame()
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
            GameSequencesList.Instance.prevGame = newGame;
            lastPlayedIdx = miniGamesInGroup.IndexOf(newGame);
            return newGame;
        }
        else return GetRandomGame();
    }
    public override void OnSequenceOver()
    {
        GameSequencesList.Instance.GoToNextSequence();
    }

    public override GameSequenceItem GetNextItem()
    {
        return GetNextMiniGame();
    }

	public override GameSequenceItem GetItemByIdx(int idx) => miniGamesInGroup[(int)idx];


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
