using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class Star : MonoBehaviour
{
    [SerializeField] Image starImage;

    public void ActivateStar()
    {
        starImage.gameObject.SetActive(true);
    }
    public void DeactivateStar()
    {
        starImage.gameObject.SetActive(false);
    }

}
