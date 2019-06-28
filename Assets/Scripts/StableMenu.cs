using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Academy.HoloToolkit.Unity;

public class StableMenu : MonoBehaviour
{
    private GameObject terrain;
    [SerializeField]
    private float DistanceFromCamera = 3f;
    private bool isfollowing;


    // Use this for initialization
    private void Start()
    {
        terrain = GameObject.Find("TerrainMap");
        isfollowing = false;
        if (terrain != null)
            this.transform.position = terrain.transform.position + new Vector3(0f, 0.5f, 0.8f);
    }

    public void OnFollowSetting(bool flag)
    {
        isfollowing = flag;
    }

    void FixedUpdate()
    {

        if (isfollowing && GazeManager.Instance.Hit && !GazeManager.Instance.HitInfo.collider.gameObject.name.Contains("Remote") 
           && !GazeManager.Instance.HitInfo.collider.gameObject.transform.parent.gameObject.name.Contains("Remote"))
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * DistanceFromCamera;

        Quaternion default_rotation = Quaternion.Euler(0, 0, 0);
        Vector3 directionToTarget = Camera.main.transform
            .position - transform.position;
        transform.rotation = Quaternion.LookRotation(-directionToTarget) * default_rotation;
    }
}
