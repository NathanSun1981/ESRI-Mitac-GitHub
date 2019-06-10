using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;

public class ButtonShowAddress : MonoBehaviour {

	// Use this for initialization
    public void OnSelect()
    {
        GameObject terrain = GameObject.Find("TerrainMap");
        terrain.SendMessage("OnClickShowAddress", this.transform.parent.transform.position);
        DestroyObject(this.transform.parent.gameObject);
    }
}
