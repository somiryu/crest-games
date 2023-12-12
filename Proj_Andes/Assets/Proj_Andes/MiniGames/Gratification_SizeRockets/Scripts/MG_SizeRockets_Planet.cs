using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MG_SizeRockets_Planet : MonoBehaviour
{
    public int coinsAmount;

    public TMP_Text coinsAmountTxtUI;

    public void Init(int _coinsAmount)
    {
        UpdateCoinsAmount(_coinsAmount);
	}

    public void UpdateCoinsAmount(int _coinsAmount)
    {
		coinsAmount = _coinsAmount;
		coinsAmountTxtUI.SetText(coinsAmount.ToString() + " monedas");
	}
}
