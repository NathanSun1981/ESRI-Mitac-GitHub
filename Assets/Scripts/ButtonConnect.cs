﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.HoloLens.APP;

public class ButtonConnect : MonoBehaviour {

	// Use this for initialization
    public void OnSelect()
    {
        this.gameObject.SendMessageUpwards("OnConnectClick");
    }
}
