using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gratification_TurboRocket_UI_BG_Base : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI starsScoreText;    
    iTurboRocketManager player => iTurboRocketManager.Instance;

    void Start()
    {
        player.OnScoreChanges += ScoreChanged;
    }

    private void ScoreChanged()
    {
        starsScoreText.text = player.starsGatheredCount.ToString(); 
    }
}
