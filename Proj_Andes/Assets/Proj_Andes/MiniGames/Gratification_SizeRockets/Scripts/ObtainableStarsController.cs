using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObtainableStarsController : MonoBehaviour
{
    [SerializeField] Transform starsParent;
    public Star[] stars;
    [SerializeField] int starsAmount;
    [HideInInspector] public List<Star> starsInUse = new List<Star>();
    void Awake()
    {
        stars = starsParent.GetComponentsInChildren<Star>();
        for (int i = 0; i < stars.Length; i++) stars[i].DeactivateStar();
        starsInUse.Clear();
    }

    public void ActivateStars(int starsAmt)
    {
        starsInUse.Clear();
        for (int i = 0; i < starsAmt; i++)
        {
            stars[i].ActivateStar();
            starsInUse.Add(stars[i]);
        }
    }
}
