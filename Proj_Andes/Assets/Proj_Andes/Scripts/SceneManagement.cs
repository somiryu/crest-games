using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using Tymski;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManagement
{
    public static SceneReference currentScene;
    public static SkinType currSkinType {  get; set; }

    public static void SetScene()
    {
        currentScene = GameConfigsList.Instance.GetCurrentGame().scene;
        SceneManager.LoadScene(currentScene);
    }
}
