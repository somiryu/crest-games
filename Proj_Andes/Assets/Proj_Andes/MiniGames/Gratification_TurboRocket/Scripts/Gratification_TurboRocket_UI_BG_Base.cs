using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gratification_TurboRocket_UI_BG_Base : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI starsScoreText;    
    Gratification_TurboRocket_PlayerController player => Gratification_TurboRocket_PlayerController.Instance;

    void Start()
    {
        player.OnScoreChanged += ScoreChanged;
    }

    private void ScoreChanged()
    {
        starsScoreText.text = player.starsGatheredCount.ToString(); 
    }
}
