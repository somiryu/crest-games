using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldStatus : MonoBehaviour
{
    public Image worldStatus;
    public void Init()
    {
        TryGetComponent(out worldStatus);
    }
}
