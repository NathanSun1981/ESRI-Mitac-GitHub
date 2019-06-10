﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StableMenu : MonoBehaviour
{
    private GameObject terrain;
    // Use this for initialization
    private void Start()
    {
        
    }

    void FixedUpdate()
    {
        Quaternion default_rotation = Quaternion.Euler(0, 0, 0);
        Vector3 directionToTarget = Camera.main.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-directionToTarget) * default_rotation;
    }
}
