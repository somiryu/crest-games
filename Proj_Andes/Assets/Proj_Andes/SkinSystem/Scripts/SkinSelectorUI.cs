using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelectorUI : MonoBehaviour
{
    public List<BtnPerSkinType> btnsPerSkins = new List<BtnPerSkinType>();
	public Button goToGame;
	[SerializeField] SimpleGameSequenceItem skinSelectorItem;
	AudioSource audioSource;
    [SerializeField] Transform blockingPanel;
    [SerializeField] AudioClip pickASkinAudio;
    bool firstAssignFlag;
    private void Awake()
	{
		for (int i = 0; i < btnsPerSkins.Count; i++)
		{
			var curr = btnsPerSkins[i];
			curr.btn.onClick.AddListener(() => GetSkinTypeSelection(curr.skinType, curr.skinTypeAudio));
		}
		goToGame.onClick.AddListener(skinSelectorItem.OnSequenceOver);
		firstAssignFlag = false;
		TryGetComponent(out audioSource);
	}
    private void Start()
    {
		StartCoroutine(Instructions());
    }
    IEnumerator Instructions()
    {
        blockingPanel.gameObject.SetActive(true);
        audioSource.clip = pickASkinAudio;
        audioSource.Play();
        yield return new WaitForSeconds(pickASkinAudio.length);
        blockingPanel.gameObject.SetActive(false);
    }


    public void GetSkinTypeSelection(SkinType skinType, AudioClip skinAudio)
    {
		audioSource.clip = skinAudio;
		audioSource.Play();
        if (SceneManagement.currSkinType == skinType && firstAssignFlag) return;
		firstAssignFlag = true;
        SkinManager.Instance.SetSkin(skinType);
        SceneManagement.currSkinType = skinType;
		if(AppSkipSceneButton.Instance) AppSkipSceneButton.Instance.skinSelector.SetValueWithoutNotify((int)skinType);
		for (int i = 0;i < btnsPerSkins.Count;i++)
		{
			btnsPerSkins[i].highlight.gameObject.SetActive(skinType == btnsPerSkins[i].skinType);
			btnsPerSkins[i].shadow.gameObject.SetActive(skinType == btnsPerSkins[i].skinType);
			btnsPerSkins[i].btn.image.color = skinType == btnsPerSkins[i].skinType? Color.white : new Color(1, 1, 1, 0.2f);
		}
    }

}

[Serializable]
public struct BtnPerSkinType
{
    public SkinType skinType;
    public Button btn;
	public Image highlight;
	public Image shadow;
	public AudioClip skinTypeAudio;
}
