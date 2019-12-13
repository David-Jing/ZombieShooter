/*

using UnityEngine;
using System.Collections;
using Pathfinding;

[AddComponentMenu("Mikkeo/PathFinding/MikkeoPathFinder")]
public class MikkeoPathFinder : MonoBehaviour {

	public GameObject source;
	public UILabel currentSourceLabel;
	
	public GameObject target;
	public UILabel currentTargetLabel;
	
	public Vector3[] pathToTarget;
	
	private Seeker seeker;
	private Vector3 targetPosition;
	private Vector3 sourcePosition;
	
	NodeLink[] nodeLinks;
	GameObject[] ports;
	
	private int sourceNode=0;
	private int targetNode=10;
	
	public bool setAllWeightsEvenly = true;
	
	public bool showPath = false;
	
	public void Start () {
		
		//Listen to notifications
		NotificationCenter.DefaultCenter.AddObserver (this, "OnChangeSourceToNextPort");
		NotificationCenter.DefaultCenter.AddObserver (this, "OnChangeSourceToPrevPort");
		NotificationCenter.DefaultCenter.AddObserver (this, "OnChangeTargetToNextPort");
		NotificationCenter.DefaultCenter.AddObserver (this, "OnChangeTargetToPrevPort");
		
		//Get the seeker component attached to this GameObject
		seeker = GetComponent<Seeker>();
		seeker.pathCallback += OnPathComplete;
		
		//Start a new path request from the current position to a position 10 units forward.
		//When the path has been calculated, it will be returned to the function OnPathComplete unless it was canceled by another path request
		
		InvokeRepeating("PathUpdate", .2f, .5f);
		
		StartCoroutine(ScanNodesAndPorts());		
		
		if (setAllWeightsEvenly)
			StartCoroutine (SetWeightsEvenly());
		
		
	}
	
	
	void PathUpdate(){
		
		if (target.transform.position != targetPosition){
			seeker.StartPath (source.transform.position, target.transform.position);
			targetPosition = target.transform.position;
		}
		if (source.transform.position != sourcePosition){
			seeker.StartPath (source.transform.position, target.transform.position);
			sourcePosition = source.transform.position;
		}
		
	
	}
	
	public void OnPathComplete (Path p) {
		//We got our path back
		if (p.error) {
			//Nooo, a valid path couldn't be found
			Debug.Log("No valid path");
		} else {
			//Yey, now we can get a Vector3 representation of the path
			//from p.vectorPath
			pathToTarget = p.vectorPath;
		}
	}
	
	
	void OnDrawGizmos(){
	
		Vector3 lastPoint = Vector3.zero;
		
		if (showPath){
		
			foreach (Vector3 point in pathToTarget){
				if (lastPoint == Vector3.zero) {
					Debug.DrawLine(point*1.1f, point, Color.yellow);
					lastPoint = source.transform.position;
				}
				Debug.DrawLine(point* 1.1f, lastPoint * 1.1f, Color.yellow);
				lastPoint = point;
			}
			
			Debug.DrawLine(lastPoint* 1.1f, lastPoint, Color.yellow);
		
		}
	
	}

	
	IEnumerator SetWeightsEvenly (){
	
		yield return new WaitForSeconds(2f);
		
		for (int i=0;i<nodeLinks.Length;i++) {
			float magnitude = Vector3.Distance( nodeLinks[i].gameObject.transform.position, nodeLinks[i].end.transform.position);
			float radius = 40f;
			//See my notes in Bumper Book or email re: calculations
			float magAngle = 2 * Mathf.Asin(.5f * magnitude / radius)*Mathf.Rad2Deg;
			float adjustedMagnitude = 2*Mathf.PI*radius/360*magAngle;
			Debug.Log (magnitude+"|"+adjustedMagnitude);
			nodeLinks[i].costFactor=Mathf.Pow(adjustedMagnitude, 5f);
		}
		
		/*
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag("WayPoint");
		foreach (GameObject go in gos) {
		Debug.Log (go.name);
		
		NodeLink[] nlArray = GetComponentsInChildren<NodeLink>();
		foreach (NodeLink nl in nlArray){
		Debug.Log ("Setting value to "+.0001f);
		nl.costFactor=.0001f;
		}
		
		}
		*/
		

/*
	}
	
	IEnumerator ScanNodesAndPorts(){
		
		yield return new WaitForSeconds(.5f);
		
		//Grabs ALL waypoints
		nodeLinks = FindObjectsOfType (typeof(NodeLink)) as NodeLink[];
		
		//Grab just ports
		ports = GameObject.FindGameObjectsWithTag("Port");

	}
	
	
	private void OnChangeSourceToNextPort(Notification n){
		sourceNode++;
		//Links
		//if (sourceNode > nodeLinks.Length){
		//Ports
		if (sourceNode > ports.Length){
			sourceNode=0;
		}
		UpdateSourceAndTarget();
	}
	private void OnChangeSourceToPrevPort(Notification n){
		sourceNode--;
		if (sourceNode < 0){
			//Links
			//sourceNode=nodeLinks.Length-1;
			//Ports
			sourceNode=ports.Length-1;
		}
		UpdateSourceAndTarget();
	}
	private void OnChangeTargetToNextPort(Notification n){
		targetNode++;
		//Links
		//if (targetNode > nodeLinks.Length){
		//Ports
		if (targetNode > ports.Length){
			targetNode=0;
		}
		UpdateSourceAndTarget();
	}
	private void OnChangeTargetToPrevPort(Notification n){
		targetNode--;
		if (targetNode < 0){
			//Links
			//targetNode=nodeLinks.Length-1;
			//Ports
			targetNode=ports.Length-1;
		}				
		UpdateSourceAndTarget();
	}

	private void UpdateSourceAndTarget(){
		
		//Links
		/*
		target = nodeLinks[targetNode].gameObject;
		currentTargetLabel.text = nodeLinks[targetNode].gameObject.name;
		source = nodeLinks[sourceNode].gameObject;
		currentSourceLabel.text = nodeLinks[sourceNode].gameObject.name;
		*/
		
//Debug.Log (targetNode+"|"+sourceNode);
		
/*
		//Ports
		GameObject portLocation = ports[targetNode].transform.Find("Earth Port").gameObject;
		target = portLocation;
		currentTargetLabel.text = ports[targetNode].name;
		
		portLocation = ports[sourceNode].transform.Find("Earth Port").gameObject;
		source = portLocation;
		currentSourceLabel.text = ports[sourceNode].name;
		
	}
	
	public Vector3[] GetCurrentPath(){
		return pathToTarget;
	}
}

*/
