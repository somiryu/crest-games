using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button playButton;
    
    void Start()
    {
        playButton.onClick.AddListener(PlayGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void PlayGame()
    {
        SceneManagement.GoToScene(GameConfigsList.Instance.GetRandomGame());
    }
}
