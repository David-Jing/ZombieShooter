using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mikkeo.Extensions;
using UnityEngine.UI;
using PicaVoxel;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class DestructionManager : Singleton<DestructionManager>, INotify
{

    public GameObject explosionPrefab;

    private float impulseForceAmount = 1f;

    void Start()
    {
        ListenFor("OnBulletHit");

    }

    void OnBulletHit(Notification n)
    {
        Vector3 point = (Vector3)n.data["point"];
        GameObject go = (GameObject)n.data["object"];

        GameObject explosion = Instantiate(explosionPrefab, point, Quaternion.identity);

        switch (go.tag)
        {
            case "Wall Segment":
                Debug.Log("Wall Segment");
                SetExplosionParams(explosion, .5f, 1);
                break;

            case "Scene Object":
                Debug.Log("Scene Object");
                SetExplosionParams(explosion, .3f, 1.5f);
                break;

            default:
                SetExplosionParams(explosion, .3f, 1f);
                break;
        }

        Exploder exploder = explosion.GetComponent<Exploder>();
        exploder.Explode(new Vector3(0f, .1f, 0f));

        Destroy(explosion, .1f);

        Rigidbody rb = go.GetComponent<Rigidbody>();
        rb.AddExplosionForce(impulseForceAmount, point, .5f, 1.1f, ForceMode.Impulse);

    }

    void SetExplosionParams(GameObject go, float radius, float force, string tag = "PicaVoxelVolume")
    {
        Exploder e = go.GetComponent<Exploder>();
        e.ExplosionRadius = radius;
        e.ParticleVelocity = radius;
        e.Tag = tag;
        impulseForceAmount = force;
    }




    #region iNotify

    public void ListenFor(string messageName)
    {
        NotificationCenter.DefaultCenter.AddObserver(this, messageName);
    }

    public void StopListeningFor(string messageName)
    {
        NotificationCenter.DefaultCenter.RemoveObserver(this, messageName);
    }

    public void PostSimpleNotification(string notificationName)
    {
        NotificationCenter.DefaultCenter.PostNotification(new Notification(this, notificationName));
    }

    public void PostNotification(string notificationName, Hashtable parameters, string debugMessage = "")
    {
        if (debugMessage != "")
            Debug.Log(debugMessage);
        NotificationCenter.DefaultCenter.PostNotification(new Notification(this, notificationName, parameters));
    }

    #endregion

}