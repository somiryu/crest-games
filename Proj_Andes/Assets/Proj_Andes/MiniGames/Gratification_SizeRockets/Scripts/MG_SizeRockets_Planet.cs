using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MG_SizeRockets_Planet : MonoBehaviour
{
    public int coinsAmount;

    [SerializeField] ObtainableStarsController obtainableStarsController;
    [SerializeField] AudioSource audioCorrect;
    [SerializeField] SpriteRenderer graphic;
    private bool isStarted;

    [SerializeField] Color normalColor;
    [SerializeField] Color selectedColor;

    public void Init(int _coinsAmount)
    {
        coinsAmount = _coinsAmount;
        obtainableStarsController.ActivateStars(_coinsAmount);
	}

    public void UpdateCoinsAmount(int _coinsAmount)
    {
        GetComponentInChildren<ParticleSystem>().Play();
        audioCorrect.Play();
        for (int i = 0; i < coinsAmount- _coinsAmount; i++) obtainableStarsController.starsInUse[i].DeactivateStar();
        coinsAmount = _coinsAmount;
	}

    //There was a match between a rocket and this planet
    public void OnMatchHappend()
    {
        StartCoroutine(OnMatchHappenedSequence());
    }

    IEnumerator OnMatchHappenedSequence()
    {
		var currProgress = 0.9f;

		while (currProgress < 1.2)
        {
            transform.localScale = Vector3.one * currProgress;
            currProgress += Time.deltaTime * 1.5f;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

		while (currProgress > 1)
		{
			transform.localScale = Vector3.one * currProgress;
			currProgress -= Time.deltaTime * 1.5f;
			yield return null;
		}

		SetSelected(false);
	}



    public void SetSelected(bool selected)
    {
        graphic.color = selected? selectedColor : normalColor;
        transform.localScale = selected? Vector3.one * 1.1f : Vector3.one;
    }
}
