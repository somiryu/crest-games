using UnityEngine;

public class TutorialPopUpType : MonoBehaviour
{
    [SerializeField] TutorialUser popUpUser;
    private void Start()
    {
        TryGetComponent(out popUpUser);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GoToNextStep();
        }
    }
    void GoToNextStep()
    {
        TutorialManager.Instance.TurnOffTutorial(popUpUser.tutorialStep);
    }
}
