using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsController : MonoBehaviour
{
    MeshRenderer rend;
    void Start()
    {
        rend = GetComponentInChildren<MeshRenderer>();
        rend.material.color = Color.yellow;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
