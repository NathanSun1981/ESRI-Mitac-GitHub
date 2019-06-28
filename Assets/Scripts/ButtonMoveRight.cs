using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;

public class ButtonMoveRight : MonoBehaviour {

	// Use this for initialization
    public void OnSelect()
    {

        if (this.transform.parent.gameObject.name.Contains("Building"))
        {
            this.transform.parent.gameObject.GetComponent<BuildingMenu>().building.SendMessage("OnClickMoveRight");
        }
        else
        {
            GameObject terrain = GameObject.Find("TerrainMap");
            terrain.SendMessage("OnClickMoveRight");
        }

    }
}
