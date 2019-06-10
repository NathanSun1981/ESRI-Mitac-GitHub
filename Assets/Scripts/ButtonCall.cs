using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;
using UnityEngine.UI;

public class ButtonCall : MonoBehaviour {

    // Use this for initialization

    public void OnSelect()
    {
        this.gameObject.SendMessageUpwards("OnCallClick");

    }
}
