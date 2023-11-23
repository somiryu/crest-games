using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurboButton : MonoBehaviour
{
    public CameraController cam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.position;
        newPos.x = cam.transform.position.x;
        transform.position = newPos;
    }
}
