using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace WaterVolume
{
    public class WaterVolumeTestUI : MonoBehaviour
    {

        #region testObjects
        private List<GameObject> testObjects;
        public GameObject plankPrefab;
        public GameObject cubePrefab;
        public GameObject cylinderPrefab;
        public GameObject pontoonBoatPrefab;
        #endregion

        public GameObject spawnPoint;
        public WaterVolume waterVolume;

        public Vector3 spawnVelocity = Vector3.zero;



        #region stats and sliders
        public Text itemCountText;
        public Text rayCountText;
        public Text raysPerSecond;
        public Text pointsPerSecond;

        public Text raySpacingText;
        public Slider spaceBetweenRaycastsSlider;


        public Text viscosityText;
        public Slider viscositySlider;


        public Text buoyancyText;
        public Slider buoyancySlider;


        public Text waveScaleText;
        public Slider waveScaleSlider;

        public Text waveSpeedText;
        public Slider waveSpeedSlider;

        public Text waterFlowText;
        public Slider waterFlowSlider;

        public Toggle threading;
        public Toggle useMeshes;

        public InputField spawnCount;

        #endregion

        // Use this for initialization
        void Start()
        {
            testObjects = new List<GameObject>();

            raySpacingText.text = waterVolume.raycastSpacing.ToString();
            spaceBetweenRaycastsSlider.value = waterVolume.raycastSpacing;

            viscosityText.text = waterVolume.viscosity.ToString();
            viscositySlider.value = waterVolume.viscosity;

            buoyancyText.text = waterVolume.buoyancy.ToString();
            buoyancySlider.value = waterVolume.buoyancy;


            waveScaleText.text = waveScaleSlider.value.ToString();
            waveSpeedText.text = waveSpeedSlider.value.ToString();

            foreach (GameObject go in waterVolume.waterMeshes)
            {
                go.BroadcastMessage("SetWaveScale", waveScaleSlider.value, SendMessageOptions.DontRequireReceiver);
            }
        }

        // Update is called once per frame
        void Update()
        {
            itemCountText.text = waterVolume.statistics.itemCount.ToString("G");
            rayCountText.text = waterVolume.statistics.lastRayCount.ToString("G");

            raysPerSecond.text = waterVolume.statistics.raysPerSecond.ToString("N0");
            pointsPerSecond.text = waterVolume.statistics.pointComparisonsPerSecond.ToString("N0");
        }

        public void AddPlank()
        {
            AddGameObject(plankPrefab).transform.Rotate(new Vector3(0, 0, 45));

        }

        public void AddPontoonBoat()
        {
            AddGameObject(pontoonBoatPrefab,1,8);
        }

        public void AddCube()
        {
            AddGameObject(cubePrefab);
        }

        public void AddCubes(int count)
        {
            AddGameObject(cubePrefab, count, 1);
        }

        public void AddCylinder()
        {
            AddGameObject(cylinderPrefab);
        }

        public void Reset()
        {
            foreach (GameObject go in testObjects)
            {
                Destroy(go);
            }

            testObjects.Clear();
        }

        private GameObject AddGameObject(GameObject prefab, int count = 1, int offset = 2)
        {
           
            count = System.Convert.ToInt32( spawnCount.text);
            GameObject go = null;

            int i = 0;
            for (int x = 0; x < Mathf.Sqrt(count); x++)
            {
                for (int y = 0; y < Mathf.Sqrt(count); y++)
                {

                   // go = (GameObject)GameObject.Instantiate(prefab, spawnPoint.transform.position +
                    //                                                 Vector3.right * x * offset + (Vector3.left * count * offset / 2f) + Vector3.back  * y * offset + (Vector3.forward * count * offset / 2f), Quaternion.identity);
                    //                    
                    go = (GameObject)GameObject.Instantiate(prefab, spawnPoint.transform.position +
                                                                    Vector3.right * x * offset + (Vector3.left * Mathf.Sqrt(count) * offset / 2f) +
                                                                    Vector3.back * y * offset + (Vector3.forward * Mathf.Sqrt(count) * offset / 2f), 
                                                                    Quaternion.identity);
                    testObjects.Add(go);

                    if (go.GetComponent<Rigidbody>() != null)
                    {
                        go.GetComponent<Rigidbody>().velocity = spawnVelocity;

                    }
                    go.name += " " + Time.realtimeSinceStartup;
                    i++;
                    if (i == count)
                    {
                        break;
                    }
                }
                if (i == count)
                {
                    break;
                }
            }
            /*
                for (int i = 0; i < count; i++)
                {
                    go = (GameObject)GameObject.Instantiate(prefab, spawnPoint.transform.position + Vector3.right * i * offset + (Vector3.left * count * offset / 2f), Quaternion.identity);
                    testObjects.Add(go);
                    if (go.GetComponent<Rigidbody>() != null)
                    {
                        go.GetComponent<Rigidbody>().velocity = spawnVelocity;

                    }
                    go.name += " " + Time.realtimeSinceStartup;
                }
            */
            return go;
        }


        public void OnWaveScaleSlider(float value)
        {
            foreach (GameObject go in waterVolume.waterMeshes)
            {
                go.BroadcastMessage("SetWaveScale", value, SendMessageOptions.DontRequireReceiver);
            }
            waveScaleText.text = value.ToString();
        }

        public void OnRaycastSpacingSlider(float value)
        {
            waterVolume.raycastSpacing = value;
            raySpacingText.text = waterVolume.raycastSpacing.ToString();
        }

        public void OnBuoyancySlider(float value)
        {
            waterVolume.buoyancy = value;
            buoyancyText.text = waterVolume.buoyancy.ToString();
        }

        public void OnViscositySlider(float value)
        {
            waterVolume.viscosity = value;
            viscosityText.text = waterVolume.viscosity.ToString();
        }

        public void OnWaveSpeedSlider(float value)
        {
            foreach (GameObject go in waterVolume.waterMeshes)
            {
                go.BroadcastMessage("SetWaveSpeed", value, SendMessageOptions.DontRequireReceiver);
            }
            waveSpeedText.text = value.ToString();
        }

        public void OnWaterFlowSlider(float value)
        {
            waterVolume.flow.z = value;
        }


        public void OnThreading(bool value)
        {
            waterVolume.useThreads = value;
        }

        public void OnUseMeshes(bool value)
        {
            if (value)
            {
                waterVolume.waterBox.center = new Vector3(waterVolume.waterBox.center.x, waterVolume.waterBox.center.y+0.1f, waterVolume.waterBox.center.z);
            }
            else
            {
                waterVolume.waterBox.center = new Vector3(waterVolume.waterBox.center.x, waterVolume.waterBox.center.y - 0.1f, waterVolume.waterBox.center.z);

            }
            waterVolume.useMeshesAsSurface = value;
        }

    }
}
