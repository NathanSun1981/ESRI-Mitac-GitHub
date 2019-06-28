using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;

public class ButtonZoomOut : MonoBehaviour {

    // Use this for initialization
    public void OnSelect()
    {
        if (this.transform.parent.gameObject.name.Contains("Building"))
        {
            this.transform.parent.gameObject.GetComponent<BuildingMenu>().building.SendMessage("OnClickZoomOut");
        }
        else
        {
            GameObject terrain = GameObject.Find("TerrainMap");
            terrain.SendMessage("OnClickZoomOut");
        }
    }
}
