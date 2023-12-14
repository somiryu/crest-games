using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsersListItem : MonoBehaviour
{
    public TMP_Text label;
    public Button deleteUserBtn;

    public void Init(string labelTxt, FirebaseAnonymousLoginUI logInController)
    {
        label.SetText(labelTxt);
        deleteUserBtn.onClick.RemoveAllListeners();
        deleteUserBtn.onClick.AddListener(() => logInController.RemoveUserOfBtn(this));
	}
}
