using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class Star : MonoBehaviour
{
    ObtainableStarsController obtainableStarsController;
    [SerializeField] Image starImage;
    public bool active;
    public void Init(ObtainableStarsController _obtainable)
    {
        obtainableStarsController = _obtainable;
        active = false;
        DeactivateStar();
    }
    public void ActivateStar()
    {
        active = true;
        gameObject.SetActive(true);
    }
    public void DeactivateStar()
    {
        if (active) obtainableStarsController.RemoveStarInUse(this);
        active = false;
        gameObject.SetActive(false);
    }

}
