using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gratification_TurboRocket_CameraController : MonoBehaviour
{
    [SerializeField] iTurboRocketManager player => iTurboRocketManager.Instance;

    [SerializeField] float visualOffset = -4.7f;
    [SerializeField] Vector2 endSequenceCamPos = new Vector2(1, 2);
    [SerializeField] float movPercent = 1f;
    [SerializeField] float moveSpeed = 1f;
    float targetY;
    

    Vector2 targetPos;

    Vector3 lastPlayerPos;

    private void Awake()
    { 
        targetPos = Vector3.right * -visualOffset;
    }



    public void Init()
    {
        lastPlayerPos = player.CurrPos;
		var currPos = transform.position;
		currPos.y = lastPlayerPos.y;
		currPos.x = lastPlayerPos.x;
		transform.position = currPos;
	}

    void Update()
    {
        var currPlayerPos = player.CurrPos;
        if (!player.onDoneAnim) return;

        if (player.onPlay)
        {
            var verticalDisplacement = (currPlayerPos.y - lastPlayerPos.y) * movPercent;
            targetY = transform.position.y + verticalDisplacement;
        }
        var currPos = transform.position;
        currPos.x = Mathf.MoveTowards(currPos.x, targetPos.x + player.CurrPos.x, player.levelConfig.accelerationSpeed * 4 * Time.deltaTime);
        currPos.y = Mathf.MoveTowards(currPos.y, targetY, player.levelConfig.accelerationSpeed * 4 * Time.deltaTime);

        transform.position = new Vector3(currPos.x, targetY, currPos.z);
        lastPlayerPos = currPlayerPos;


    }
    public void OnEnterTurbo()
    {        
        targetPos.x = 0;        
    }
    public void OnExitTurbo()
    {
        if (!player.onPlay) return;
        targetPos.x = -visualOffset;
    }

    public void OnGameFinishedSequence()
    {
        targetPos.x = endSequenceCamPos.x;
        targetY = endSequenceCamPos.y;
    }
}
