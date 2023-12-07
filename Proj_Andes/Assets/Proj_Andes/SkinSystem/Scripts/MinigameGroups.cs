using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MiniGameGroup", menuName = "GameConfigs/MiniGameGroup")]
public class MinigameGroups : ScriptableObject
{
    public List<GameConfig> miniGamesInGroup;
    public bool randomize;
    public GameConfig prevGame;
    public bool goToNextGame;

    [ContextMenu("ResetSequence")]
    private void ResetSequence()
    {
        prevGame = null;
    }
    public void OnValidate()
    {
        if (goToNextGame)
        {
            goToNextGame = false;
            if (miniGamesInGroup.IndexOf(prevGame) >= miniGamesInGroup.Count-1) SceneManagement.GoToScene(prevGame.scene);
            else SceneManagement.GoToScene(GetNextMiniGame().scene);
        }
    }
    public GameConfig GetNextMiniGame()
    {
        if (randomize)
        {
            return GetRandomGame();
        }
        if(miniGamesInGroup.Contains(prevGame))
        {
            var newGame = miniGamesInGroup[miniGamesInGroup.IndexOf(prevGame) + 1];
            prevGame = newGame;
            return newGame;
        }
        else
        {
            prevGame = miniGamesInGroup[0];
            return miniGamesInGroup[0];
        }
    }

    GameConfig GetRandomGame()
    {
        var newGame = miniGamesInGroup[Random.Range(0, miniGamesInGroup.Count)];
        if (newGame != prevGame)
        {
            return newGame;
        }
        else return GetRandomGame();
    }
}
