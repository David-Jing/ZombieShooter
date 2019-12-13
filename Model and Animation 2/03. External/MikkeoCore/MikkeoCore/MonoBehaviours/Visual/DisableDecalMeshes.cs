using UnityEngine;
using System.Collections;

public class DisableDecalMeshes : MonoBehaviour {
	
	public float meshSetUpDelay = .2f;
	private string[] meshesToDisableByName = { "Ones", "Tens", "RectangleLogo", "SquareLogo", "LogosRectangle", "LogosSquare" };
	
	// Use this for initialization
	IEnumerator Start () {
	
		yield return new WaitForSeconds(meshSetUpDelay);
		
		MeshRenderer[] mrs = (MeshRenderer[])gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer mr in mrs){
			
			foreach (string s in meshesToDisableByName){
				//Debug.Log (s+"|"+mr.gameObject.name);
				if (mr.gameObject.name == s){
					mr.enabled = false;					
				}
			}
		}
		
	}
	
}
