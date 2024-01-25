using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SkinnableImageRandom : MonoBehaviour
{
    public bool assignOnAwake = false;

	[SerializeField] Image image;
	public SpriteRenderer sRenderer;
	[SerializeField] RandomImagePerSkinType[] imagePerSkinTypes;

    private void OnValidate()
    {
        if (image == null) TryGetComponent(out image);
        if (sRenderer == null) TryGetComponent(out sRenderer);
    }

	private void Awake()
	{
        if (assignOnAwake)
        {
            SkinManager.Instance.RegisterSkinRandomImg(this);
            SetSkinType(SkinManager.Instance.GetCurrSkin());
        }
	}

	public void SetSkinType(SkinType skinType)
    {
		Sprite sprite = null;
        for (int i = 0; i < imagePerSkinTypes.Length; i++)
        {
            if (imagePerSkinTypes[i].skinType == skinType)
            {
                sprite = imagePerSkinTypes[i].GetRandom();
                break;
            }
        }
        if(image) image.sprite = sprite;
        if(sRenderer) sRenderer.sprite = sprite;
    }
}

[Serializable]
public struct RandomImagePerSkinType
{
    public SkinType skinType;
    public Sprite[] sprites;

    public Sprite GetRandom()
    {
        var randomIndex = Random.Range(0, sprites.Length);
        return sprites[randomIndex];
    }
}
