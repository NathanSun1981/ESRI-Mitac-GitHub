using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingMenu : MonoBehaviour {

    public GameObject building;
    private GameObject terrain;
    // Use this for initialization
    void Start()
    {
        terrain = GameObject.Find("TerrainMap");
    }
    void FixedUpdate()
    {
        this.transform.position = building.transform.position + new Vector3(0f, 0.1f, 0f);
    }
}
