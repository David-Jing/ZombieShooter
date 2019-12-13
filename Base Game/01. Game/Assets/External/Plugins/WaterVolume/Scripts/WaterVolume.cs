using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading;


namespace WaterVolume
{
    public class WaterVolume : MonoBehaviour
    {
        private WaterThreads waterThreads;

        public Dictionary<int, ItemInWater> itemsInVolume = new Dictionary<int, ItemInWater>();

        // Physical properties
        [Header("Physicality")]
        public float buoyancy = 10;
        public float viscosity = 30;
        [Tooltip("A force vector that will be constantly applied")]
        public Vector3 flow = Vector3.zero;


        [Header("Water Collider")]
        [Tooltip("Specify a BoxCollider set as a trigger to define the shape of the water.")]
        public BoxCollider waterBox;
        [Tooltip("Raise and lower the effective surface of the water.  Has no effect if higher than the top of the Water Box collider.")]
        public float waterLevelOffset = 0;
        [HideInInspector]
        public float waterSurfaceHeight = Mathf.NegativeInfinity;

        [Header("Water Meshes (Optional)")]
        [Tooltip("Enable tracking animated meshes to use as a water surface. The meshes must be inside the \"Water Collider\".")]
        public bool useMeshesAsSurface = false;
        [Tooltip("Breaking a large surface into smaller meshes lets us track them more efficiently.")]
        public GameObject[] waterMeshes;
        [Tooltip("Reading the vertex positions is costly.  Less frequent reads can improve performance with minimal quality degradation. If you have high buoyancy and a quickly animated mesh, you may need a low value to prevent sudden upward movements.")]
        public float meshCacheUpdatePeriod = 0.2f;


        [Header("Observer (Optional)")]
        [Tooltip("A GameObject, likely a player or camera, that the water should always behave near.  This lets us avoid making calculations that won't be seen by the observer.")]
        public GameObject observer;
        [HideInInspector]
        public Vector3 observerPosition; // for threaded access
        [Tooltip("Items in water this far from the observer will be made inactive")]
        public float observerDistance = 10;


        [Serializable]
        public class Statistics
        {
            public int lastRayCount = 0;
            public int raysPerSecond = 0;
            [HideInInspector]
            public int raysPerSecondCounter = 0;
            [HideInInspector]
            public float raysPerSecondClock = 0;
            public int lastPointComparisonCount = 0;
            public int pointComparisonsPerSecond = 0;
            [HideInInspector]
            public int pointComparisonsPerSecondCounter = 0;
            public int itemCount = 0;
        }

        [Header("Threading")]
        [Tooltip("If true, will decide on Start if threads should be used and how many.")]
        public bool automaticThreads = true;
        [Tooltip("Can be toggled at runtime")]
        public bool useThreads = false;
        [Tooltip("Can be changed at runtime")]
        public int threadCount = 2;

        [Header("Misc (Mouseover for details)")]
        [Tooltip("Gizmos can show center of mass, buoyancy forces, underwater corners and more")]
        public bool drawGizmos = false;
        private bool notifyEnteringItems = true;
        // Accuracy / efficiency

        [Tooltip("Raycasts greatly affect performance.  Lower spacing means more raycasts. Set spacing to be just smaller than the smallest objects you wish to track")]
        public float raycastSpacing = 0.75f;
        [HideInInspector]
        public float minimumRaycastSpacing = 0.1f;

        [Tooltip("GameObjects that should be tracked but not affected by water. Items can be added here before runtime or during runtime via the OnlyTrack(GameObject) method.")]
        public GameObject[] onlyTrackList;
        private List<int> onlyTrackIDs = new List<int>(); // processed from the ignorelist for efficiency
        private LayerMask layerMask;

        [Tooltip("The Water Box Collider must be on a different layer than the items it is tracking for object occlusion to work properly near the surface.  By default it will be forced to the IgnoreRaycast layer, but you can change that here.")]
        public bool dontChangeLayer = false;

