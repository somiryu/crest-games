using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

public interface IEndOfGameManager
{
    public EndOfGameManager EndOfGameManager { get; }
}
public class EndOfGameManager : MonoBehaviour
{
    public Button resetBtn;
    public Button continueBtn;
    [SerializeField] Transform eogContainer;

    private void Awake()
    {
        continueBtn.onClick.AddListener(ContinueAction);
        resetBtn.onClick.AddListener(ResetAction);
        OnGameStart();
    }
    public void OnGameOver()
    {
        eogContainer.gameObject.SetActive(true);
    }
    public void OnGameStart()
    {
        eogContainer.gameObject.SetActive(false);
    }
    void ContinueAction()
    {
        GameSequencesList.Instance.GoToNextItemInList();
    }
    void ResetAction()
    {
        SceneManagement.GoToScene(SceneManagement.currentScene);
    }
}
