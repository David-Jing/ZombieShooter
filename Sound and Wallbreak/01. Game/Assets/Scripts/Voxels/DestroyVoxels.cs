using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PicaVoxel;
using System.Linq;
using System.Linq.Expressions;

public class DestroyVoxels : MonoBehaviour
{
    /*  To do that we loop through the array, 
    y then x then z ( So from the bottom left corner of that image, 
    the grid location closest to us ) looking for active voxels. 
    Once we find one we fire off a particle from that position and colour, 
    and repeat until every voxel has a particle counter part and then we can kill the actual zombie mesh.
    
    To optimise things and stop the performance suffering ( It's 60fps or go home ) 
    we skip some voxels in our array, not only does it help performance wise 
    but it actually looks better, originally we had too many particles 
    ( Which is something I thought I've never write ).
*/
    public Volume voxelVolume;
    public float percentageOfCubesToWipe = .5f;
    private BoxCollider wallCollider;
    public float wallHealth = 100f;
    public int wallHitAmount = 10;

    private Frame voxelFrame;

    void Start()
    {
        wallCollider = gameObject.GetComponent<BoxCollider>();
    }
    public void TakeAHit()
    {
        voxelFrame = voxelVolume.GetCurrentFrame();

        // With LINQ
        Debug.Log(voxelFrame.Voxels.Count(v => v.State == VoxelState.Active));

        // Simple count
        var active = 0;
        int counter = 0;

        for (var i = 0; i < voxelFrame.Voxels.Length; i++)
        {
            if (voxelFrame.Voxels[i].State == VoxelState.Active)
            {
                active++;


                if (Random.Range(0f, 1f) < percentageOfCubesToWipe)
                {
                    if (voxelFrame.Voxels[i].State == VoxelState.Active)
                    {
                        voxelFrame.Voxels[i].State = VoxelState.Inactive;
                        active--;
                    }
                }
            }
        }
        voxelFrame.UpdateAllChunks();
        Debug.Log(active);


        wallHealth -= wallHitAmount;

        Debug.Log("Wall health: " + wallHealth);

        if (wallHealth < 15)
        {
            Debug.Log("Wall breached");
            wallCollider.enabled = false;
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        voxelFrame = voxelVolume.GetCurrentFrame();

        Vector3 distFromColliderToMyOrigin = collision.transform.position - transform.position;

        foreach (ContactPoint contact in collision.contacts)
        {
            //Vector3 voxelWorldCoord = contact.point + (distFromColliderToMyOrigin * .5f);
            Vector3 voxelWorldCoord = contact.point;
            Debug.Log("Boom: " + contact.point.ToString());
            //Debug.DrawRay(contact.point, contact.normal, Color.white);
            Voxel v = (Voxel)voxelFrame.GetVoxelAtWorldPosition(voxelWorldCoord);
            v.State = VoxelState.Inactive;
        }
        voxelFrame.UpdateAllChunks();


    }


    public Vector3 GetVoxelWorldPosition(Volume v, int x, int y, int z)
    {
        Vector3 localPos = (new Vector3(x * v.VoxelSize, y * v.VoxelSize, z * v.VoxelSize)
        + (Vector3.one * (v.VoxelSize / 2f)));

        return transform.TransformPoint(localPos);
    }



}
