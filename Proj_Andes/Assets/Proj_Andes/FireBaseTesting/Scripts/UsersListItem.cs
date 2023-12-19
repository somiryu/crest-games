using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsersListItem : MonoBehaviour
{
    public TMP_Text label;
    public Button deleteUserBtn;
    public Button mainBtn;

    public void Init(string labelTxt, FirebaseAnonymousLoginUI logInController)
    {
        label.SetText(labelTxt);
        deleteUserBtn.onClick.RemoveAllListeners();
        deleteUserBtn.onClick.AddListener(() => logInController.RemoveUserOfBtn(this));
		mainBtn.onClick.RemoveAllListeners();
		mainBtn.onClick.AddListener(() => GameSequencesList.Instance.GoToNextSequence());
	}
}
