using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkinManager : MonoBehaviour
{
    private static SkinManager instance;
    public static SkinManager Instance => instance;

	private List<SkinnableImage> allSkinnableImages = new List<SkinnableImage>();
	public bool mainSkinSelectionScreen;
	private void Awake()
	{
		if(instance != null && instance != this) Destroy(instance);
		instance = this;

		Utility.FindObjectsByType(allSkinnableImages);
	}

	public void SetSkin(SkinType skinToSet)
	{
		for(int i = 0; i < allSkinnableImages.Count; i++)
		{
			allSkinnableImages[i].SetSkinType(skinToSet);
			GameConfigsList.Instance.skinType = skinToSet;
            Debug.Log("setting " + skinToSet + " in " + SceneManagement.Instance.currentScene);

        }
        if (mainSkinSelectionScreen) SceneManagement.Instance.SetScene();
    }


}

public enum SkinType
{
	Galactic,
	flowers,
	Hearths
}
