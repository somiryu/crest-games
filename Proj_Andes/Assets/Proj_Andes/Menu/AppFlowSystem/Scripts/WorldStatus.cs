using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldStatus : MonoBehaviour
{
    public Image worldStatus;
    void Start()
    {
        TryGetComponent(out worldStatus);
    }
}
