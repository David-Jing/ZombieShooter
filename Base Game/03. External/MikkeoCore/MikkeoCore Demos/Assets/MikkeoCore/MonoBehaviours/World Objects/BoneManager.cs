using UnityEngine;
using System.Collections;

public class BoneManager : MonoBehaviour {

	private SkinnedMeshRenderer mesh;
	
	public bool firstBoneIsRoot = true;
	
	private int startIndex = 0;
	private int endIndex = 0;
	
	private Transform[] parents;
	
	private Transform[] bones; 
	
	// Use this for initialization
	void Awake () {
		mesh = (SkinnedMeshRenderer) GetComponentInChildren(typeof(SkinnedMeshRenderer));
				
		if (firstBoneIsRoot) startIndex = 1;
		endIndex = mesh.bones.Length;
		
		parents = new Transform[endIndex];
		bones = new Transform[endIndex];
	}
	
	void Start () {
		for (int i=0; i < endIndex ;i++) {
			bones[i] = mesh.bones[i];
		}
	}
	
	// Update is called once per frame
	void LateUpdate () {
		// move bones to position and rotation of parent objects.
	
		for (int i = startIndex; i < endIndex; i++) {
			if (parents[i] != null) {
				bones[i].position = parents[i].position;
				bones[i].rotation = parents[i].rotation;				
			}
		}
	}
	
	public void AttachBone (GameObject parent, int index) {
		
		index += startIndex;		
		parents[index] = parent.transform;
		
	}
}
