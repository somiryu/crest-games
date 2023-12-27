using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Gratification_TurboRocket_UIController : MonoBehaviour
{
    [SerializeField] Button playBtn;
    [SerializeField] Transform endOfGameContainer;
    [SerializeField] GameObject inGameObj;
    [SerializeField] GameObject inGameObj2;
    [SerializeField] GameObject inGameObjUI;
    [SerializeField] TextMeshProUGUI finishText;
    [SerializeField] TextMeshProUGUI starsText;
    [SerializeField] Slider progressSlider;
    Gratification_TurboRocket_PlayerController player => Gratification_TurboRocket_PlayerController.Instance;
    public void StartUi()
    {
        endOfGameContainer.gameObject.SetActive(false);
        inGameObj.gameObject.SetActive(true);
        inGameObj2.gameObject.SetActive(true);
        inGameObjUI.gameObject.SetActive(true);
    }

	private void Update()
	{
        if (!player.onPlay) return;
        progressSlider.value = player.CurrProgress;
	}

	public void EndOfGame()
    {
        progressSlider.value = 1;
        starsText.text =  player.data.starsCollected.ToString();
        inGameObj.gameObject.SetActive(false);
        inGameObj2.gameObject.SetActive(false);
        inGameObjUI.gameObject.SetActive(false);
        endOfGameContainer.gameObject.SetActive(true);

    }


}
