using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using Tymski;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    static SceneManagement instance;
    public static SceneManagement Instance=>instance;

    public SceneReference currentScene;
    SkinManager skinManager => SkinManager.Instance;
    SkinType currSkinType => GameConfigsList.Instance.skinType;

    private void Awake()
    {
        if(instance)
        {
            if (instance != this) DestroyImmediate(this);
        }
        instance = this;
    }
    void Start()
    {
        if(!skinManager.mainSkinSelectionScreen) skinManager.SetSkin(currSkinType);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetScene()
    {
        currentScene = GameConfigsList.Instance.GetCurrentGame().scene;
        SceneManager.LoadScene(currentScene);
    }
}
