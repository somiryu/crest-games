using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkinManager : MonoBehaviour
{
    private static SkinManager instance;
    public static SkinManager Instance => instance;

	List<SkinnableImage> allSkinnableImages = new List<SkinnableImage>();
	List<SkinnableObject> allItems = new List<SkinnableObject>();
	public Transform currentSkinObject;
	private void Awake()
	{
		if(instance != null && instance != this) Destroy(instance);
		instance = this;

		Utility.FindObjectsByType(allSkinnableImages);
		Utility.FindObjectsByType(allItems);
		SetSkin(SceneManagement.currSkinType);
    }
    public void SetSkin(SkinType skinToSet)
	{
		for(int i = 0; i < allSkinnableImages.Count; i++)
		{
			allSkinnableImages[i].SetSkinType(skinToSet);
        }
		for (int i = 0; i < allItems.Count ; i++)
		{
            allItems[i].SwitchItem(skinToSet, out currentSkinObject);
        }
    }

}

public enum SkinType
{
	Galactic,
	flowers,
	Hearths
}
