using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tymski;
using UnityEngine.UI;

public class SplashArtsManager : MonoBehaviour
{
    static SplashArtsManager instance;
    public static SplashArtsManager Instance => instance;
    [SerializeField] Transform Panel;
    [SerializeField] Transform logo1;
    [SerializeField] Transform logo2;
    [SerializeField] int waitFor;
    WaitForSeconds waitSplashArt;
    [SerializeField] SceneReference mainMenuScene;
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
        logo1.gameObject.SetActive(false);
        logo2.gameObject.SetActive(false);
        waitSplashArt = new WaitForSeconds(waitFor);
        StartCoroutine(SplasArtsDisplay());
    }
    public IEnumerator SplasArtsDisplay()
    {
        logo1.gameObject.SetActive (true);
        yield return waitSplashArt;
        logo1.gameObject.SetActive(false);
        logo2.gameObject.SetActive(true);
        yield return waitSplashArt;
        SceneManagement.GoToScene(mainMenuScene);
    }
}
