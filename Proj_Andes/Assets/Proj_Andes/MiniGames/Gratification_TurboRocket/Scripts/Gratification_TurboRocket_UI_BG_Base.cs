using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gratification_TurboRocket_UI_BG_Base : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI starsScoreText;    

    void Start()
    {
		iTurboRocketManager.Instance.OnScoreChanges = ScoreChanged;
    }

    private void ScoreChanged()
    {
        starsScoreText.text = iTurboRocketManager.Instance.starsGatheredCount.ToString(); 
    }
}
