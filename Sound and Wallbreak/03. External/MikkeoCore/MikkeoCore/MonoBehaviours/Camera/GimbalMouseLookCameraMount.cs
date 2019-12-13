using UnityEngine;
using System.Collections;

public class GimbalMouseLookCameraMount : MonoBehaviour {

	public Transform itemCameraIsMountedTo;
	public bool isEnabled = false;

	void Update () {
		if ((itemCameraIsMountedTo) && (isEnabled))
			transform.position = itemCameraIsMountedTo.position;
	}
		
}
