using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tymski;
using UnityEngine.UI;

public class SplashArtsManager : MonoBehaviour
{
    static SplashArtsManager instance;
    public static SplashArtsManager Instance => instance;
    [SerializeField] Transform logo;
    [SerializeField] int waitFor;
    WaitForSeconds waitSplashArt;
    //[SerializeField] SceneReference mainMenuScene;
    private void Awake()
    {
        if (instance)
        {
            if (instance != this) DestroyImmediate(this);
        }
        instance = this;
        Init();
    }
    void Init()
    {
        logo.gameObject.SetActive(false);
        waitSplashArt = new WaitForSeconds(waitFor);
        StartCoroutine(SplasArtsDisplay());
    }
    public IEnumerator SplasArtsDisplay()
    {
        logo.gameObject.SetActive (true);
        yield return waitSplashArt;
        GameSequencesList.Instance.GoToNextItemInList();
    }
}
