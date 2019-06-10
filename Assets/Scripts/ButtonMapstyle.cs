using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;
using UnityEngine.UI;

public class ButtonMapstyle : MonoBehaviour {

    // Use this for initialization
    // Use this for initialization
    public void Start()
    {
        GameObject tc = GameObject.Find("TerrainMap");
        TerrainMap terrainmap = tc.GetComponent<TerrainMap>();
        if (terrainmap.currentStyle == "satellite")
        {
            GetComponentInChildren<Text>().text = "street";
        }
        else
        {
            GetComponentInChildren<Text>().text = "satellite";
        }
    }

    public void OnSelect()
    {
        string text = GetComponentInChildren<Text>().text;
        if (text == "satellite")
        {
            GetComponentInChildren<Text>().text = "street";
        }
        else
        {
            GetComponentInChildren<Text>().text = "satellite"; 
        }
        GameObject terrain = GameObject.Find("TerrainMap");
        terrain.SendMessage("OnChangeMapstyle", text);
    }
}
