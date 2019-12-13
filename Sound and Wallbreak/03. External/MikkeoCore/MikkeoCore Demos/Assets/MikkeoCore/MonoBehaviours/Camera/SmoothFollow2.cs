using UnityEngine;
using System.Collections;

public class SmoothFollow2 : MonoBehaviour, ICameraController {
	
	public Transform target = null;
	public Vector3 cameraOffset = Vector3.back * 10f + Vector3.up * 10f;
	public float damping = 1.0f;
	public bool smoothRotation = true;
	public float rotationDamping = 1.0f;
	public bool isEnabled = false;
	
	Vector3 wantedPosition;

	void Update () {
		
		if (isEnabled) {
			
			wantedPosition = target.TransformPoint (cameraOffset);
					
			transform.position = Vector3.Lerp (transform.position, wantedPosition, Time.deltaTime * damping);
			
			if (smoothRotation) {
				Quaternion wantedRotation = Quaternion.LookRotation (target.position - transform.position, target.up);
				transform.rotation = Quaternion.Slerp (transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
			} else
				transform.LookAt (target, target.up);
		}
	}

	public void SetTarget (Transform newTransform) {
		target = newTransform;
	}

	public void SetFollowPosition (Vector3 v3) {
		wantedPosition = v3;
	}


	#region ICameraController

	public CameraControllerType GetCameraControllerType () {
		return CameraControllerType.SMOOTH_FOLLOW_2;
	}

	public void Enable () {
		isEnabled = true;
	}

	public void Disable () {
		isEnabled = false;
	}

	#endregion




}
