using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsersListItem : MonoBehaviour
{
    public TMP_Text label;

    public void Init(string labelTxt)
    {
        label.SetText(labelTxt);
    }
}
