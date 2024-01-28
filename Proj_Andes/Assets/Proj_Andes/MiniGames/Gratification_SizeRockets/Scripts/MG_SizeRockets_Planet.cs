using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MG_SizeRockets_Planet : MonoBehaviour
{
    public int coinsAmount;

    public TMP_Text coinsAmountTxtUI;
    [SerializeField] AudioSource audioCorrect;
    private bool isStarted;

    public void Init(int _coinsAmount)
    {
        coinsAmount = _coinsAmount;
        coinsAmountTxtUI.SetText(coinsAmount.ToString());
	}

    public void UpdateCoinsAmount(int _coinsAmount)
    {
        GetComponentInChildren<ParticleSystem>().Play();
        audioCorrect.Play();
		coinsAmount = _coinsAmount;
		coinsAmountTxtUI.SetText(coinsAmount.ToString());
	}
}
