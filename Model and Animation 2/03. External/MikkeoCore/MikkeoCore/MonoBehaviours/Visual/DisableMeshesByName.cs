using UnityEngine;
using System.Collections;

public class DisableMeshesByName : MonoBehaviour {
	
	public float meshSetUpDelay = .2f;
	public string[] meshesToDisableByName;
	
	// Use this for initialization
	IEnumerator Start () {
	
		yield return new WaitForSeconds(meshSetUpDelay);
		
		MeshRenderer[] mrs = (MeshRenderer[])gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer mr in mrs){
			
			foreach (string s in meshesToDisableByName){
				if (mr.gameObject.name == s){
					mr.enabled = false;					
				}
			}
		}
		
	}
	
}
