using System;
using System.Collections;
using UnityEngine;
using WaterVolume;

/*

A simple example of how subscribe to water events to create a splash effect.
All available events are shown here though only one is used.

*/

public class SplashExample : MonoBehaviour {

    public WaterVolume.WaterVolume water;
    public GameObject splashPrefab;
    public float splashSize = 50;
    public void Start()
    {
        water.OnItemEnteredWater += OnItemEnteredWater;
        water.OnItemExitedWater += OnItemExitedWater;
        water.OnItemCenterEnteredWater += OnItemCenterEnteredWater;
        water.OnItemCenterExitedWater += OnItemCenterExitedWater;        
    }

    public void OnItemCenterEnteredWater(ItemInWater item)
    {
       
    }

    public void OnItemCenterExitedWater(ItemInWater item)
    {
    }

    public void OnItemEnteredWater(ItemInWater item)
    {
        //GameObject s = (GameObject)GameObject.Instantiate(splashPrefab);//, item.waterEventPoint + Vector3.right * 1000, Quaternion.identity);
        GameObject s = (GameObject)GameObject.Instantiate(splashPrefab, item.waterEventPoint, splashPrefab.transform.rotation);
        ParticleSystem p = s.GetComponent<ParticleSystem>();
        s.transform.position = item.waterEventPoint;
        p.maxParticles = (int)Mathf.Clamp(splashSize * item.rigidbody.velocity.magnitude, 0, 500);
        p.startSpeed = (int)Mathf.Clamp(item.rigidbody.velocity.magnitude, 0, 5);
        p.Play();
        Destroy(s, 3);
      
    }


    public void OnItemExitedWater(ItemInWater item)
    {
      
    }

    void OnDestroy()
    {
        water.OnItemEnteredWater -= OnItemEnteredWater;
        water.OnItemExitedWater -= OnItemExitedWater;
        water.OnItemCenterEnteredWater -= OnItemCenterEnteredWater;
        water.OnItemCenterExitedWater -= OnItemCenterExitedWater;
    }
}
