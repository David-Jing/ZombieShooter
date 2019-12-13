using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombiePrefab : MonoBehaviour, INotify {
    // Start is called before the first frame update
    public float radius = 1500.0f;
    private NavMeshAgent zombMov;
    private Vector3 destination;
    private GameObject target;
    void Start () {
        zombMov = GetComponent<NavMeshAgent> ();
        destination = zombMov.destination;
        InvokeRepeating ("Pathing", 0.5f, 1f);
    }

    void Pathing () {
        Vector3 currentZombPos = this.transform.position;
        Collider[] targets = Physics.OverlapSphere (currentZombPos, radius, 1 << LayerMask.NameToLayer ("Player"));
        int i = 0;
        float closestDistance = radius;
        if (targets.Length == 0)
            return;

        while (i < targets.Length) {
            Vector3 playerPosition = targets[i].transform.position;
            float dist = Vector3.Distance (currentZombPos, playerPosition);
            if (dist < closestDistance) {
                target = targets[i].gameObject;
                closestDistance = dist;
            }
            i++;
        }

        destination = target.transform.position;
        zombMov.destination = destination;
    }

    void OnCollisionEnter (Collision col) {
        if (col.gameObject.tag == "Char") {
            //PostNotification ("onZombieHitPlayer", new Hashtable { { "Char", col.gameObject } });
        } else if (col.gameObject.tag == "Wall") {
            PostNotification ("onZombieHitWall", new Hashtable { { "Wall", col.gameObject } });
        }
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