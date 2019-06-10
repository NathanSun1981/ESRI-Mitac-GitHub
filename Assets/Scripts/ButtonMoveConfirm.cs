using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;

public class ButtonMoveConfirm : MonoBehaviour {

	// Use this for initialization
    public void OnSelect()
    {
        GameObject terrain = GameObject.Find("TerrainMap");
        terrain.SendMessage("OnClickMoveConfirm", this.transform.parent.transform.position);
    }
}
