using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Academy.HoloToolkit.Unity;
using UnityEngine.Networking;
using System;
using Esri.HoloLens.APP;
using Esri.PrototypeLab.HoloLens.Unity;

public class Buildings : MonoBehaviour {

    public int MoveSpeed = 10;
    private string lockedby;

    private string updateURL = "http://142.104.69.88:8080/update.php";

    private Vector3 navigationPreviousPosition;
    public float MaxScale = 2f;
    public float MinScale = 0.1f;

    public float RotationSensitivity = 25.0f;

    private float rotationFactorY;

    public Canvas MenuControl;

    private Vector3 obj_position;
    private Quaternion obj_rotation;
    private Vector3 obj_scale;
    private DateTime m_datetime;


    enum lockAction
    {
        dounlock = 0,
        dolock
    };

    // Use this for initialization
    void Start () {
        //MenuControl.gameObject.SetActive(false);
        obj_position = gameObject.transform.position;
        obj_rotation = gameObject.transform.rotation;
        obj_scale = gameObject.transform.localScale;

        string assetbundleURL = "http://142.104.69.88:8080/assetbundle/" + gameObject.name + ".assetbundle";
        this.StartCoroutine(this.DownloadSKPModule(assetbundleURL, gameObject.name));
    }
	
	// Update is called once per frame
	void Update () {
        //download the position from database

        DateTime dTimeNow = DateTime.Now;
        TimeSpan ts = dTimeNow.Subtract(m_datetime);
        float tsf = float.Parse(ts.TotalSeconds.ToString());

        if (tsf > 5)
        {
            StartCoroutine(querySQL(updateURL));
            m_datetime = DateTime.Now;
        }

        
        if (gameObject.transform.position != obj_position)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, obj_position, MoveSpeed * Time.deltaTime);
        }

        if (gameObject.transform.rotation != obj_rotation)
        {
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, obj_rotation, Time.deltaTime * 3); ;
        }
        if (gameObject.transform.localScale != obj_scale)
        {
            gameObject.transform.localScale = obj_scale;
        }
        

    }

    // Use this for initialization  
	 /*
    void PerformManipulationStart()
    {
        if ((lockedby == SystemInfo.deviceName) || (lockedby == "none"))
        {
            List<string> list = new List<string>();
            list.Add("locked");
            StartCoroutine(UpdateSQL(updateURL, list, lockAction.dolock));
        }

    }

    void PerformManipulationUpdate(Vector3 position)
    {
        if (GestureManager.Instance.IsManipulating && ((lockedby == SystemInfo.deviceName) || ((lockedby == "none"))))
        {
            try
            {
                if (GazeManager.Instance.HitInfo.collider.gameObject.tag == "Rotate")
                {   //绕Y轴进行旋转
                    rotationFactorY = GestureManager.Instance.ManipulationPosition.x * RotationSensitivity;
                    transform.Rotate(new Vector3(0, rotationFactorY, 0));
                    
                    List<string> list = new List<string>();
                    list.Add("rotation");
                    StartCoroutine(UpdateSQL(updateURL, list, lockAction.dolock));
                }
            }
            catch
            {

            }
        }
    }

    void PerformManipulationCompleted()
    {
        if ((lockedby == SystemInfo.deviceName) || (lockedby == "none"))
        {
            List<string> list = new List<string>();
            list.Add("locked");
            StartCoroutine(UpdateSQL(updateURL, list, lockAction.dounlock));
        }
    }

   
    void PerformNavigationStart(Vector3 position)
    {
        //设置初始位置    
        navigationPreviousPosition = position;
    }

    void PerformZoomUpdate(Vector3 position)
    {
        if (GestureManager.Instance.IsNavigating && HandsManager.Instance.FocusedGameObject == gameObject)
        {
            Vector3 deltaScale = Vector3.zero;
            float ScaleValue = 0.01f;
            //设置每一帧缩放的大小  
            if (position.x < 0)
            {
                ScaleValue = -1 * ScaleValue;
            }
            //当缩放超出设置的最大，最小范围时直接返回  
            if (transform.localScale.x >= MaxScale && ScaleValue > 0)
            {
                return;
            }
            else if (transform.localScale.x <= MinScale && ScaleValue < 0)
            {
                return;
            }
            //根据比例计算每个方向上的缩放大小  
            deltaScale.x = ScaleValue;
            deltaScale.y = (transform.localScale.y / transform.localScale.x) * ScaleValue;
            deltaScale.z = (transform.localScale.z / transform.localScale.x) * ScaleValue;
            transform.localScale += deltaScale;
        }
    }
    void OnHold()
    {
        if ((lockedby == SystemInfo.deviceName) || (lockedby == "none"))
        {
            List<string> list = new List<string>();
            list.Add("locked");
            StartCoroutine(UpdateSQL(updateURL, list, lockAction.dolock));
            //MenuControl.gameObject.SetActive(true);
        }

    }

    void OnSelect()
    {
        if ((lockedby == SystemInfo.deviceName) || (lockedby == "none"))
        {
            List<string> list = new List<string>();
            list.Add("locked");
            StartCoroutine(UpdateSQL(updateURL, list, lockAction.dolock));
            //MenuControl.gameObject.SetActive(true);
        }
    }

    private IEnumerator UpdateSQL(string url, List<string> list, lockAction lockaction)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (string item in list)
        {
            switch(item)
            {
                case "position":
                    formData.Add(new MultipartFormDataSection("positionX", gameObject.transform.position.x.ToString()));                 
                    formData.Add(new MultipartFormDataSection("positionY", gameObject.transform.position.y.ToString()));                    
                    formData.Add(new MultipartFormDataSection("positionZ", gameObject.transform.position.z.ToString()));
                    break;
                case "rotation":
                    formData.Add(new MultipartFormDataSection("rotationX", gameObject.transform.eulerAngles.x.ToString()));                    
                    formData.Add(new MultipartFormDataSection("rotationY", gameObject.transform.eulerAngles.y.ToString()));                    
                    formData.Add(new MultipartFormDataSection("rotationZ", gameObject.transform.eulerAngles.z.ToString()));
                    break;
                case "scale":
                    formData.Add(new MultipartFormDataSection("scaleX", gameObject.transform.localScale.x.ToString()));                    
                    formData.Add(new MultipartFormDataSection("scaleY", gameObject.transform.localScale.y.ToString()));                    
                    formData.Add(new MultipartFormDataSection("scaleZ", gameObject.transform.localScale.z.ToString()));
                    break;

            } 
        }
        formData.Add(new MultipartFormDataSection("itemName", gameObject.name));
        if (lockaction == lockAction.dolock)
        {
            formData.Add(new MultipartFormDataSection("lockedBy", SystemInfo.deviceName));
        }
        else
        {
            formData.Add(new MultipartFormDataSection("lockedBy", "none"));
        }
        

        url += "?action=update";

        //formData.Add(new MultipartFormDataSection("nameid", SystemInfo.deviceName));
        //formData.Add(new MultipartFormDataSection("name", gameObject.name));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.error != "" && www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!" + www.downloadHandler.text);
        }
    }
	*/

    private IEnumerator DownloadSKPModule(string url, string name)
    {

        while (Caching.ready == false)
        {
            yield return null;
        }

        UnityWebRequest www = UnityWebRequest.GetAssetBundle(url, 0);
        yield return www.SendWebRequest();

        if (www.error != "" && www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundle bundle = (www.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
            GameObject obj = (GameObject)Instantiate(bundle.mainAsset, this.gameObject.transform.position + new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
            obj.transform.parent = this.gameObject.transform;
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            bundle.Unload(false);
        }

    }
    private IEnumerator querySQL(string url)
    {

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("itemName", gameObject.name));

        url += "?action=querycoordinate";
        
        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.error != "" && www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form query complete!" + www.downloadHandler.text);

            string xml = www.downloadHandler.text;
            if (xml.Length > 10)
            {            
                string[] buildinginfo = xml.Split('\t');

                float obj_longtitude = float.Parse(buildinginfo[0]);
                float obj_latitude = float.Parse(buildinginfo[1]);

                GameObject tc = GameObject.Find("TerrainMap");
                TerrainMap terrainmap = tc.GetComponent<TerrainMap>();

                obj_rotation = Quaternion.Euler(float.Parse(buildinginfo[2]), float.Parse(buildinginfo[3]), float.Parse(buildinginfo[4]));
                obj_scale = new Vector3(float.Parse(buildinginfo[5]) / Mathf.Pow(2, (21 - terrainmap._place.Level)), float.Parse(buildinginfo[6]) / Mathf.Pow(2, (21 - terrainmap._place.Level)), float.Parse(buildinginfo[7]) / Mathf.Pow(2, (21 - terrainmap._place.Level)));


                var tileUL = terrainmap._place.Location.ToTile(terrainmap._place.Level);
                var tileLR = new Tile()
                {
                    Zoom = tileUL.Zoom,
                    X = tileUL.X + terrainmap.CHILDREN_LEVEL * 2,
                    Y = tileUL.Y + terrainmap.CHILDREN_LEVEL * 2
                };
                var coordUL = tileUL.UpperLeft(terrainmap.CHILDREN_LEVEL);
                var coordLR = tileLR.UpperLeft(terrainmap.CHILDREN_LEVEL);

                // Get tapped location relative to lower left.
                GameObject terrain = GameObject.Find("terrain");

                Vector3 locationonMap = new Vector3();

                locationonMap.x = (obj_longtitude - coordUL.Longitude) / (coordLR.Longitude - coordUL.Longitude) * terrainmap.SIZE;
                locationonMap.z = (1 - (obj_latitude - coordUL.Latitude) / (coordLR.Latitude - coordUL.Latitude)) * terrainmap.SIZE;
                locationonMap.y = 0.02f;

                obj_position = locationonMap + terrain.transform.position;


            }
        }
    }


	
	/*
    private IEnumerator insertSQL(string url)
    {

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("itemName", gameObject.name+"_"+SystemInfo.deviceName));
        formData.Add(new MultipartFormDataSection("positionX", gameObject.transform.position.x.ToString()));
        formData.Add(new MultipartFormDataSection("positionY", gameObject.transform.position.y.ToString()));
        formData.Add(new MultipartFormDataSection("positionZ", gameObject.transform.position.z.ToString()));
        formData.Add(new MultipartFormDataSection("rotationX", gameObject.transform.eulerAngles.x.ToString()));
        formData.Add(new MultipartFormDataSection("rotationY", gameObject.transform.eulerAngles.y.ToString()));
        formData.Add(new MultipartFormDataSection("rotationZ", gameObject.transform.eulerAngles.z.ToString()));
        formData.Add(new MultipartFormDataSection("scaleX", gameObject.transform.localScale.x.ToString()));
        formData.Add(new MultipartFormDataSection("scaleY", gameObject.transform.localScale.y.ToString()));
        formData.Add(new MultipartFormDataSection("scaleZ", gameObject.transform.localScale.z.ToString()));
        formData.Add(new MultipartFormDataSection("lockedBy", "none"));

        url += "?action=insert";

        //formData.Add(new MultipartFormDataSection("nameid", SystemInfo.deviceName));
        //formData.Add(new MultipartFormDataSection("name", gameObject.name));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.error != "" && www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!" + www.downloadHandler.text);
        }
    }

    public static GameObject FindHideChildGameObject(GameObject parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }
        if (parent.transform.childCount < 1)
        {
            return null;
        }
        GameObject obj = null;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject go = parent.transform.GetChild(i).gameObject;
            obj = FindHideChildGameObject(go, childName);
            if (obj != null)
            {
                break;
            }
        }
        return obj;
    }
	*/
}
