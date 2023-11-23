using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public MovementController player;
    public float visualOffset;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.position;
        if (player.onTurbo) newPos.x = player.transform.position.x;
        else newPos.x = player.transform.position.x + visualOffset;
        transform.position = newPos;
    }
}
