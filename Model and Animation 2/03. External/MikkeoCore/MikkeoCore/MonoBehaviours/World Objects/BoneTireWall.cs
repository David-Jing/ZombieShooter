using UnityEngine;
using System.Collections;

public enum TireWallBone {
	TIRES01, TIRES02, TIRES03,
	TIRES04, TIRES05, TIRES06,
	TIRES07, TIRES08, TIRES09, TIRES10,
	TIRES11, TIRES12, TIRES13,
	TIRES14, TIRES15, TIRES16,
	TIRES17, TIRES18, TIRES19
}
	
public class BoneTireWall : MonoBehaviour {

	public TireWallBone tireWallID;
	
	private float radius = .45f;
	
	public BoneManager boneManager;
	
	//private static BoneManager boneManager;

	// Use this for initialization
	void Awake() {
		//trans = transform;
	
		// Get the bone manager
		if (boneManager == null) {
			//GameObject mgrGo = GameObject.FindGameObjectWithTag("BoneManager");
			// nice try, no cigar GameObject mgrGo = transform.parent.parent.gameObject;
			//boneManager = (BoneManager) mgrGo.GetComponent(typeof(BoneManager));
		}
		
		
	}

	void Start () {
		boneManager.AttachBone(gameObject, (int) tireWallID);	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	void OnDrawGizmos () {		
		Gizmos.color = new Color (0.5f,0.5f,1.0f,0.5f);
		// Gizmos.DrawCube (transform.position, new Vector3 (0.6f, 0.6f, 0.6f));
		
		Gizmos.DrawSphere(transform.position, radius);
	}	
	
}
