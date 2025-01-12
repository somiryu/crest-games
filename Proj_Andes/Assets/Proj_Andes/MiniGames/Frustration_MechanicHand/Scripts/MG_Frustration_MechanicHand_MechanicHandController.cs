using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MG_Frustration_MechanicHand_MechanicHandController : MonoBehaviour
{
    public MG_MechanicHand_GameManger gameManager => MG_MechanicHand_GameManger.Instance;
	public float rotSpeedMultiplier => MG_MechanicHand_GameManger.Instance.gameConfigs.hookRotationSpeed ;
	public float hookSpeed => MG_MechanicHand_GameManger.Instance.gameConfigs.hookMoveSpeed;
	public float hookMaxDistance;
    public Transform hookedObj;
    public MG_MechanicHand_Hook hook;
    private BoxCollider hookCollider;

    float mouseDragStartPosition = 0;
    float lastFrameMousePos = 0;
    private bool isDragging = false;
    public float dragDeathZone = 0.02f;
    public bool canDrag = true;

    [SerializeField] Transform startPointLine;
    [SerializeField] Transform endPointLine;
    [SerializeField] LineRenderer lineRenderer;

    [Header("Game Audio")]
    [SerializeField] AudioClip selectingAudio;
    [SerializeField] AudioClip toHookAudio;
    [SerializeField] AudioClip onHookedAudio;
    [SerializeField] AudioClip notHookedAudio;
    private AudioSource audioSource;

    [Header("GameParticles")]
    [SerializeField] ParticleSystem correctParticles;
    [SerializeField] ParticleSystem incorrectParticles;
    [SerializeField] GameObject skinObj;
    [SerializeField] Animator objAnim;

    int throwns;
    float dragTimer;
    IEnumerator hookShootingRoutine;


	public void Init()
	{
        hook.TryGetComponent(out hookCollider);
        hook.Init(this);
        audioSource = GetComponent<AudioSource>();
        lineRenderer.SetPosition(0, startPointLine.position);
        throwns = 0;
    }

    void Update()
    {
        DragBehaviour();
        lineRenderer.SetPosition(1, endPointLine.position);

        //progressSlider.value = player.CurrProgress;

    }

    void DragBehaviour()
    {
        if (MG_MechanicHand_GameManger.Instance.IsOnEndScreen) return;
        if (!canDrag) return;
        if (Input.GetMouseButtonDown(0))
        {
            mouseDragStartPosition = Input.mousePosition.y;
            lastFrameMousePos = Input.mousePosition.y;
        }
        if (Input.GetMouseButton(0))
        {
            dragTimer += Time.deltaTime;
            var diff = mouseDragStartPosition - Input.mousePosition.y;
            if (Mathf.Abs(diff) > dragDeathZone)
            {
                if(isDragging== false)
                {
                    audioSource.Stop();
                    audioSource.clip = selectingAudio;
                    audioSource.Play();
                }
                isDragging = true;
                OnMouseDragging();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                audioSource.Stop();
                if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_MechanicHand_1HoldClickAndMove))
                {
                    TutorialManager.Instance.TurnOffTutorialStep(tutorialSteps.MG_MechanicHand_1HoldClickAndMove);
                }
            }
            isDragging = false;
        }
    }

    void StoreAnalytics()
    {
        throwns++;
        gameManager.currRoundAnalytics.roundCount = throwns;
        gameManager.currRoundAnalytics.thrown = throwns;
        gameManager.currRoundAnalytics.presition = dragTimer;
        gameManager.allRoundAnalytics.Add(gameManager.currRoundAnalytics);
        Debug.Log("new one " + throwns);
        var newRound = new MechHandRoundAnalytics();
        gameManager.currRoundAnalytics = newRound;
        dragTimer = 0;
    }
    public void OnClickSendHook()
    {
        if (hookShootingRoutine != null) return;
		if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_MechanicHand_1HoldClickAndMove)) return;
		if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_MechanicHand_2JustClickToGrab))
		{
			TutorialManager.Instance.TurnOffTutorialStep(tutorialSteps.MG_MechanicHand_2JustClickToGrab);
		}
		ShootHook();
	}


    void OnMouseDragging()
    {
        var currDelta = Input.mousePosition.y - lastFrameMousePos;
        var currAngles = transform.eulerAngles;
        currAngles.z += currDelta * rotSpeedMultiplier * Time.deltaTime;
        currAngles.z = ClampAngle(currAngles.z, -90f, 90f);
        transform.eulerAngles = currAngles;

        lastFrameMousePos = Input.mousePosition.y;
    }

    float ClampAngle(float angle, float from, float to)
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }


    void ShootHook()
    {
        if (hookShootingRoutine != null) StopCoroutine(hookShootingRoutine);
        hookShootingRoutine = SendHookRoutine();
        StartCoroutine(hookShootingRoutine);
        audioSource.Stop();
        audioSource.clip = toHookAudio;
        audioSource.Play();
        StoreAnalytics();

        MG_MechanicHand_GameManger.Instance.clawThrows++;

    }

    RaycastHit[] hitResults = new RaycastHit[20];

    IEnumerator SendHookRoutine()
    {
        canDrag = false;
        MG_MechanicHand_GameManger.Instance.sendHookBtn.interactable = false;
        var originalPosition = hook.transform.localPosition;
        var currPosition = originalPosition;

        var currProgress = gameManager.totalCapturedAsteroids;
        var neededAmount = gameManager.NeededAsteroidsToWin;

        if(MG_MechanicHand_GameManger.Instance.currRound == 2)
        {
            MG_MechanicHand_GameManger.Instance.lostByCheat++;
            StartCoroutine(ShowTrapSign());
            CorrectRotationToFail();
        }
		else if (gameManager.gameConfigs.activeCheats && currProgress >= neededAmount - 1)
        {
            MG_MechanicHand_GameManger.Instance.lostByCheat++;
            StartCoroutine(ShowTrapSign());
			CorrectRotationToFail();
        }
        objAnim.SetTrigger("Open");

        while ((originalPosition - currPosition).magnitude < hookMaxDistance)
        {
            hook.transform.position += transform.right * hookSpeed * Time.deltaTime;
            currPosition = hook.transform.localPosition;
            if (hookedObj) break;
            yield return null;
        }
        if (hookedObj)
        {
            correctParticles.transform.position = hookedObj.position;
            correctParticles.Play();

            audioSource.Stop();
            audioSource.clip = onHookedAudio;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = notHookedAudio;
            audioSource.Play();
        }
        objAnim.SetTrigger("Close");

        //animacion agarrar
        //Esperar 2 segundos
        //Animacion moverse
        while ((currPosition != originalPosition))
        {
            hook.transform.localPosition = Vector3.MoveTowards(hook.transform.localPosition, originalPosition, hookSpeed * Time.deltaTime);
            currPosition = hook.transform.localPosition;
            yield return null;
        }

        objAnim.SetTrigger("Idle");

        canDrag = true;
        hookShootingRoutine = null;
		MG_MechanicHand_GameManger.Instance.sendHookBtn.interactable = true;
		CheckIfScored();
    }


	IEnumerator ShowTrapSign()
	{
		MG_MechanicHand_GameManger.Instance.trapImage.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);
		MG_MechanicHand_GameManger.Instance.trapImage.gameObject.SetActive(false);
	}

	void CorrectRotationToFail()
    {
		var hitAmount = Physics.
		   BoxCastNonAlloc(hook.transform.position, hookCollider.size / 2, transform.right, hitResults, transform.rotation, Mathf.Infinity);

        var correctHit = false;
        audioSource.Stop();
        audioSource.clip = notHookedAudio;
        audioSource.Play();
        for (int i = 0; i < hitAmount; i++)
        {
            var curr = hitResults[i];
            if (curr.collider.tag != MG_MechanicHand_GameManger.ASTEROIDS_TAG) continue;
            correctHit = true;
        }

        if (correctHit)
        {
			var currRotation = transform.eulerAngles;
            currRotation.z += 0.2f;
            transform.eulerAngles = currRotation;
            CorrectRotationToFail();
            audioSource.Stop();
            audioSource.clip = onHookedAudio;
            audioSource.Play();
        }
	}


    void CheckIfScored()
    {
        if (hookedObj)
        {
			gameManager.OnCapturedAsteroid(hookedObj);
            hookedObj = null;
        }
        else gameManager.OnPlayerFailedHook();
	}
}
