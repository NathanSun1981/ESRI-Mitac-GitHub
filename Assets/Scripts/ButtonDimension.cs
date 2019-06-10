using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;
using UnityEngine.UI;

public class ButtonDimension : MonoBehaviour {

    // Use this for initialization
    public void Start()
    {
        GameObject tc = GameObject.Find("TerrainMap");
        TerrainMap terrainmap = tc.GetComponent<TerrainMap>();
        if (terrainmap.currentDimension == "2D")
        {
            GetComponentInChildren<Text>().text = "3D";
        }
        else
        {
            GetComponentInChildren<Text>().text = "2D";
        }
    } 

    public void OnSelect()
    {
        string text = GetComponentInChildren<Text>().text;
        if (text == "2D")
        {
            GetComponentInChildren<Text>().text = "3D";
        }
        else
        {
            GetComponentInChildren<Text>().text = "2D";
        }
        GameObject terrain = GameObject.Find("TerrainMap");
        terrain.SendMessage("OnChangeDimension", text);
    }
}
