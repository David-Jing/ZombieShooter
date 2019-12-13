/*
 * 
 
CarSmoothFollow

target = self explanatory
distance = Standard distance to follow object
height = The height of the camera
heightDamping = Smooth out the height position

lookAtHeight = An offset of the target
lookAtZOffset - Look ahead of the car, improves the view

rotationSnapTime = The time it takes to snap back to original rotation
distanceSnapTime = The time it takes to snap back to the original distance or the zoomed distance (depending on speed of parentRigidyBody)

distanceMultiplier = Amount camera MOVES BACK from car as speed increases - Make this around 0.1f for a small zoom out or 0.5f for a large zoom (depending on the speed of your rigidbody)
heightMultiplier = Amount camera MOVES UP LIKE A HELICOPTER as speed increases - Make this around 0.1f for a small zoom out or 0.5f for a large zoom (depending on the speed of your rigidbody)
lookAheadSpeedMultiplier = Amount camera LOOKS AHEAD OF THE CAR as speed increases - Make this around 0.1f for a small zoom out or 0.5f for a large zoom (depending on the speed of your rigidbody)

 * 
 */
	

using UnityEngine;
using System.Collections;
 
public class CarSmoothFollow : MonoBehaviour {
 
	public GameObject target = null;
	
	public float distanceFromCar = 8.0f;
	public float distanceSpeedMultiplier = .1f;
	
	public float height = 3.0f;
	public float heightDamping = 2.0f;
	public float heightSpeedMultiplier =.1f;

	//Look Ahead of the target
	//public float lookAtHeight = 0.0f;
	public float lookAtZOffset = 0.5f;
	public float lookAheadSpeedMultiplier =.1f;
	

	
	private Rigidbody parentRigidbody;
 
	//public bool smoothRotation = true;
	
	//public float rotationSnapTime = 0.3F;
	//public float distanceSnapTime = 0.1f;
	public float cameraPositionDamping = 1f;
	public float cameraRotationDamping = 1f;
	
	
 
		
	//private Vector3 lookAtVector;
 
	//private float usedDistance;
 
	//float wantedRotationAngle;
	float wantedHeight;
 
	//float currentRotationAngle;
	float currentHeight;
 
	//Quaternion currentRotation;
	Vector3 wantedPosition;
 
	//private float yVelocity = 0.0F;
	//private float zVelocity = 0.0F;
 
	void Start () {
 
		//lookAtVector =  new Vector3(0,lookAtHeight,lookAtZOffset);
 
	}

	public void SetTarget(GameObject targetGO){
		target = targetGO;
		parentRigidbody = (Rigidbody)target.GetComponent<Rigidbody>();
	}
	
	
	void LateUpdate () {
		
		if ((target)&&(parentRigidbody) ){
			
			//lookAtVector =  new Vector3(0, lookAtHeight, lookAtZOffset);
	 
			//Height
			wantedHeight = target.transform.position.y + height + (parentRigidbody.velocity.magnitude * heightSpeedMultiplier);
			currentHeight = Mathf.Lerp(transform.position.y, wantedHeight, heightDamping * Time.deltaTime);
	 
			//Rotation
			//wantedRotationAngle = target.transform.eulerAngles.y;
			//currentRotationAngle = transform.eulerAngles.y;
			//currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, wantedRotationAngle, ref yVelocity, rotationSnapTime);
			
			//wantedPosition = target.transform.position;
			wantedPosition = target.transform.TransformPoint( Vector3.forward * ( -distanceFromCar - (distanceSpeedMultiplier * parentRigidbody.velocity.magnitude)));	
			
			wantedPosition = Vector3.Lerp(transform.position, wantedPosition, cameraPositionDamping * Time.deltaTime);
	 
			//Debug.DrawRay(target.transform.position, target.transform.TransformDirection (Vector3.forward) * 5.0f );
			wantedPosition.y = currentHeight;
	 
			//Rotation of camera
			//usedDistance = Mathf.SmoothDampAngle(usedDistance, distance + (parentRigidbody.velocity.magnitude * distanceMultiplier), ref zVelocity, distanceSnapTime);  
			//wantedPosition += Quaternion.Euler(0, currentRotationAngle, 0) * new Vector3(0, 0, -usedDistance);
			
			
			//Put the camera in the calculated position
			transform.position = wantedPosition;
			
			/*
			if (smoothRotation) {
					Quaternion wantedRotation = Quaternion.LookRotation(target.transform.position - transform.position, target.transform.up);
					transform.rotation = Quaternion.Slerp (transform.rotation, wantedRotation, Time.deltaTime * cameraPositionDamping);
				}
				else 
					//transform.LookAt (target, target.up);
			*/
					//Look at the position we want to
			
			Vector3 wantedLookAtPosition = target.transform.TransformPoint(Vector3.forward * (lookAtZOffset + (lookAheadSpeedMultiplier * parentRigidbody.velocity.magnitude)));
			
			Quaternion wantedRotation = Quaternion.LookRotation(wantedLookAtPosition - transform.position, target.transform.up);
			transform.rotation = Quaternion.Slerp (transform.rotation, wantedRotation, Time.deltaTime * cameraRotationDamping);
			
			//transform.LookAt( target.transform.TransformPoint(Vector3.forward * (lookAtZOffset + (lookAheadSpeedMultiplier * parentRigidbody.velocity.magnitude) )));
					
			
			
		}
		
	}
 
}