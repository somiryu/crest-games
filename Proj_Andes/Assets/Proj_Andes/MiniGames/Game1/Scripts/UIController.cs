using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] Button playBtn;
    [SerializeField] Transform endOfGameContainer;
    [SerializeField] TextMeshProUGUI finishText;
    [SerializeField] TextMeshProUGUI starsText;
    [SerializeField] TextMeshProUGUI turboText;
    [SerializeField] TextMeshProUGUI durationText;
    PlayerController player => PlayerController.Instance;
    public void Init()
    {
        playBtn.onClick.AddListener(player.Play);
        endOfGameContainer.gameObject.SetActive(false);
    }
    public void EndOfGame()
    {
        starsText.text = "Stars collected: " + player.data.starsCollected;
        turboText.text = "Turbo used " + player.data.turboSelectedTime + "s";
        durationText.text = "Ride duration " + player.data.totalRideDuration + "s";
        endOfGameContainer.gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

}
