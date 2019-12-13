using UnityEngine;
using System.Collections;

public class SphereCreator : MonoBehaviour {
	
	
	public GameObject cubePrefab;
	
	public Vector3 center = Vector3.zero;
	public float sphereRadius = 10f;
	public float numberOfRings = 10f;
	public float numberOfSpokes = 20f;
		
	// Use this for initialization
	void Start () {
		
		MakeSphere(sphereRadius, center, sphereRadius/numberOfRings, 360/numberOfSpokes);
		
	}
	
	public void MakeSphere(float sphereRadius, Vector3 center, float heightStep, float degreeStep) {
		
	    for (float y = center.y - sphereRadius; y <= center.y + sphereRadius; y+=heightStep) {
			
	        float radius = GetSphereRadiusAtHeight(sphereRadius, y - center.y); //get the radius of the sphere at this height
	        if (radius == 0) {
				//for the top and bottom points of the sphere add a single point
	            AddNewPoint( (Mathf.Sin(0) * radius) + center.x, y, (Mathf.Cos(0) * radius) + center.z);
	        } else { 
				//otherwise step around the circle and add points at the specified degrees
	            for (float d = 0; d <= 360; d += degreeStep) {
	                AddNewPoint((Mathf.Sin(d*Mathf.Deg2Rad) * radius) + center.x, y, (Mathf.Cos(d*Mathf.Deg2Rad) * radius) + center.z);
	            }
	        }
	    }
	}

	float GetSphereRadiusAtHeight(float sphRad, float height) {
	    return Mathf.Sqrt((sphRad * sphRad) - (height * height));
	}
	
	void AddNewPoint(float x, float y, float z){
		GameObject go = (GameObject) Instantiate(cubePrefab, transform.position + center + new Vector3(x, y, z), Quaternion.identity);
		go.transform.parent = gameObject.transform;
	}
	

	
}