        [Tooltip("By default rigidbody drag is set to zero on entry and restored on exit.")]
        public bool dontAlterDrag = false;



        [SerializeField]
        public Statistics statistics = new Statistics();

        // Events
        public delegate void ItemDelegate(ItemInWater item);
        public event ItemDelegate OnItemEnteredWater;
        public event ItemDelegate OnItemExitedWater;
        public event ItemDelegate OnItemCenterEnteredWater;
        public event ItemDelegate OnItemCenterExitedWater;


        #region threading
        int bucketSize;
        int itemCountForThread;
        ManualResetEvent[] threadCompleteEvents;
        #endregion

        [HideInInspector]
        public MeshCache meshCache;

        //Degradations for teaching/debugging purposes
        [HideInInspector]
        public bool useObjectCenterAsUnderWaterCenter = false;
        [HideInInspector]
        public bool ignoreDistanceFromSurface = false;
        [HideInInspector]
        public bool applyViscosityOnlyAtCenter = false;


        void Awake()
        {
            CheckInitialConditions();
        }

        private void CheckInitialConditions()
        {

            if (waterBox == null)
            {
                if (gameObject.GetComponent<BoxCollider>() != null)
                {
                    waterBox = gameObject.GetComponent<BoxCollider>();
                }
                else
                {
                    Debug.LogError("You must specify a BoxCollider for the waterBox");
                }
            }

            if (!waterBox.isTrigger)
            {
                Debug.LogWarning("Setting waterBox to be a trigger. Set \"" + waterBox.name + "\"'s collider as trigger manually to avoid this warning.");
                waterBox.isTrigger = true;
            }

            if (!dontChangeLayer)
            {
                waterBox.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
            if (useMeshesAsSurface && waterMeshes.Length == 0)
            {
                Debug.LogError("Cannot track water mesh surfaces: No water meshes specified");
                if (waterBox == null)
                {
                    Debug.LogError("Cannot fall back to track water box collider: None specified");
                }
                else
                {
                    Debug.LogWarning("Falling back on tracking water box collider: " + waterBox.name);
                }
            }

            meshCache = new MeshCache();
            meshCache.waterVolume = this;
            meshCache.SetMeshes(waterMeshes);

            if (onlyTrackList.Length > 0)
            {
                onlyTrackIDs = new List<int>();
                foreach (GameObject go in onlyTrackList)
                {
                    onlyTrackIDs.Add(go.GetInstanceID());
                }
            }

            layerMask = ~LayerMask.GetMask(LayerMask.LayerToName(waterBox.gameObject.layer));

            itemsEnumerator = itemsInVolume.GetEnumerator();
            if (automaticThreads)
            {
                threadCount = SystemInfo.processorCount;
                if (threadCount > 1)
                {
                    useThreads = true;
                }
                else
                {
                    useThreads = false;
                }
            }
            waterThreads = new WaterThreads();
            waterThreads.water = this;
        }

        public void AddMesh(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("Tried to add null mesh to WaterVolume");
            }
            else
            {
                meshCache.AddMesh(obj);

            }
        }

        public ItemInWater GetItem(int id)
        {
            if (itemsInVolume.ContainsKey(id))
            {
                return itemsInVolume[id];
            }
            else
            {
                //Debug.LogWarning("Item not in volume: " + id);
            }
            return null;

        }

        void OnTriggerEnter(Collider other)
        {
            bool trackOnly = onlyTrackIDs.Contains(other.gameObject.GetInstanceID());
            Rigidbody r = other.GetComponent<Rigidbody>();
            if ((r != null && !r.isKinematic) || trackOnly)
            {
                if (!r.gameObject.isStatic && !itemsInVolume.ContainsKey(r.gameObject.GetInstanceID()))
                {
                    ItemInWater item = new ItemInWater(other.gameObject);
                    item.water = this;
                    item.id = other.gameObject.GetInstanceID();
                    item.trackOnly = trackOnly;
                    itemsInVolume.Add(item.id, item);
                    itemsEnumerator = itemsInVolume.GetEnumerator();
                }
            }

        }

