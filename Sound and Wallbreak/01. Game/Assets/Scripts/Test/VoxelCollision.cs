using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PicaVoxel;
using Mikkeo.Extensions;

public class VoxelCollision : MonoBehaviour
{


    public Volume voxelVolume;

    Rigidbody rb;
    Bounds boundingBox;
    Frame voxelFrame;
    public GameObject exploderPrefab;

    void Start()
    {
        if (!voxelVolume)
        {
            voxelVolume = GetComponent<Volume>();
        }
        rb = gameObject.GetComponent<Rigidbody>();
        boundingBox = new Bounds(transform.position, Vector3.one * 1f);

    }

    void OnCollisionEnterNotYet(Collision collision)
    {
        string s = "";

        if (collision.gameObject.tag == ("PicaVoxelVolume"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                boundingBox.center = contact.point;
                boundingBox.size = Vector3.one;

                voxelFrame = voxelVolume.GetCurrentFrame();

                int counter = 0;
                s = "";

                for (var i = 0; i < voxelVolume.XSize; i++)
                {
                    for (var j = 0; j < voxelVolume.YSize; j++)
                    {
                        for (var k = 0; k < voxelVolume.ZSize; k++)
                        {

                            PicaVoxelPoint pvp = new PicaVoxelPoint(i, j, k);

                            Vector3 voxelPosition = voxelVolume.GetVoxelWorldPos(pvp);

                            if (boundingBox.Contains(voxelPosition))
                            {
                                Voxel v = (Voxel)voxelVolume.GetVoxel(pvp);

                                if (v.Active)
                                {
                                    s += ("Setting inactive cell:" + i + "|" + j + "|" + k + "\n");
                                    v.State = VoxelState.Inactive;
                                    //v.Color = Color.blue;
                                }
                            }

                        }
                    }
                }

            }

            Debug.Log(s);

            //Debug.Log("Collision: ");
            voxelFrame.UpdateAllChunks();

        }



    }

    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(boundingBox.center, boundingBox.extents);
    }

    void OnCollisionEnterYupItDidWork(Collision collision)
    {
        if (collision.gameObject.tag == ("PicaVoxelVolume"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                boundingBox.center = contact.point;
                boundingBox.size = .1f * Vector3.one;

                voxelFrame = voxelVolume.GetCurrentFrame();

                int counter = 0;
                string s = "";

                for (var i = 0; i < voxelFrame.Voxels.Length; i++)
                {
                    Voxel v = voxelFrame.Voxels[i];

                    if (v.State == VoxelState.Active)
                    {
                        counter++;
                        if (counter < 50)
                        {
                            //Vector3 voxelPosn = PicaVoxel.Get  GetVoxelAtArrayPosition(vx, vy, vz);
                            //                         if (boundingBox.Contains(voxelPosn))
                            //Vector3 posn = voxelVolume.GetVoxelWorldPos(name PicaVoxelPoint()
                            v.State = VoxelState.Hidden;

                        }
                    }
                }
            }
            voxelVolume.GetCurrentFrame().UpdateAllChunks();
        }


    }


    void UpdateMe()
    {
        // Left click
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the camera position outward

            Ray r = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            Debug.DrawRay(r.origin, r.direction * 100f, Color.red, 10f);

            RaycastHit hitInfo;
            if (Physics.Raycast(r, out hitInfo)) // first, cast the ray using normal Unity physics. Don't forget to include a layer mask if needed
            {
                Volume pvo = hitInfo.collider.GetComponentInParent<Volume>();
                if (pvo != null) // check to see if we have hit a PicaVoxel Volume. because the Hitbox is a child object on the Volume, we use GetComponentInParent
                {
                    r = new Ray(hitInfo.point, r.direction); // now create a new ray starting at the hit position of the old ray
                    for (float d = 0; d < 50f; d += pvo.VoxelSize * 0.5f) // iterate along the ray. we're using a maximum distance of 50 units here, you should adjust this to a sensible value for your scene
                    {
                        Voxel? v = pvo.GetVoxelAtWorldPosition(r.GetPoint(d)); // see if there's a voxel at the ray position
                        if (v.HasValue && v.Value.Active)
                        {
                            // We have a voxel, and it's active so cause an explosion at this location
                            Batch b = pvo.Explode(r.GetPoint(d), .2f, 0, Exploder.ExplodeValueFilterOperation.GreaterThanOrEqualTo);

                            // The delegate function here calculates a random particle velocity based on the position of the explosion
                            VoxelParticleSystem.Instance.SpawnBatch(b, pos =>
                                 (pos - r.GetPoint(d)) * Random.Range(0f, .3f));

                            break;
                        }
                    }
                }
            }

        }

    }



    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == ("PicaVoxelVolume"))
        {

            Debug.Log("Colliding with: " + col.gameObject.name);
            //return;
            // Something has collided with the car, so we'll find out where in the scene the collision occured
            // We'll average out the contact points to give a median position
            Vector3 avg = Vector3.zero;
            foreach (ContactPoint cp in col.contacts) avg += cp.point;
            if (col.contacts.Length > 1)
                avg /= (float)col.contacts.Length;

            // Set the Exploder's position to the average collision position
            GameObject go = Instantiate(exploderPrefab);
            Exploder exploder = go.GetComponent<Exploder>();
            exploder.transform.position = avg;
            exploder.ExplosionRadius = .3f;
            exploder.ParticleVelocity = .1f;
            exploder.Tag = "PicaVoxelVolume";

            // We'll give our explosion particles some upward velocity - also for effect
            exploder.Explode(new Vector3(0f, .1f, 0f));
            Destroy(go, .1f);
        }

    }




}