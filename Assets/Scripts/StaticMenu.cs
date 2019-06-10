using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMenu : MonoBehaviour
{
    GameObject tc;
    // Use this for initialization
    void Start()
    {
        tc = GameObject.Find("TerrainMap");
    }
    void FixedUpdate()
    {
        this.transform.position = tc.transform.position + new Vector3(1f, 0.1f, 0.1f);
    }
}