        void OnTriggerExit(Collider other)
        {
            Rigidbody r = other.GetComponent<Rigidbody>();
            if (r != null)
            {

                item = GetItem(r.gameObject.GetInstanceID());
                if (item != null)
                {
                    item.ResetRigidBodyChanges();
                    itemsInVolume.Remove(r.gameObject.GetInstanceID());
                    itemsEnumerator = itemsInVolume.GetEnumerator();

                    if (!useMeshesAsSurface)
                    {
                        if (OnItemExitedWater != null)
                        {
                            OnItemExitedWater(item);
                        }


                    }
                    NotifyOfExit(item.gameObject);
                }
            }
        }

        private void NotifyOfEntry(GameObject go)
        {
            if (notifyEnteringItems)
            {
                go.BroadcastMessage("OnEnterWater", go.GetInstanceID(), SendMessageOptions.DontRequireReceiver);
            }
        }

        private void NotifyOfExit(GameObject go)
        {
            if (notifyEnteringItems)
            {
                go.BroadcastMessage("OnExitWater", SendMessageOptions.DontRequireReceiver);
            }
        }


        public void OnlyTrack(GameObject go)
        {
            onlyTrackIDs.Add(go.GetInstanceID());
        }

        float distanceToSurfaceTemp;

        void FixedUpdate()
        {

            statistics.lastPointComparisonCount = 0;

            if (waterBox != null)
            {
                waterSurfaceHeight = waterBox.bounds.max.y;
            }

            if (!useMeshesAsSurface)
            {
                if (waterBox == null)
                {
                    Debug.Log("Not tracking water mesh surfaces and no waterbox available");
                }
            }
            else
            {
                if (itemsInVolume.Count > 0)
                {
                    meshCache.FixedUpdate();
                }
            }

            if (observer != null)
            {
                observerPosition = observer.transform.position;
            }

            if (buoyancy != 0 || viscosity != 0)
            {

                if (itemsInVolume.Count > 0)
                {
                    itemsEnumerator = itemsInVolume.GetEnumerator();

                    if (useThreads && threadCount >= 1)
                    {
                        waterThreads.DoFixedUpdate();
                    }
                    else
                    {
                        while (itemsEnumerator.MoveNext())
                        {
                            itemsEnumerator.Current.Value.PrepareForThreadsafeFixedUpdate();
                            itemsEnumerator.Current.Value.FixedUpdate_TS();
                        }
                    }

                    // Clean max 1 invalid item per loop. 
                    itemsEnumerator = itemsInVolume.GetEnumerator();

                    while (itemsEnumerator.MoveNext())
                    {
                        if (!itemsEnumerator.Current.Value.valid)
                        {
                            itemsInVolume.Remove(itemsEnumerator.Current.Key);
                            itemsEnumerator = itemsInVolume.GetEnumerator();

                            break;
                        }
                    }

                }
                if (!useThreads || threadCount <= 1)
                {
                    ForceLoop();
                }


            }

            statistics.pointComparisonsPerSecondCounter += statistics.lastPointComparisonCount;
            statistics.itemCount = itemsInVolume.Count;
        }

        void Update()
        {
            /*
            if (useThreads && !waterThreads.trigger && doForce)
            {
                ForceLoop();
                doForce = false;
            }
            */
            statistics.raysPerSecondClock += Time.deltaTime;

            if (statistics.raysPerSecondClock >= 1)
            {
                statistics.raysPerSecond = statistics.raysPerSecondCounter;
                statistics.raysPerSecondCounter = 0;
                statistics.raysPerSecondClock = 0;

                statistics.pointComparisonsPerSecond = statistics.pointComparisonsPerSecondCounter;
                statistics.pointComparisonsPerSecondCounter = 0;
            }

        }


