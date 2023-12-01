using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_Frustration_MechanicHand_MechanicHandController : MonoBehaviour
{
    public float yRotLimit;
    public float rotSpeed = 1;
    public float movingSmootherBy = 0.2f;
    private Vector3 refSpeed = Vector3.zero;
    public float yAngle = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            transform.eulerAngles += Vector3.up * mouseX * rotSpeed * Time.deltaTime;

            var currAngles = transform.eulerAngles;
            /*
            yAngle += -mouseY * rotSpeed * Time.deltaTime;
            yAngle = Mathf.Clamp(yAngle, -yRotLimit, yRotLimit);
            currAngles.x = yAngle;
            transform.eulerAngles = currAngles;*/
        }

    }
}
