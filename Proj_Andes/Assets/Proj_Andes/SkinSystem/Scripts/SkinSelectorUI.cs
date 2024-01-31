using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelectorUI : MonoBehaviour
{
    public List<BtnPerSkinType> btnsPerSkins = new List<BtnPerSkinType>();
	public Button goToGame;
	[SerializeField] SimpleGameSequenceItem skinSelectorItem;

	bool firstAssignFlag;
    private void Awake()
	{
		for (int i = 0; i < btnsPerSkins.Count; i++)
		{
			var curr = btnsPerSkins[i];
			curr.btn.onClick.AddListener(() => GetSkinTypeSelection(curr.skinType));
		}
		goToGame.onClick.AddListener(skinSelectorItem.OnSequenceOver);
		firstAssignFlag = false;
	}

	

	public void GetSkinTypeSelection(SkinType skinType)
    {
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
}
