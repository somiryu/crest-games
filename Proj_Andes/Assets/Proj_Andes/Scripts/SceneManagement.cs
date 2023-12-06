using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using Tymski;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManagement
{
    public static SceneReference currentScene;
    public static SkinType currSkinType { get; set; }
    public static void SetGameScene()
    {
        currentScene = GameConfigsList.Instance.GetCurrentGame().scene;
        SceneManager.LoadScene(currentScene);
    }
    public static void GoToScene(SceneReference scene)
    {
        SceneManager.LoadScene(scene);
    }
}
