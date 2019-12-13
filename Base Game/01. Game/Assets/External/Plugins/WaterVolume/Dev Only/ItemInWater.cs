using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WaterVolume
{

    // Used to prep and cache info about items in water
    public class ItemInWater
    {
        public int id;
        public GameObject gameObject;

        public Vector3[] bounds = new Vector3[8];
        public Vector3[] boundsInWorldSpace = new Vector3[8];
        public Vector3[] nearestSurfaceVertexPerBound = new Vector3[8];
        public float[] distancesToSurface = new float[8]; // distances for each corner of the bounding box
        public float distanceToSurface = Mathf.NegativeInfinity; // distances for the center
        public float lastDistanceToSurface = Mathf.NegativeInfinity;
        public Vector3 underWaterCenter = Vector3.zero;
        public Vector3 underWaterCenterLast = Vector3.zero;
        public Vector3 underWaterCenterTarget = Vector3.zero;

        public Rigidbody rigidbody;
        public Collider collider;
        public WaterVolume water;
        public bool valid = true;
        public bool trackOnly = false;
        public float width;
        private float underwaterPointsUpdateClock = 10;


        public Vector3 pointInFrontOfItem = Vector3.zero; // for obstruction tracking, we jump in front of the item and look back to see if anything is in the way

        public float buoyancyForceMultiplier;

        public Vector3 waterEventPoint;

        private Vector3 lastScale;

        #region Viscosity
        public Vector3 forwardVelocity;
        public Vector3 forwardVelocityNormalized;
        public Vector3 perpendicularHorizontal;
        public Vector3 perpendicularVertical;

        private float lastRaycastSpacing = 0;
        public Vector3[] viscosityRayStartPoints;
        public int viscosityRayCount = 0;

        public Vector3[] viscosityRayHitsStartPoints;
        public int viscosityRayHitCount = 0;

        private float entryDrag;
        private float entryAngularDrag;
        #endregion

        public ItemInWater(GameObject go)
        {

            Init(go);

        }

        protected void Init(GameObject go)
        {
            gameObject = go;

            position = gameObject.transform.position;


            rigidbody = go.GetComponent<Rigidbody>();
            rigidbody.angularDrag = 0.2f;
            collider = go.GetComponent<Collider>();

            entryDrag = rigidbody.drag;
            entryAngularDrag = rigidbody.angularDrag;


            CalculateBounds(go);

            for (int i = 0; i < distancesToSurface.Length; i++)
            {
                distancesToSurface[i] = Mathf.NegativeInfinity;
            }

            for (int i = 0; i < underwaterPoints.Length; i++)
            {
                underwaterPoints[i] = Vector3.zero;
            }
        }

        private void CalculateBounds(GameObject go)
        {
            Vector3 boundsCenter = Vector3.zero;
            MeshFilter meshfilter = go.GetComponent<MeshFilter>();
            lastScale = go.transform.lossyScale;

            if (go.GetComponent<BoxCollider>() != null)
            {
                BoxCollider b = go.GetComponent<BoxCollider>();

                bounds[0] = -b.size / 2f;
                bounds[4] = b.size / 2f;

                boundsCenter = b.center;

            }
            else if (meshfilter != null)
            {
                bounds[0] = meshfilter.mesh.bounds.min;
                bounds[4] = meshfilter.mesh.bounds.max;
            }
            else if (go.GetComponent<CapsuleCollider>() != null)
            {
                CapsuleCollider b = go.GetComponent<CapsuleCollider>();
                switch (b.direction)
                {
                    case 0:
                        bounds[0] = new Vector3(-b.height / 2f, -b.radius, -b.radius);
                        bounds[4] = new Vector3(b.height / 2f, b.radius, b.radius);

                        break;
                    case 1:
                        bounds[0] = new Vector3(-b.radius, -b.height / 2f, -b.radius);
                        bounds[4] = new Vector3(b.radius, b.height / 2f, b.radius);

                        break;
                    case 2:
                        bounds[0] = new Vector3(-b.radius, -b.radius, -b.height / 2f);
                        bounds[4] = new Vector3(b.radius, b.radius, b.height / 2f);

                        break;
                }

            }
            else if (go.GetComponent<SphereCollider>() != null)
            {
                SphereCollider b = go.GetComponent<SphereCollider>();
                bounds[0] = new Vector3(-b.radius, -b.radius, -b.radius);
                bounds[4] = new Vector3(b.radius, b.radius, b.radius);
            }
            else
            {

                valid = false;
            }

            if (valid)
            {


                // bounds[0] += boundsCenter;
                //bounds[4] += boundsCenter;

                bounds[1].x = -bounds[0].x;
                bounds[1].y = bounds[0].y;
                bounds[1].z = bounds[0].z;

                bounds[1] += boundsCenter;

                bounds[2].x = -bounds[0].x;
                bounds[2].y = -bounds[0].y;
                bounds[2].z = bounds[0].z;

                bounds[2] += boundsCenter;

                bounds[3].x = bounds[0].x;
                bounds[3].y = -bounds[0].y;
                bounds[3].z = bounds[0].z;

                bounds[3] += boundsCenter;


                bounds[5].x = -bounds[4].x;
                bounds[5].y = bounds[4].y;
                bounds[5].z = bounds[4].z;

                bounds[5] += boundsCenter;


                bounds[6].x = -bounds[4].x;
                bounds[6].y = -bounds[4].y;
                bounds[6].z = bounds[4].z;

                bounds[6] += boundsCenter;


                bounds[7].x = bounds[4].x;
                bounds[7].y = -bounds[4].y;
                bounds[7].z = bounds[4].z;

                bounds[7] += boundsCenter;

                bounds[0] += boundsCenter;
                bounds[4] += boundsCenter;



                // used for viscosity scanning.  is dumb.
                // Don't know what orientation its in, so scanning an area that's the square of the longest dimension
                width = Mathf.Max((bounds[4].x - bounds[0].x) * gameObject.transform.localScale.x, (bounds[4].y - bounds[0].y) * gameObject.transform.localScale.y);
                width = Mathf.Max((bounds[4].z - bounds[0].z) * gameObject.transform.localScale.z, width);
            }

        }

        public void ResetRigidBodyChanges()
        {
            rigidbody.angularDrag = entryAngularDrag;
            rigidbody.drag = entryDrag;
        }


        public void ApplyDragAtPosition(Vector3 force, Vector3 point, int viscosityRayIndex)
        {
            if (!trackOnly)
            {
                viscosityRayHitsStartPoints[viscosityRayHitCount] = viscosityRayStartPoints[viscosityRayIndex];
                viscosityRayHitCount++;

                rigidbody.AddForceAtPosition(force, point);
            }
        }


        public void ApplyBuoyancy()
        {
            if (!trackOnly && distanceToSurface > 0)
            {
                rigidbody.AddForceAtPosition(Vector3.up * buoyancyForceMultiplier, underWaterCenter);
            }
        }

        public float Depth()
        {
            return distanceToSurface;
        }

        #region Surface tracking
        private GameObject nearestSurfaceChunk;
        Vector3[] nearestSurfaceChunkVertices;
        int chunkVertexCount = 0;
        int nearestWaterMeshIndex = 0;
        private Vector3[] underwaterPoints = new Vector3[8];

        float distanceToSurfaceTemp;

        // Rough approxomation.  Is the point you're asking about beneath any of the surface points above?  
        // Won't be accurate for big waves and water edges
        public virtual bool IsInside(Vector3 position)
        {
            for (int i = 0; i < nearestSurfaceVertexPerBound.Length; i++)
            {
                if (position.y < nearestSurfaceVertexPerBound[i].y)
                {
                    return true;
                }
            }
            return false;
        }

        public Vector3 position;

        public virtual void PrepareForThreadsafeFixedUpdate()
        {
            if (gameObject == null || rigidbody == null)
            {
                valid = false;
                return;
            }



            underwaterPointsUpdateClock += Time.deltaTime;

            position = gameObject.transform.position;

            isNearObserver = water.observer == null || (water.observerPosition - position).sqrMagnitude <= water.observerDistance * water.observerDistance;

            if (!isNearObserver)
            {
                return;
            }

            if (water.useMeshesAsSurface)
            {
                nearestWaterMeshIndex = water.meshCache.GetNearestWaterObjectIndex(position);
                nearestSurfaceChunk = water.meshCache.GetWaterObject(nearestWaterMeshIndex);
                nearestSurfaceChunkVertices = water.meshCache.GetMeshVertices(nearestWaterMeshIndex);
                chunkVertexCount = nearestSurfaceChunkVertices.Length;
            }


            for (int i = 0; i < bounds.Length; i++)
            {
                boundsInWorldSpace[i] = gameObject.transform.TransformPoint(bounds[i]);
            }

            buoyancyForceMultiplier = Time.fixedDeltaTime * water.buoyancy * 100;
            // for viscocity
            forwardVelocity = rigidbody.velocity;
            viscosityRayHitCount = 0;


            if (lastRaycastSpacing != water.raycastSpacing || lastScale != gameObject.transform.lossyScale)
            {
                if (lastScale != gameObject.transform.lossyScale)
                {
                    lastScale = gameObject.transform.lossyScale;
                    CalculateBounds(gameObject);
                }


                lastRaycastSpacing = water.raycastSpacing;
                viscosityRayCount = 0;

                if (lastRaycastSpacing == 0)
                {
                    Debug.LogError("Raycast spacing cannot be 0");
                    lastRaycastSpacing = 1;
                }
             
                // Assuming a perfect fit along an axis, the ray count on one axis would be the width / ray spacing plus one
                viscosityRayCount = (int)Mathf.Pow(Mathf.FloorToInt(width / lastRaycastSpacing) + 1, 2);
                // TODO: persist this value and object for a period of time after leaving the water to prevent reallocations when bouncing in and out of the water volume
                viscosityRayStartPoints = new Vector3[viscosityRayCount];
                viscosityRayHitsStartPoints = new Vector3[viscosityRayCount];

                for (int i = 0; i < viscosityRayCount; i++)
                {
                    viscosityRayStartPoints[i] = new Vector3(0, 0, 0);
                    viscosityRayHitsStartPoints[i] = new Vector3(0, 0, 0);
                }
            }

            rigidbody.angularDrag = 0.25f * underWaterPointCount + entryAngularDrag;
            rigidbody.maxAngularVelocity = 1;

            if (!water.dontAlterDrag)
            {
                rigidbody.drag = underWaterPointCount > 0 ? 0 : entryDrag;
            }
        }

        private void ClearUnderwaterPoints()
        {
            for (int i = 0; i < underwaterPoints.Length; i++)
            {
                underwaterPoints[i].x = 0;
                underwaterPoints[i].y = Mathf.NegativeInfinity;
                underwaterPoints[i].z = 0;
            }
            underWaterPointCount = 0;
            underWaterCenter.x = underWaterCenter.y = underWaterCenter.z = 0;
            underWaterCenterTarget.x = underWaterCenterTarget.y = underWaterCenterTarget.z = 0;

        }

        public int underWaterPointCount = 0;
        private int lastUnderWaterPointCount = 0;

        public bool eventFlagFirstTouch = false;
        public bool eventFlagCenterTouch = false;
        public bool eventFlagCenterExit = false;
        public bool eventFlagFullExit = false;

        public bool isNearObserver = true;
        public void FixedUpdate_TS()
        {
            if (valid && isNearObserver)
            {
                ClearUnderwaterPoints();


                for (int i = 0; i < bounds.Length; i++)
                {
                    nearestSurfaceVertexPerBound[i] = NearestSurfaceVertex_TS(boundsInWorldSpace[i]);

                    distancesToSurface[i] = nearestSurfaceVertexPerBound[i].y - boundsInWorldSpace[i].y;

                    if (distancesToSurface[i] > 0)
                    {
                        underwaterPoints[i] = boundsInWorldSpace[i];
                        underWaterPointCount++;
                        underWaterCenterTarget += underwaterPoints[i];
                    }
                }

                distanceToSurface = NearestSurfaceVertex_TS(position).y - position.y;
               
                if (!water.ignoreDistanceFromSurface)
                {
                    buoyancyForceMultiplier *= Mathf.Max(0.3f, distanceToSurface);
                }


                buoyancyForceMultiplier = Mathf.Max(0, buoyancyForceMultiplier);

                AdjustUnderWaterCenter_TS();



                // for viscocity
                perpendicularHorizontal = Vector3.Cross(forwardVelocity, Vector3.up);
                // Velocity will often be extremely small in one direction but not quite zero
                if ((forwardVelocity.x > -0.001f || forwardVelocity.x < 0.001f) && (forwardVelocity.z > -0.001f || forwardVelocity.z < 0.001f))
                {
                    perpendicularHorizontal = Vector3.Cross(forwardVelocity, Vector3.right); // Really any vector that's not the same direction as forwardVelocity or the inverse will do in the second arg
                }
                // perpendicularHorizontal = Vector3.Cross(forwardVelocity, new Vector3(Random.Range(-100,100),Random.Range(-100,100),Random.Range(-100,100)).normalized); // Really any vector that's not the same direction as forwardVelocity or the inverse will do in the second arg

                perpendicularVertical = Vector3.Cross(forwardVelocity, perpendicularHorizontal);

                forwardVelocityNormalized = forwardVelocity.normalized;

                perpendicularHorizontal.Normalize();
                perpendicularVertical.Normalize();


                pointInFrontOfItem = position + ((forwardVelocityNormalized * 1.1f) * width / 2f);


                UpdateViscocityRays_TS();

                UpdateEventFlags_TS();

            }

        }

        private void UpdateEventFlags_TS()
        {
            eventFlagCenterExit = eventFlagCenterTouch = eventFlagFirstTouch = eventFlagFullExit = false;
            if (lastUnderWaterPointCount == 0)
            {
                if (underWaterPointCount > 0)
                {
                    eventFlagFirstTouch = true;
                    for (int i = 0; i < underwaterPoints.Length; i++)
                    {
                        if (underwaterPoints[i].y > Mathf.NegativeInfinity)
                        {
                            waterEventPoint = underwaterPoints[i];
                            break;
                        }
                    }
                }
            }
            else
            {
                if (underWaterPointCount == 0)
                {
                    eventFlagFullExit = true;
                    waterEventPoint = position;
                    waterEventPoint.y += distanceToSurface;
                }
            }

            if (lastDistanceToSurface < 0 && distanceToSurface > 0)
            {
                eventFlagCenterTouch = true;
                waterEventPoint = position;
            }

            if (lastDistanceToSurface >= 0 && distanceToSurface <= 0)
            {
                eventFlagCenterExit = true;
                waterEventPoint = position;

            }


            lastDistanceToSurface = distanceToSurface;
            lastUnderWaterPointCount = underWaterPointCount;
        }



        private void AdjustUnderWaterCenter_TS()
        {



            if (underWaterPointCount > 0)
            {
                underWaterCenterTarget = underWaterCenterTarget / underWaterPointCount;

                if (underWaterCenterLast == Vector3.zero)
                {
                    underWaterCenter = underWaterCenterTarget;

                }
                else
                {
                    underWaterCenter = underWaterCenterTarget;// Vector3.Lerp(underWaterCenterLast, underWaterCenterTarget, water.centerLerp);
                }
                underWaterCenterLast = underWaterCenter;

                if (water.useObjectCenterAsUnderWaterCenter)
                {
                    underWaterCenter = position;
                    underWaterPointCount = 8;
                }
            }



            buoyancyForceMultiplier *= (float)underWaterPointCount / 8f;


        }

        // negative means above water
        private Vector3 tmpNearestWaterPoint;
        private float tmpShortestDistance;
        Vector3 localSpacePoint;


        Vector3 waterTemp;

        protected virtual Vector3 NearestSurfaceVertex_TS(Vector3 worldSpaceVector)
        {
            if (water.useMeshesAsSurface)
            {


                tmpNearestWaterPoint = Vector3.up * Mathf.Infinity;
                tmpShortestDistance = Mathf.Infinity;
                if (nearestSurfaceChunk == null)
                {
                    return Vector3.up * Mathf.NegativeInfinity;
                }

                for (int i = 0; i < chunkVertexCount; i++)
                {

                    if ((nearestSurfaceChunkVertices[i].x - worldSpaceVector.x) * (nearestSurfaceChunkVertices[i].x - worldSpaceVector.x) +
                        (nearestSurfaceChunkVertices[i].z - worldSpaceVector.z) * (nearestSurfaceChunkVertices[i].z - worldSpaceVector.z) < tmpShortestDistance)
                    {

                        tmpShortestDistance = (nearestSurfaceChunkVertices[i].x - worldSpaceVector.x) * (nearestSurfaceChunkVertices[i].x - worldSpaceVector.x) + (nearestSurfaceChunkVertices[i].z - worldSpaceVector.z) * (nearestSurfaceChunkVertices[i].z - worldSpaceVector.z);
                        tmpNearestWaterPoint = nearestSurfaceChunkVertices[i];
                        tmpNearestWaterPoint.y += water.waterLevelOffset;
                        tmpNearestWaterPoint.y = Mathf.Clamp(tmpNearestWaterPoint.y, tmpNearestWaterPoint.y, water.waterSurfaceHeight);
                    }

                    water.statistics.lastPointComparisonCount++;
                }
                if (tmpShortestDistance != Mathf.Infinity)
                {
                    return tmpNearestWaterPoint;
                }

                return Vector3.up * Mathf.NegativeInfinity;
            }
            else
            {
                return new Vector3(worldSpaceVector.x, water.waterSurfaceHeight + Mathf.Clamp(water.waterLevelOffset, Mathf.NegativeInfinity, 0), worldSpaceVector.z);

            }

        }

        private void UpdateViscocityRays_TS()
        {


            if (lastRaycastSpacing == 0)
            {
                Debug.LogError("Raycast spacing cannot be zero");
                return;
            }
            int i = 0;

           
            // Scan from center out along 4 directions
            for (float x = 0; x <= width / 2f; x += lastRaycastSpacing)
            {

                for (float y = 0; y <= width / 2f; y += lastRaycastSpacing)
                {
                  
                        viscosityRayStartPoints[i] = pointInFrontOfItem + (perpendicularHorizontal * x) + (perpendicularVertical * y);
                        if (x > 0)
                        {
                            i++;
                            viscosityRayStartPoints[i] = pointInFrontOfItem + (perpendicularHorizontal * (-x)) + (perpendicularVertical * y);

                            if (y > 0)
                            {
                                i++;
                                viscosityRayStartPoints[i] = pointInFrontOfItem + (perpendicularHorizontal * (-x)) + (perpendicularVertical * -y);

                            }
                        }
                        if (y > 0)
                        {
                            i++;
                            viscosityRayStartPoints[i] = pointInFrontOfItem + (perpendicularHorizontal * (x)) + (perpendicularVertical * -y);

                        }
                    i++;
                }
            }
        
        }

        #endregion

        public void OnDrawGizmos()
        {
            if (!valid || !water.drawGizmos)
            {
                return;
            }

            if (nearestSurfaceChunkVertices != null)
            {

                for (int i = 0; i < nearestSurfaceChunkVertices.Length; i++)
                {
                    Gizmos.color = Color.white;
                    // Gizmos.DrawWireSphere( waterMeshLocalToWorldMatrix.MultiplyPoint(nearestSurfaceChunkVertices[i]), 0.1f);
                    Gizmos.DrawWireSphere(nearestSurfaceChunkVertices[i], 0.1f);
                }

            }
            for (int i = 0; i < underwaterPoints.Length; i++)
            {

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(underwaterPoints[i], 0.2f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(underwaterPoints[i], underwaterPoints[i] + Vector3.up * distancesToSurface[i]);
                Gizmos.DrawWireSphere(nearestSurfaceVertexPerBound[i], 0.2f);
                Gizmos.color = Color.blue;

                Gizmos.DrawLine(underWaterCenter, underWaterCenter + Vector3.up * buoyancyForceMultiplier);


            }


            Gizmos.color = Color.red;

            //Gizmos.DrawSphere(pointInFrontOfItem, 0.1f);

            Gizmos.DrawLine(pointInFrontOfItem + (-width / 2f * perpendicularHorizontal), pointInFrontOfItem + (width / 2f * perpendicularHorizontal));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pointInFrontOfItem + (-width / 2f * perpendicularVertical), pointInFrontOfItem + (width / 2f * perpendicularVertical));

            if (distanceToSurface > 0)
            {

                foreach (Vector3 start in viscosityRayStartPoints)
                {
                    Gizmos.color = Color.cyan;

                    Gizmos.DrawLine(start, start + (-forwardVelocityNormalized * width));
                }

            }

            for (int j = 0; j < viscosityRayHitCount; j++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(viscosityRayHitsStartPoints[j], viscosityRayHitsStartPoints[j] + (-forwardVelocityNormalized * width));
            }




        }
    }

}
