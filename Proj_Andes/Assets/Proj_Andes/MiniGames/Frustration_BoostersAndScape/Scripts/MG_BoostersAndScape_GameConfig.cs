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
    public bool updateScene;

    private void OnValidate()
    {
        if (updateScene)
        {
            updateScene = false;
            scene = OnSceneChanged(scene);
        }
    }
    SceneReference OnSceneChanged(SceneReference sceneRef)
    {
        if(scene != sceneRef)
        {
            return scene;
        }
        return scene;
    }

}