        void OnDrawGizmos()
        {
            foreach (KeyValuePair<int, ItemInWater> kvp in itemsInVolume)
            {
                kvp.Value.OnDrawGizmos();
            }
        }

        Vector3 velocityAtPoint = Vector3.zero;

        private ItemInWater item;

        RaycastHit hit;
        float dragForceMultiplier;
        Vector3 findObjRayStart = Vector3.zero;

        Dictionary<int, ItemInWater>.Enumerator itemsEnumerator;


        public virtual void ForceLoop()
        {
            statistics.lastRayCount = 0;

            // Keep the total amount of force constant regardless of variability in time and raycast spacing
            dragForceMultiplier = Time.fixedDeltaTime * raycastSpacing * raycastSpacing * viscosity;

            itemsEnumerator = itemsInVolume.GetEnumerator();

            if (raycastSpacing < minimumRaycastSpacing)
            {
                Debug.LogError("Volume raycast spacing cannot be less than: " + minimumRaycastSpacing);
                raycastSpacing = 1;// minimumRaycastSpacing;
            }

            while (itemsEnumerator.MoveNext()) // using a cached enumerator saves 48B allocation
            {

                item = itemsEnumerator.Current.Value;

                if (!item.valid)
                {
                    continue;
                }



                if (!item.isNearObserver)
                {
                    item.gameObject.SetActive(false);
                    continue;
                }
                else
                {
                    if (!item.gameObject.activeInHierarchy)
                    {
                        item.gameObject.SetActive(true);
                    }

                }

                if (item.eventFlagFirstTouch)
                {
                    if (OnItemEnteredWater != null)
                    {
                        OnItemEnteredWater(item);
                    }
                    NotifyOfEntry(item.gameObject);

                }

                if (item.eventFlagFullExit)
                {
                    if (OnItemExitedWater != null)
                    {
                        OnItemExitedWater(item);
                    }
                    NotifyOfExit(item.gameObject);
                }


                if (item.eventFlagCenterTouch)
                {
                    if (OnItemCenterEnteredWater != null)
                    {
                        OnItemCenterEnteredWater(item);
                    }
                }


                if (item.eventFlagCenterExit)
                {
                    if (OnItemCenterExitedWater != null)
                    {
                        OnItemCenterExitedWater(item);
                    }
                }

                if (applyViscosityOnlyAtCenter)
                {
                    if (item.IsInside(item.underWaterCenter))
                    {

                        item.ApplyDragAtPosition(-item.forwardVelocity * dragForceMultiplier, item.position, 0);
                    }
                }

                else
                {

                    for (int i = 0; i < item.viscosityRayCount; i++)
                    {
                        findObjRayStart = item.viscosityRayStartPoints[i];
                        statistics.lastRayCount++;

                        // if (Physics.Raycast(viscoscityRay, out hit, 10.0F, layerMask)) // 35% slower!
                        // use the mask to not see the surface as a shielding obstruction when subject is moving up
                        if (Physics.Raycast(findObjRayStart, -item.forwardVelocityNormalized, out hit, item.width, layerMask))
                        {

                            if (hit.collider.gameObject.GetInstanceID() == itemsEnumerator.Current.Key)
                            {

                                if (item.IsInside(hit.point))
                                {

                                    // Drag is applied against the direction of forward motion
                                    velocityAtPoint = item.rigidbody.GetPointVelocity(hit.point);
                                    item.ApplyDragAtPosition(-velocityAtPoint * dragForceMultiplier, hit.point, i);
                                }

                            }
                        }
                    }
                }

                if (item.underWaterPointCount > 0)
                {
                    if (buoyancy > 0)
                    {
                        item.ApplyBuoyancy();
                    }
                    if (flow != Vector3.zero)
                    {
                        item.rigidbody.AddForceAtPosition(flow, item.underWaterCenter);
                    }
                }


            }


            statistics.raysPerSecondCounter += statistics.lastRayCount;

        }


        void OnDisable()
        {
            waterThreads.OnDisable();
        }
    
    }


}