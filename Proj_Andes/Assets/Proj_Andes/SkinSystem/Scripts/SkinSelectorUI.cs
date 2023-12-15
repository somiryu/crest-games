using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelectorUI : MonoBehaviour
{
    public List<BtnPerSkinType> btnsPerSkins = new List<BtnPerSkinType>();
	public Button goToGame;
	[SerializeField] SkinSelectorItem skinSelectorItem;
    private void Awake()
	{
		for (int i = 0; i < btnsPerSkins.Count; i++)
		{
			var curr = btnsPerSkins[i];
			curr.btn.onClick.AddListener(() => GetSkinTypeSelection(curr.skinType));
		}
		goToGame.onClick.AddListener(skinSelectorItem.OnSequenceOver);
	}
    public void GetSkinTypeSelection(SkinType skinType)
    {
        if (SceneManagement.currSkinType == skinType) return;
        SkinManager.Instance.SetSkin(skinType);
        SceneManagement.currSkinType = skinType;
    }

}

[Serializable]
public struct BtnPerSkinType
{
    public SkinType skinType;
    public Button btn;
}
