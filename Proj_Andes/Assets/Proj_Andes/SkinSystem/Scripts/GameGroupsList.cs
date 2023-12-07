using System.Collections.Generic;
using Tymski;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "GameConfigsList", menuName = "GameConfigs/GameConfigsList")]
public class GameGroupsList : ScriptableObject
{
    public static string instancePath = "GameConfigsList";
    static GameGroupsList instance;
    public static GameGroupsList Instance
    {
        get
        {
            if (!instance) instance = Resources.Load<GameGroupsList>(instancePath);
            return instance;
        }
    }

    public List<MinigameGroups> gameConfigs;
    public int goToGameGroupIdx;
    public MinigameGroups GetMinigameGroup()
    {
        return gameConfigs[goToGameGroupIdx];
    }
}

public abstract class GameConfig : ScriptableObject
{
    public abstract SceneReference scene { get; }
}
