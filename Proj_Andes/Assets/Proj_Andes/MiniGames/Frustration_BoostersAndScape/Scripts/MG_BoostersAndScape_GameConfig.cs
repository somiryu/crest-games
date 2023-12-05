using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "BoostersAndScape_GameConfig", menuName = "MiniGames/BoostersAndScape_GameConfig")]
public class MG_BoostersAndScape_GameConfig : GameConfig
{
    public int boostersPerRun;
    public int forcedFails;
    public float boosterTriggerRate;
    public bool forceToFail;
    public SceneReference gameScene;
    public bool updateScene;

    public override SceneReference scene => gameScene;


    private void OnValidate()
    {
        if (updateScene)
        {
            updateScene = false;
            gameScene = OnSceneChanged(gameScene);
        }
    }
    SceneReference OnSceneChanged(SceneReference scene)
    {
        if(gameScene != scene)
        {
            return scene;
        }
        return gameScene;
    }

}

