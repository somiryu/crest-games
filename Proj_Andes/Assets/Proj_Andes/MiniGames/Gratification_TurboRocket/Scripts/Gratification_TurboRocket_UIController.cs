using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Gratification_TurboRocket_UIController : MonoBehaviour
{
    [SerializeField] Button playBtn;
    [SerializeField] Transform endOfGameContainer;
    [SerializeField] GameObject inGameObj;
    [SerializeField] GameObject inGameObj2;
    [SerializeField] GameObject inGameObjUI;
    [SerializeField] TextMeshProUGUI finishText;
    [SerializeField] TextMeshProUGUI starsText;
    public Slider progressSlider;
    iTurboRocketManager player => iTurboRocketManager.Instance;
    IEnumerator cameraInstruction;
    [SerializeField] Camera cam;
    [SerializeField] float timeRef;
    [SerializeField] Animator anim;
    Vector3 resetCameraPos;
    public void StartUi()
    {
        endOfGameContainer.gameObject.SetActive(false);
        inGameObj.gameObject.SetActive(true);
        inGameObj2.gameObject.SetActive(true);
        inGameObjUI.gameObject.SetActive(true);

        resetCameraPos = cam.transform.position;
        
        cameraInstruction = CameraMovement();
        StartCoroutine(cameraInstruction);
    }

	private void Update()
	{
        if (!player.onPlay) return;

        progressSlider.value = player.CurrProgress;
	}

    IEnumerator CameraMovement()
    {
        player.onDoneAnim = false;
        anim.enabled = true;
        player.onPlay = false;
        anim.SetTrigger("RideIntro");
        yield return new WaitForSeconds(timeRef);
        player.onPlay = true;
        anim.enabled = false;
        if(player is TutorialManager_Gratification_TurboRocket tuto)
        {
            tuto.InitTuto();
        }
        player.onDoneAnim = true;
        StopCoroutine(cameraInstruction);   
    }
	public void EndOfGame()
    {

        progressSlider.value = 10;
        starsText.text =  player.starsGatheredCount.ToString();
        inGameObj.gameObject.SetActive(false);
        inGameObj2.gameObject.SetActive(false);
        inGameObjUI.gameObject.SetActive(false);
        endOfGameContainer.gameObject.SetActive(true);

    }


}
