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
    [SerializeField] TextMeshProUGUI finishText;
    [SerializeField] TextMeshProUGUI starsText;
    [SerializeField] Slider progressSlider;
    Gratification_TurboRocket_PlayerController player => Gratification_TurboRocket_PlayerController.Instance;
    public void StartUi()
    {
        endOfGameContainer.gameObject.SetActive(false);
    }

	private void Update()
	{
        if (!player.onPlay) return;
        progressSlider.value = player.CurrProgress;
	}

	public void EndOfGame()
    {
        progressSlider.value = 1;
        starsText.text = "Stars collected: " + player.data.starsCollected;
        endOfGameContainer.gameObject.SetActive(true);
    }


}
