using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct TheZombie {
    public int HP;
    public GameObject zombie;
}

public class ZombieManager : MonoBehaviour, INotify {
    public Transform spawnCentre;
    public Transform zombieContainer;
    public GameObject zombiePrefab;
    public int maxZombieHP;
    public float zombieMaxSpawnRange;
    public float zombieMinSpawnRange;
    public float zombieSpawnRate;
    public float zombieFirstSpawnTime;
    int zombieLabel;
    Dictionary<string, TheZombie> ZombieDict = new Dictionary<string, TheZombie> ();

    void Start () {
        ListenFor ("OnRandomZombieSpawn");
        ListenFor ("OnBulletHitZombie");
        zombiePrefab.SetActive (false);

        InvokeRepeating ("SpawnZombie", zombieFirstSpawnTime, zombieSpawnRate);
    }
    void OnSpawnZombie (Notification n) {
        InvokeRepeating ("SpawnZombie", zombieFirstSpawnTime, zombieSpawnRate);
    }

    void onStopSpawnZombie (Notification n) {
        CancelInvoke ("SpawnZombie");
    }

    void SpawnZombie () {
        PostSimpleNotification ("OnRandomZombieSpawn");
    }

    void OnRandomZombieSpawn (Notification n) {
        TheZombie newZombie = new TheZombie ();
        newZombie.HP = maxZombieHP;
        newZombie.zombie = Instantiate (zombiePrefab, spawnCentre);
        newZombie.zombie.gameObject.SetActive (true);

        float SpawnRange = Random.Range (zombieMinSpawnRange, zombieMaxSpawnRange);
        float SpawnAngle = Random.Range (0, 360f);

        // --------------Radius--------------
        newZombie.zombie.transform.localPosition = new Vector3 (SpawnRange, 0, 0);

        // --------------Angle--------------
        spawnCentre.transform.Rotate (new Vector3 (0, SpawnAngle, 0));
        // ---------------------------------

        newZombie.zombie.transform.SetParent (zombieContainer);
        newZombie.zombie.transform.name = "Zombie " + zombieLabel;

        ZombieDict.Add ("Zombie " + zombieLabel++, newZombie);

        // ---------Zombie Spawn Stuff------
        //newZombie.zombie.GetComponent<ZombiePrefab> ().Spawn ();
    }
    void OnBulletHitZombie (Notification n) {
        GameObject zombie = (GameObject) n.data["Zombie"];
        TheZombie selectZombie;

        if (ZombieDict.TryGetValue (zombie.name, out selectZombie)) {
            if (selectZombie.HP < 1) {
                ZombieDict.Remove (zombie.name);
                Destroy (zombie);
                // ---------Zombie Death Stuff------
                //selectZombie.zombie.GetComponent<ZombiePrefab> ().Die ();
                // ---------------------------------
            } else {
                HPdecrease (selectZombie);
                // ---------Zombie Hit Stuff--------
                //selectZombie.zombie.GetComponent<ZombiePrefab> ().Hit ();
                // ---------------------------------
            }
        } else {
            Debug.Log ("Not Found");
        };
    }

    void HPdecrease (TheZombie selectZombie) {
        int hp = --selectZombie.HP;
        GameObject zom = selectZombie.zombie;
        zom.name = selectZombie.zombie.name;

        ZombieDict.Remove (selectZombie.zombie.name);

        TheZombie newZombie = new TheZombie ();
        newZombie.zombie = zom;
        newZombie.HP = hp;

        ZombieDict.Add (newZombie.zombie.name, newZombie);
    }

    #region iNotify

    public void ListenFor (string messageName) {
        NotificationCenter.DefaultCenter.AddObserver (this, messageName);
    }

    public void StopListeningFor (string messageName) {
        NotificationCenter.DefaultCenter.RemoveObserver (this, messageName);
    }

    public void PostSimpleNotification (string notificationName) {
        NotificationCenter.DefaultCenter.PostNotification (new Notification (this, notificationName));
    }

    public void PostNotification (string notificationName, Hashtable parameters, string debugMessage = "") {
        if (debugMessage != "")
            Debug.Log (debugMessage);
        NotificationCenter.DefaultCenter.PostNotification (new Notification (this, notificationName, parameters));
    }

    #endregion
}