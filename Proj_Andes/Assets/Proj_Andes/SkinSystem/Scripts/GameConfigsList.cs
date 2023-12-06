using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEditor.Tilemaps;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "GameConfigsList", menuName = "GameConfigs/GameConfigsList")]
public class GameConfigsList : ScriptableObject
{
    public static string instancePath = "GameConfigsList";
    static GameConfigsList instance;
    public static GameConfigsList Instance
    {
        get
        {
            if (!instance) instance = Resources.Load<GameConfigsList>(instancePath);
            return instance;
        }
    }

    public List<GameConfig> gameConfigs;
    public int goToSceneIdx;
    public GameConfig GetCurrentGame()
    {
        return gameConfigs[goToSceneIdx];
    }
    public SceneReference GetRandomGame()
    {
        return gameConfigs[Random.Range(0, gameConfigs.Count)].scene;
    }
}

public abstract class GameConfig : ScriptableObject
{
    public abstract SceneReference scene { get; }
}
