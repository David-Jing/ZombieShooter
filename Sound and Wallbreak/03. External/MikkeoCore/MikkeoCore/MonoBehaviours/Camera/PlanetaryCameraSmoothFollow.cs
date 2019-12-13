using UnityEngine;
using System.Collections;

public class PlanetaryCameraSmoothFollow : MonoBehaviour {
	
	public SmoothFollow2 smoothFollowScriptInMainCamera;
	public SmoothOrbit smoothOrbitScript;
	public Transform target;
	public float portDistance = 12.0f;
	public float portHeight = 5.0f;
	public float shipDistance = 20.0f;
	public float shipHeight = 20.0f;
	public bool smoothRotation = true;
	public bool followBehind = true;
	public float rotationDamping = 10.0f;

	public enum ItemTracked {
		SHIP,
		PORT

	}

	public ItemTracked itemTracked = ItemTracked.PORT;

	void Update () {
		
		
		/*
		Vector3 wantedPosition;
		
		if (transform){
			
			if (itemTracked == ItemTracked.PORT){
				if(followBehind)
					wantedPosition = target.TransformPoint(0, portHeight, -portDistance);
				else
					wantedPosition = target.TransformPoint(0, portHeight, portDistance);			
			}
			else {
				if(followBehind)
					wantedPosition = target.TransformPoint(0, shipHeight, -shipDistance);
				else
					wantedPosition = target.TransformPoint(0, shipHeight, -shipDistance);
	
			}
		}
		transform.position = Vector3.Lerp (transform.position, wantedPosition, Time.deltaTime * damping);
		*/
			
		if (smoothRotation) {
			//Quaternion wantedRotation = Quaternion.LookRotation(target.position - transform.position, target.up);
			Quaternion wantedRotation = target.transform.rotation;
			transform.rotation = Quaternion.Slerp (transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
			
			if (Quaternion.Angle (transform.rotation, wantedRotation) < 20) {
				//We're there
				smoothFollowScriptInMainCamera.Enable ();
			}
			if (Quaternion.Angle (transform.rotation, wantedRotation) < 1) {
				//We're there
				smoothFollowScriptInMainCamera.Disable ();
				smoothOrbitScript.Enable ();
			}

		} else
			transform.LookAt (target, target.up);
		
		//smoothFollowScriptInMainCamera
	}

	
	public void ChangeTarget (GameObject go) {
		
		//GameObject[] tempGOs = newTransform.gameObject.GetComponentsInChildren<GameObject>();
		
		//foreach (GameObject go in tempGOs) {
		
		
		foreach (Transform t in go.transform) {
			
			Debug.Log ("++ " + t.name);
			
			if (t.name == "Earth Port") {
				Debug.Log ("Setting sf target to: " + t.name);
				smoothFollowScriptInMainCamera.Disable ();
				smoothFollowScriptInMainCamera.SetTarget (t);
				smoothFollowScriptInMainCamera.SetFollowPosition (Vector3.up * portHeight + Vector3.back * portDistance);
				
				smoothOrbitScript.Enable ();
				smoothOrbitScript.ChangeTarget (t);
				itemTracked = ItemTracked.SHIP;
				
			}
			
			if (t.name == "Ship Model") {
			
				smoothFollowScriptInMainCamera.Disable ();
				smoothFollowScriptInMainCamera.SetTarget (t);
				smoothFollowScriptInMainCamera.SetFollowPosition (Vector3.up * shipHeight + Vector3.back * shipDistance);

				//For now we just don't orbit the ship.
				smoothOrbitScript.Disable ();
				smoothOrbitScript.ChangeTarget (t);

				itemTracked = ItemTracked.PORT;
			}
		}
		
		Debug.Log ("Setting target to: " + go.name);
		target = go.transform;
		
	}
	
	
}