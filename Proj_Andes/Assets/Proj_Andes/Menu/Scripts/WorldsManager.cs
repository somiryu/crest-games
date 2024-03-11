using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tymski;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldsManager : MonoBehaviour
{
    public static int index;
    [SerializeField] Button[] worlds;
    [SerializeField] Sprite worldDone;
    [SerializeField] Sprite worldUndone;
    [SerializeField] WorldStatus[] worldStatusSprite;
    AudioSource audioSource;
    [SerializeField] AudioClip pickAWorldAudio;
    [SerializeField] TextMeshProUGUI coinsAmt;
    [SerializeField] MyCollectionManager collectionManager;
    int currActivePlanet => index;

    void Start()
    {
        TryGetComponent(out audioSource);
        audioSource.clip = pickAWorldAudio;
        audioSource.Play();
        collectionManager.Init(true);
        worlds = GetComponentsInChildren<Button>();
        for (int i = 0; i < worlds.Length; i++)
        {
            worlds[i].onClick.AddListener(ClickedActivePlanet);
            var txt = worlds[i].GetComponentInChildren<TextMeshProUGUI>();
            txt.text = (i + 1).ToString();
        }
        for (int i = 0; i < worlds.Length; i++) worldStatusSprite = GetComponentsInChildren<WorldStatus>();
        for (int i = 0; i < worldStatusSprite.Length; i++) worldStatusSprite[i].Init();
        SetCurrentProgress();

        coinsAmt.text = UserDataManager.CurrUser.Coins.ToString();
    }
    void SetCurrentProgress()
    {
        for (int i = 0; i < worlds.Length; i++) worlds[i].interactable = false;
        for (int i = 0; i < currActivePlanet; i++) worldStatusSprite[i].worldStatus.sprite = worldDone;
        for (int i = currActivePlanet+1; i < worldStatusSprite.Length; i++) worldStatusSprite[i].worldStatus.sprite = worldUndone;

        worldStatusSprite[currActivePlanet].worldStatus.gameObject.SetActive(false);
        worlds[currActivePlanet].interactable = true;
    }
    void ClickedActivePlanet()
    {
        GameSequencesList.Instance.GoToNextItemInList();
    }
}
