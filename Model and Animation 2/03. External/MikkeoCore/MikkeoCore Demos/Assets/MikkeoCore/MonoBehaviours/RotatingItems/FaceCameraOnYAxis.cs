using UnityEngine;
using System.Collections;
using System.Collections.Specialized;

public class FaceCameraOnYAxis : MonoBehaviour {
	
	public Camera mainCamera;
	
	// Use this for initialization
	void Start () {
		mainCamera = Camera.main;
	}

	// Update is called once per frame
	void Update () {
		
		gameObject.transform.LookAt (mainCamera.transform);
		Quaternion q = gameObject.transform.rotation;
		q.eulerAngles = Vector3.up * q.eulerAngles.y;
		gameObject.transform.rotation = q;
	}
}
