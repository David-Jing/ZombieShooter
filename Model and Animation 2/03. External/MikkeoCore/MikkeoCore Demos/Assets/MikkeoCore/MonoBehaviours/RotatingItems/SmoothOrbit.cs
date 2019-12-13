using UnityEngine;
using System.Collections;

public class SmoothOrbit : MonoBehaviour, IEnableable {

	public bool isEnabled = false;
	public Transform target;
	public float orbitalVelocity = 20f;
	public bool parentToTransform = true;

	void Awake () {
		ChangeTarget (target);
	}

	void Update () {
		if (isEnabled && transform)
			transform.RotateAround (target.position, target.TransformDirection (Vector3.down), orbitalVelocity * Time.deltaTime);
	}


	public void ChangeTarget (Transform newTransform) {
		target = newTransform;
		if (parentToTransform)
			transform.parent = target;
	}

	#region IEnableable

	public void Enable () {
		isEnabled = true;
	}

	public void Disable () {
		isEnabled = false;
	}

	#endregion
}
