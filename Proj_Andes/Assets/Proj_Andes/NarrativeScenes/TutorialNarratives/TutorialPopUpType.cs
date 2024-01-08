using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TutorialPopUpType : MonoBehaviour
{
    [SerializeField] Transform tutoObj;
    [SerializeField] List<PopUpObj> popUpObjs = new List<PopUpObj>();
    PopUpObj currPopUp;
    void Start()
    {
        for (int i = 0; i < popUpObjs.Count; i++) popUpObjs[i].stepIdx = i;
        PopObject(popUpObjs[0]);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GoToNextStep();
        }
    }
    void GoToNextStep()
    {
        HideObject(currPopUp);
        PopObject(popUpObjs[currPopUp.stepIdx+1]);
    }
    void PopObject(PopUpObj tutoObj)
    {
        currPopUp = tutoObj;
        tutoObj.popUpObj.gameObject.SetActive(true);
    }
    void HideObject(PopUpObj tutoObj)
    {
        tutoObj.popUpObj.gameObject.SetActive(false);
    }
}

[Serializable]
public class PopUpObj
{
    public int stepIdx;
    public Transform popUpObj;
    public bool completed;
}
