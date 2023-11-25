using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] Button playBtn;
    [SerializeField] Button replayBtn;
    [SerializeField] Transform endOfGameContainer;
    [SerializeField] TextMeshProUGUI finishText;
    [SerializeField] TextMeshProUGUI starsText;
    [SerializeField] Slider progressSlider;
    //[SerializeField] TextMeshProUGUI turboText;
    //[SerializeField] TextMeshProUGUI durationText;
    PlayerController player => PlayerController.Instance;
    public void StartUi()
    {
        playBtn.onClick.AddListener(
            () =>
            {
                player.Play();
                playBtn.gameObject.SetActive(false);
            });

        endOfGameContainer.gameObject.SetActive(false);
		replayBtn.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single));
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
        //turboText.text = "Turbo used " + player.data.turboSelectedTime + "s";
        //durationText.text = "Ride duration " + player.data.totalRideDuration + "s";
        endOfGameContainer.gameObject.SetActive(true);
    }


}
