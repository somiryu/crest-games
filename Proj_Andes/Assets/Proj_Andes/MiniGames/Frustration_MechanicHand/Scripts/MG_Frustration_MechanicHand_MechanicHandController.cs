using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MG_Frustration_MechanicHand_MechanicHandController : MonoBehaviour
{
    public MG_MechanicHand_GameManger gameManager => MG_MechanicHand_GameManger.Instance;
	//Multiplying it because normal values are too large
	public float rotSpeedMultiplier => MG_MechanicHand_GameManger.Instance.gameConfigs.hookRotationSpeed * 0.05f;
	//Multiplying it because normal values are too large
	public float hookSpeed => MG_MechanicHand_GameManger.Instance.gameConfigs.hookMoveSpeed * 0.05f;
	public float hookMaxDistance;
    public Transform hookedObj;
    public MG_MechanicHand_Hook hook;
    private BoxCollider hookCollider;

    float mouseDragStartPosition = 0;
    float lastFrameMousePos = 0;
    private bool isDragging = false;
    public float dragDeathZone = 0.02f;
    bool canDrag = true;

    IEnumerator hookShootingRoutine;


	public void Init()
	{
        hook.TryGetComponent(out hookCollider);
        hook.Init(this);
	}

	void Update()
    {
        DragBehaviour();
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
            var diff = mouseDragStartPosition - Input.mousePosition.y;
            if (Mathf.Abs(diff) > dragDeathZone)
            {
                isDragging = true;
                OnMouseDragging();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (!isDragging)
            {
                ShootHook();
            }
            isDragging = false;
        }
    }

    void OnMouseDragging()
    {
        var currDelta = Input.mousePosition.y - lastFrameMousePos;
        var currAngles = transform.eulerAngles;
        currAngles.z += currDelta * rotSpeedMultiplier;
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
    }

    RaycastHit[] hitResults = new RaycastHit[20];

    IEnumerator SendHookRoutine()
    {
        canDrag = false;
        var originalPosition = hook.transform.localPosition;
        var currPosition = originalPosition;

        var currProgress = gameManager.totalCapturedAsteroids;
        var neededAmount = gameManager.NeededAsteroidsToWin;


		if (gameManager.gameConfigs.activeCheats && currProgress >= neededAmount - 1)
        {
            CorrectRotationToFail();
        }

        while ((originalPosition - currPosition).magnitude < hookMaxDistance)
        {
            hook.transform.position += transform.right * hookSpeed;
            currPosition = hook.transform.localPosition;
            if (hookedObj) break;
            yield return null;
        }
        while ((currPosition != originalPosition))
        {
            hook.transform.localPosition = Vector3.MoveTowards(hook.transform.localPosition, originalPosition, hookSpeed);
            currPosition = hook.transform.localPosition;
            yield return null;
        }

        canDrag = true;
        hookShootingRoutine = null;
        CheckIfScored();
    }

    void CorrectRotationToFail()
    {
		var hitAmount = Physics.
		   BoxCastNonAlloc(hook.transform.position, hookCollider.size / 2, transform.right, hitResults, transform.rotation, Mathf.Infinity);

        var correctHit = false;
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