using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;

public class ButtonExit : MonoBehaviour {

	// Use this for initialization
    public void OnSelect()
    {
        if (this.transform.parent.gameObject.name.Contains("Building"))
        {
            this.transform.parent.gameObject.GetComponent<BuildingMenu>().building.SendMessage("OnExitClick", this.transform.parent.gameObject);
        }
        else
        {
            this.gameObject.SendMessageUpwards("OnExitClick");
        }
       
    }
}
