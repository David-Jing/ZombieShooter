using UnityEngine;
using System.Collections;

public class RotateItem : MonoBehaviour {

	public float rpm = .1f;

	void FixedUpdate () {
		//iTween.RotateUpdate (gameObject, iTween.Hash ("y", rpm / 60f, "time", 60f));
		transform.RotateAround (transform.position, transform.TransformDirection (Vector3.up), Time.fixedDeltaTime * rpm * 360f);

	}
}

