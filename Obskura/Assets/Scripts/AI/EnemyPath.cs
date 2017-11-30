using System.Collections;
using UnityEngine;

public class EnemyPath : MonoBehaviour {
	
	public EnemyPath nextNode;

	public Vector3 GetNextNodePosition(){	//connect the nodes to create path for enemy to move in idle state
		return nextNode.GetPosition ();
	}
	void OnDrawGizmos(){	// Whole function used to give visual debugging of the path in scene view only
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position,0.25f);
		if (nextNode != null) {
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine (GetPosition (), GetNextNodePosition ());
		}
	}
	public Vector3 GetPosition(){	//used to help with Gizmos
		return transform.position;
	}
}
