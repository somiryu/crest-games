using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivingWithType : MonoBehaviour
{
    public UserLivingWith livingWithType;
    public Toggle toggle;

    public bool GetValue()
    {
        return toggle.isOn;
    }
}
