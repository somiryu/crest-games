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
    void Update()
    {
        
    }
    void PlayGame()
    {
        SceneManagement.GoToScene(GameSequencesList.Instance.GetGameSequence().GetNextItem().scene);
    }
}
