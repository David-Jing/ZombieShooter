/*
 * using UnityEngine;
using System.Collections;
//Include generics to be able to use Generics
using System.Collections.Generic;
//Include the Pathfinding namespace to gain access to a lot of useful classes
using Pathfinding;
using Pathfinding.Serialization.JsonFx;

//Inherit our new graph from a base graph type
[JsonOptIn]
public class PolarGraph : NavGraph {
	
	[JsonMember]
	public int circles = 12;
	[JsonMember]
	public int steps = 24;
	[JsonMember]
	public Vector3 center = Vector3.zero;
	
	[JsonMember]
	public float scale = 2;
	
	[JsonMember]
	// Max distance for a connection to be valid. 0 = infinity 
	public float maxDistance = 0;
	
	[JsonMember]
	//** Max distance along the axis for a connection to be valid. 0 = infinity 
	public Vector3 limits;
	
	[JsonMember]
	//** Use raycasts to check connections 
	public bool raycast = true;
		
	[JsonMember]
	//** Use thick raycast 
	public bool thickRaycast = false;
	
	[JsonMember]
	//** Thick raycast radius 
	public float thickRaycastRadius = 1;

	[JsonMember]
	//** Layer mask to use for raycast 
	public LayerMask mask;
	
	
	public override void Scan () {
		
		//Create an array containing all nodes
				
		//MakeSphereOld();
		MakeSphere(scale, center, scale*2/circles, 360/steps);
		
		
		//Now all nodes are created, let's create some connections between them!
		//Loop through all circles except the first one which is only one point
		for (int i=1;i<circles*2;i++) {
			for (int j=0;j<steps;j++) {
				//Get the current node
				Node node = nodes[(i-1)*steps + j + 1];
				
				//The nodes here will always have exactly four (actually, eight) connections, like a grid, but polar.
				//Except for those in the last circle which will only have three (five) connections
				//int numConnections = (i < circles-1) ? 4 : 3;
				int numConnections = (i < circles-1) ? 8 : 5;
				Node[] connections = new Node[numConnections];
				int[] connectionCosts = new int[numConnections];
						
				//Get the next clockwise node in the current circle.
				//If j++ would overflow steps, it would create a connection to the first node in the next circle, so if it does
				//we have to prevent it by setting the steps index to 0 which will then create a connection to the correct node
				int connId = (i-1)*steps + (j < steps-1 ? j+1 : 0) + 1;
				// 0 = clockwise				
				connections[0] = nodes[connId];
				
				//Counter clockwise node. Here we check for underflow instead
				connId = (i-1)*steps + (j > 0 ? j-1 : steps-1) + 1;
				// 1 = counter clockwise
				connections[1] = nodes[connId];
				
				//The node in the prev circle (out from the center)
				if (i > 1) {
					connId = (i-2)*steps + j + 1;
				} else {
					//Create a connection to the middle node, special case
					connId = 0;
				}			
				connections[2] = nodes[connId];

				//The node in the prev circle (out from the center CCW)
				if (i > 1) {
					connId = (i-1)*steps + (j < steps-1 ? j+1 : 0) + 1;
				}			
				connections[3] = nodes[connId];
				
				//The node in the prev circle (out from the center CW)
				if (i > 1) {
					connId = (i-1)*steps + (j > 0 ? j-1 : steps-1) + 1;
				}			
				connections[4] = nodes[connId];


				
				//Are there any more circles outside this one?
				//if (numConnections == 4) {
				if (numConnections == 8) {
					
					//The node in the next circle (out from the center)
					connId = i*steps + j + 1;					
					connections[5] = nodes[connId];					
					
					//The node in the next circle (out from the center CW)
					connId = (i)*steps + (j < steps-1 ? j+1 : 0) + 1;
					connections[6] = nodes[connId];
					
					//The node in the next circle (out from the center CCW)
					connId = (i)*steps + (j > 0 ? j-1 : steps-1) + 1;
					connections[7] = nodes[connId];
				}
				
				

				
				
				//Connection costs now
				for (int q=0;q<connections.Length;q++) {
					//Node.position is an Int3, here we get the cost of moving between the two positions
					connectionCosts[q] = (node.position-connections[q].position).costMagnitude*1000;
					if ( (q==3) || (q==4) || (q==6) || (q==7) ){
						connectionCosts[q] = (int)System.Math.Round(connectionCosts[q]*.01f);
					}
				}
				
				
				node.connections = connections;
				node.connectionCosts = connectionCosts;
			}
		}
		
		//The center node is a special case, so we have to deal with it separately
		Node centerNode = nodes[0];
		centerNode.connections = new Node[steps];
		centerNode.connectionCosts = new int[steps];
		
		//Assign all nodes in the first circle as connections to the center node
		for (int j=0;j<steps;j++) {
			centerNode.connections[j] = nodes[1+j];		
			//centerNode.position is an Int3, here we get the cost of moving between the two positions
			centerNode.connectionCosts[j] = (centerNode.position-centerNode.connections[j].position).costMagnitude;
		}
		
		//All nodes set up, now how do we deal with colliders on each node-node connection?
		//One assumes a raycast is done in some clever way
		//if a hit is made the node[i].walkable = false;
		
		//Set all the nodes to be walkable
		for (int i=0; i<nodes.Length; i++) {
			//if ( (nodes[i] != null) && (nodes[i].connections[j] != null) ){
			
			if (nodes[i].connections != null) {

				for (int j=0; j<nodes[i].connections.Length; j++){
			
					if (IsValidConnection(nodes[i], nodes[i].connections[j]))
						nodes[i].walkable = true;
					else
						nodes[i].walkable = false;
				}
			}
			
		}
	}
	
	
	/** Returns if the connection between \a a and \a b is valid.
	 * Checks for obstructions using raycasts (if enabled) and checks for height differences.\n
	 * As a bonus, it outputs the distance between the nodes too if the connection is valid 
	public bool IsValidConnection (Node a, Node b) {
		
		//return true;
		
		//if ((a == null) || (b == null)) return false;
		
		float dist = 0;
		
		//if (!a.walkable || !b.walkable) return false;
		
		Vector3 dir = (Vector3)(a.position-b.position);
		
		if (
			(!Mathf.Approximately (limits.x,0) && Mathf.Abs (dir.x) > limits.x) ||
			(!Mathf.Approximately (limits.y,0) && Mathf.Abs (dir.y) > limits.y) ||
			(!Mathf.Approximately (limits.z,0) && Mathf.Abs (dir.z) > limits.z))
		{
			return false;
		}
		
		dist = dir.magnitude;
		if (maxDistance == 0 || dist < maxDistance) {
			
			if (raycast) {
				
				Ray ray = new Ray ((Vector3)a.position,(Vector3)(b.position-a.position));
				Ray invertRay = new Ray ((Vector3)b.position,(Vector3)(a.position-b.position));
				
				if (thickRaycast) {
					if (!Physics.SphereCast (ray,thickRaycastRadius,dist,mask) && !Physics.SphereCast (invertRay,thickRaycastRadius,dist,mask)) {
						return true;
					}
				} else {
					if (!Physics.Raycast (ray,dist,mask) && !Physics.Raycast (invertRay,dist,mask)) {
						return true;
					}
				}
			} else {
				return true;
			}
		}
		return false;
	}
	
	
	
	public void MakeSphere(float sphereRadius, Vector3 center, float heightStep, float degreeStep) {
		
		nodes = CreateNodes ((circles*steps*2)+2);

	    for (float y = center.y - sphereRadius; y <= center.y + sphereRadius; y+=heightStep) {
			
	        float radius = GetSphereRadiusAtHeight(sphereRadius, y - center.y); //get the radius of the sphere at this height
	        if (radius == 0) {
				//for the top and bottom points of the sphere add a single point
	            AddNewPoint( (Mathf.Sin(0) * radius) + center.x, y, (Mathf.Cos(0) * radius) + center.z, " Pole");
	        } else { 
				//otherwise step around the circle and add points at the specified degrees
	            for (float d = 0; d <= 360; d += degreeStep) {
	                AddNewPoint((Mathf.Sin(d*Mathf.Deg2Rad) * radius) + center.x, y+center.y, (Mathf.Cos(d*Mathf.Deg2Rad) * radius) + center.z, y.ToString()+"|");
	            }
	        }
	    }
	}

	float GetSphereRadiusAtHeight(float sphRad, float height) {
	    return Mathf.Sqrt((sphRad * sphRad) - (height * height));
	}

	private int pointCount=0;
	void AddNewPoint(float x, float y, float z, string comment = ""){
		
		//GameObject go = (GameObject) Instantiate(cubePrefab, transform.position + center + new Vector3(x, y, z), Quaternion.identity);
		//go.transform.parent = gameObject.transform;

		Vector3 pos = center + new Vector3(x, y, z);
		
		Node node = nodes[pointCount];
		node.position = (Int3)pos;
		pointCount++;
		Debug.Log (comment+"|"+pointCount+" / "+((circles*steps)+2));
		
	}
	
	
	void MakeSphereOld(){
				
		nodes = CreateNodes (circles*steps);
		
		Matrix4x4 matrix = Matrix4x4.TRS (center,Quaternion.identity,Vector3.one*scale);
		nodes[0].position = (Int3)matrix.MultiplyPoint (Vector3.zero);
							
		//The number of angles (in radians) each step will use
		float anglesPerStep = (Mathf.PI*2)/steps;
		
		//Y axis
		float yAxisOffset=0;
		//float yAnglesPerStep = Mathf.PI/circles;
		
		
		//Make a sphere, not a flat set of circles
		for (int i=1;i<circles;i++) {
			
			float yAngle = ( 90f - (180f/circles*i) ) * Mathf.Deg2Rad;
			
			yAxisOffset = (float) Mathf.Cos(yAngle);
			Debug.Log ("Y angle: "+Mathf.Rad2Deg * yAngle+", scale: "+yAxisOffset );
			//yAxisOffset += .1f;
			
			for (int j=0;j<steps;j++) {
				//Get the angle to the node relative to the center
				float angle = j * anglesPerStep;
				//Get the direction towards the node from the center
				//Vector3 pos = new Vector3 (Mathf.Sin (angle), yAxisOffset, Mathf.Cos (angle));
				Vector3 pos = new Vector3 (Mathf.Sin (angle), yAxisOffset, Mathf.Cos (angle));
				//Multiply it with scale and the circle number to get the node position in graph space
				//pos *= i*scale;
				pos *= i*scale;
				//Multiply it with the matrix to get the node position in world space
				pos = matrix.MultiplyPoint (pos);
				//Get the node from an index
				//The middle node is at index 0
				//The first circle is from 1...steps-1
				//The second circle is from steps...2*steps-1
				//The third is from 2*steps...3*steps-1 and so on
				Node node = nodes[(i-1)*steps + j + 1];
				//Assign the node position
				node.position = (Int3)pos;
			}
		}
	}
}
*/