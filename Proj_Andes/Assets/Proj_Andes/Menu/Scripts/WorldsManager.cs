using System.Collections;
using System.Collections.Generic;
using Tymski;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldsManager : MonoBehaviour
{
    [SerializeField] PickAWorld config;
    [SerializeField] Button[] worlds;
    [SerializeField] Sprite worldDone;
    [SerializeField] Sprite worldUndone;
    [SerializeField] WorldStatus[] worldStatusSprite;

    [SerializeField] Button myCollBtn;
    [SerializeField] SceneReference myCollScene;
    int currActivePlanet => config.worldsConfig.gameIndex;
    void Start()
    {
        worlds = GetComponentsInChildren<Button>();
        for (int i = 0; i < worlds.Length; i++) worlds[i].onClick.AddListener(ClickedActivePlanet);
        for (int i = 0; i < worlds.Length; i++) worldStatusSprite = GetComponentsInChildren<WorldStatus>();
        for (int i = 0; i < worldStatusSprite.Length; i++) worldStatusSprite[i].Init();
        SetCurrentProgress();

        myCollBtn.onClick.AddListener(GoToMyCollection);
    }
    void SetCurrentProgress()
    {
        for (int i = 0; i < worlds.Length; i++) worlds[i].interactable = false;
        for (int i = 0; i < currActivePlanet; i++) worldStatusSprite[i].worldStatus.sprite = worldDone;
        for (int i = currActivePlanet+1; i < worldStatusSprite.Length; i++) worldStatusSprite[i].worldStatus.sprite = worldUndone;

        worldStatusSprite[currActivePlanet].worldStatus.sprite = null;
        worlds[currActivePlanet].interactable = true;
    }
    void ClickedActivePlanet()
    {
        config.worldsConfig.OnSequenceOver();
        config.worldsConfig.AssignPlanetIdx();
    }

    void GoToMyCollection()
    {
        SceneManagement.GoToScene(myCollScene);
    }
}
