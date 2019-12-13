using UnityEngine;
using System.Collections;

namespace WaterVolume
{
    public class MeshCache
    {
        private float[] waterMeshVertexTimestamps;
        public WaterVolume waterVolume;
        private Vector3[][] waterMeshVertexCache;
        private Mesh[] waterMeshes;
        private GameObject[] waterObjects;
        private Vector3[] waterObjectsPositionCache;

        GameObject nearestWaterObject;
        int nearestMeshIndex;

        bool skinnedMode = false;

        private Mesh bakedMesh;

        public void SetMeshes(GameObject[] waterObjects)
        {


            this.waterObjects = waterObjects;
            waterMeshes = new Mesh[waterObjects.Length];


            if (waterObjects.Length == 0)
            {
                waterVolume.useMeshesAsSurface = false;
                return;
            }
            else
            {
                waterVolume.useMeshesAsSurface = true;

            }


            for (int i = 0; i < waterObjects.Length; i++)
            {
                if (waterObjects[i].GetComponent<MeshFilter>() != null)
                {
                    if (skinnedMode)
                    {
                        Debug.LogError("Water meshes must be either all MeshFilters or SkinnedMeshRenderers, not a mix.");
                    }
                    else
                    {
                        waterMeshes[i] = waterObjects[i].GetComponent<MeshFilter>().mesh;
                    }
                }
                else
                    if (waterObjects[i].GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        skinnedMode = true;
                        bakedMesh = new Mesh();
                        waterMeshes[i] = waterObjects[i].GetComponent<SkinnedMeshRenderer>().sharedMesh;
                    }
            }
            FillWaterObjectsPositionsCache();
        }

        private void FillWaterObjectsPositionsCache()
        {
            if (waterObjectsPositionCache == null || waterObjectsPositionCache.Length != waterObjects.Length)
            {
                waterObjectsPositionCache = new Vector3[waterObjects.Length];
            }
            for (int i = 0; i < waterObjects.Length; i++)
            {
                waterObjectsPositionCache[i] = waterObjects[i].transform.position;
            }
        }

        public void AddMesh(GameObject waterObject)
        {
            if (waterObject == null)
            {
                Debug.LogError("Tried to add null mesh to WaterVolume");
                return;
            }
            ClearNullMeshes();
            GameObject[] newWaterObjects = new GameObject[waterObjects.Length + 1];
            int i = 0;
            for (i = 0; i < waterObjects.Length; i++)
            {
                newWaterObjects[i] = waterObjects[i];
            }
            newWaterObjects[i] = waterObject;

            SetMeshes(newWaterObjects);
        }

        // Returns true if null meshes had to be cleared out
        public bool ClearNullMeshes()
        {
            int count = 0;
            for (int i = 0; i < waterObjects.Length; i++)
            {
                if (waterObjects[i] != null)
                {
                    count++;
                }
            }


            if (count != waterObjects.Length)
            {
                //Debug.Log("Clearing: " + (waterObjects.Length - count));

                GameObject[] newWaterObjects = new GameObject[count];
                int x = 0;
                for (int i = 0; i < waterObjects.Length; i++)
                {
                    if (waterObjects[i] != null)
                    {
                        newWaterObjects[x] = waterObjects[i];
                        x++;
                    }
                }
                waterObjects = newWaterObjects;
                SetMeshes(newWaterObjects);
                return true;
            }

            return false;

        }

        public void FixedUpdate()
        {
            ClearNullMeshes();

            UpdateWaterMeshVertexCacheTimestamps();
        }

        public int GetNearestWaterObjectIndex(Vector3 position)
        {
            float nearestSqrMagnitude = (waterObjects[0].transform.position - position).sqrMagnitude;

            nearestWaterObject = waterObjects[0];
            nearestMeshIndex = 0;
            for (int i = 0; i < waterMeshes.Length; i++)
            {
                //  if ((waterObjects[i].transform.position - position).sqrMagnitude < nearestSqrMagnitude)
                if ((waterObjectsPositionCache[i] - position).sqrMagnitude < nearestSqrMagnitude)
                {
                    nearestWaterObject = waterObjects[i];
                    nearestMeshIndex = i;
                    nearestSqrMagnitude = (nearestWaterObject.transform.position - position).sqrMagnitude;

                }
            }

            return nearestMeshIndex;
        }

        public GameObject GetWaterObject(int i)
        {
            return waterObjects[i];
        }


        public Vector3[] GetMeshVertices(int chunk)
        {
            if (waterMeshVertexCache == null || waterMeshVertexCache.Length != waterMeshes.Length)
            {
                waterMeshVertexCache = new Vector3[waterMeshes.Length][];
                waterMeshVertexTimestamps = new float[waterMeshes.Length];
            }
            if (waterMeshVertexTimestamps[chunk] > waterVolume.meshCacheUpdatePeriod || waterMeshVertexCache[chunk] == null)
            {
                if (skinnedMode)
                {
                    waterObjects[chunk].GetComponent<SkinnedMeshRenderer>().BakeMesh(bakedMesh);
                    waterMeshVertexCache[chunk] = bakedMesh.vertices;


                }
                else
                {
                    waterMeshVertexCache[chunk] = waterMeshes[chunk].vertices; // This is a big memory allocation. mesh.vertices copies when referenced!

                }

                for (int j = 0; j < waterMeshVertexCache[chunk].Length; j++)
                {
                    waterMeshVertexCache[chunk][j] = waterObjects[chunk].transform.TransformPoint(waterMeshVertexCache[chunk][j]);
                }

                waterMeshVertexTimestamps[chunk] = 0;
                FillWaterObjectsPositionsCache();
            }
            return waterMeshVertexCache[chunk];
        }

        private void UpdateWaterMeshVertexCacheTimestamps()
        {
            if (waterMeshVertexTimestamps == null || waterMeshVertexCache == null || waterMeshVertexCache.Length != waterMeshes.Length || waterMeshVertexTimestamps.Length != waterMeshes.Length)
            {
                waterMeshVertexTimestamps = new float[waterMeshes.Length];
            }
            for (int i = 0; i < waterMeshes.Length; i++)
            {
                waterMeshVertexTimestamps[i] += Time.deltaTime;
            }
        }

        public GameObject[] GetWaterObjects()
        {
            return waterObjects;
        }
    }
}
