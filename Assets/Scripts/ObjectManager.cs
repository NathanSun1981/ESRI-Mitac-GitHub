using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Esri.HoloLens.APP;
using Esri.PrototypeLab.HoloLens.Unity;

public class ObjectManager : MonoBehaviour {

    // Update is called once per frame
    public string networkAddress;
    public string networkPort;
    //public string buildingName;
    private string updateURL = "http://142.104.69.88:8080/update.php";

    private GameObject obj;
    private DateTime m_datetime;
    public TerrainMap terrainmap; 

    //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch(); 
    

    void Update()
    { 
        DateTime dTimeNow = DateTime.Now;
        TimeSpan ts = dTimeNow.Subtract(m_datetime);
        float tsf = float.Parse(ts.TotalSeconds.ToString());

        if (tsf > 5)
        {
            GameObject tc = GameObject.Find("terrain");
            if (tc != null)
            {
                StartCoroutine(queryCoordinatesSQL(updateURL));
            }
            m_datetime = DateTime.Now;


        }
    }

    // read AssetBundles from web
    IEnumerator GetAssetBundle(string url, Vector3 position, Quaternion rotation, Vector3 scale, string itemName)
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
            obj = (GameObject)Instantiate(bundle.mainAsset, position, rotation);
            obj.name = itemName;
            GameObject terrain = GameObject.Find("TerrainMap");
            obj.transform.parent = terrain.transform;
            obj.transform.localScale = scale;

            //for (var i = 0; i < gameObjects.Length; i++)
            //{
            //obj.transform.parent = gameObjects[i].transform;
            //};
            //stopwatch.Stop(); //  开始监视代码运行时间
            //  获取当前实例测量得出的总时间
            //System.TimeSpan timespan = stopwatch.Elapsed;

            //double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数

            //打印代码执行时间
            //Debug.Log(milliseconds);

            bundle.Unload(false);
        }
    }

     private IEnumerator queryCoordinatesSQL(string url)
    {

        //stopwatch.Start(); //  开始监视代码运行时间
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("itemName", gameObject.name));

        url += "?action=querycoordinates";

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.error != "" && www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form queryall complete!" + www.downloadHandler.text);

            string xml = www.downloadHandler.text;
            if (xml.Length > 10)
            {

                string[] buildinginfos = xml.Split('\n');

                List<string> buildinglist = new List<string>();
                int i = 0;

                foreach (string building in buildinginfos)
                {
                    if (building.Length > 10)
                    {

                        string[] buildinginfo = building.Split('\t');
                        string itemName = buildinginfo[0];
                        buildinglist.Add(itemName);
                        float obj_longtitude = float.Parse(buildinginfo[1]);
                        float obj_latitude = float.Parse(buildinginfo[2]);
                        Quaternion obj_rotation = Quaternion.Euler(float.Parse(buildinginfo[3]), float.Parse(buildinginfo[4]), float.Parse(buildinginfo[5]));
                        Vector3 obj_scale = new Vector3(float.Parse(buildinginfo[6]), float.Parse(buildinginfo[7]), float.Parse(buildinginfo[8]));


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

                        var positiononMap = locationonMap + terrain.transform.position;

                        if ((positiononMap.x <= terrain.transform.position.x + terrainmap.SIZE) && (positiononMap.x >= terrain.transform.position.x) &&
                            (positiononMap.z <= terrain.transform.position.z + terrainmap.SIZE) && (positiononMap.z >= terrain.transform.position.z))
                        {
                            if (GameObject.Find(itemName) == null)
                            {
                                string assetbundleURL = "http://" + networkAddress + ":" + networkPort + "/assetbundle/" + "BasicModule" + ".assetbundle";

                                StartCoroutine(GetAssetBundle(assetbundleURL, positiononMap, obj_rotation, obj_scale, itemName));
                            }                          
                        }
                        else
                        {
                            Destroy(GameObject.Find(itemName).gameObject);
                        }

                    }
                }
                GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Rotate");
                for (var j = 0; j < gameObjects.Length; j++)
                {
                    if (!buildinglist.Contains(gameObjects[j].name))
                    {
                        Destroy(gameObjects[i]);
                    }

                }
            }
        }
    }

    /*
    private IEnumerator querySQL(string url)
    {

        //stopwatch.Start(); //  开始监视代码运行时间
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("itemName", gameObject.name));

        url += "?action=queryall";

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.error != "" && www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form queryall complete!" + www.downloadHandler.text);

            string xml = www.downloadHandler.text;
            if (xml.Length > 10) { 

                string[] buildinginfos = xml.Split('\n');

                List<string> buildinglist = new List<string>();
                int i = 0;

                foreach (string building in buildinginfos)
                {
                    if (building.Length > 10)
                    {

                        string[] buildinginfo = building.Split('\t');
                        string itemName = buildinginfo[0];
                        buildinglist.Add(itemName);
                        Vector3 obj_position = new Vector3(float.Parse(buildinginfo[1]), float.Parse(buildinginfo[2]), float.Parse(buildinginfo[3]));
                        Quaternion obj_rotation = Quaternion.Euler(float.Parse(buildinginfo[4]), float.Parse(buildinginfo[5]), float.Parse(buildinginfo[6]));
                        Vector3 obj_scale = new Vector3(float.Parse(buildinginfo[7]), float.Parse(buildinginfo[8]), float.Parse(buildinginfo[9]));
                        string locked = buildinginfo[10];

                        if (GameObject.Find(itemName) == null)
                        {
                            string assetbundleURL = "http://" + networkAddress + ":" + networkPort + "/assetbundle/" + itemName.Split('_')[0] + ".assetbundle";
                            StartCoroutine(GetAssetBundle(assetbundleURL, obj_position, obj_rotation, obj_scale, itemName));
                        }

                    }
                }
                GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Rotate");
                for (var j = 0; j < gameObjects.Length; j++)
                {
                    if (!buildinglist.Contains(gameObjects[j].name))
                    {
                        Destroy(gameObjects[i]);
                    }
                
                }
            }
        }
    }
    */
    

}
