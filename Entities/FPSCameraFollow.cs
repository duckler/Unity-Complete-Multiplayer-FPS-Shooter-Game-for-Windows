using UnityEngine;
using System.Collections;

public class FPSCameraFollow : MonoBehaviour {
	
	public Transform target;

	void FixedUpdate () {
		transform.position = target.position + Vector3.up * 1.8f;
	}
}
