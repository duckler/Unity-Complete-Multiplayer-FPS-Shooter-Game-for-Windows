using UnityEngine;
using System.Collections;

// join my blog > http://bit.ly/unity3diy

public class SpawnPoint : MonoBehaviour {
	
	public float radius = 10f;
	
	// Use this for initialization
	void Start () {
		NetworkManager.Instance.RegisterSpawnPoint(this);
	}
	
	public bool IsClear() {
		Collider[] cols = Physics.OverlapSphere(transform.position, radius);
		foreach(Collider c in cols) {
			if(c.tag=="Player") {
				return false;
			}
		}
		return true;
	}

	void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere (transform.position + Vector3.up, 1);
	}

}

// join my blog > http://bit.ly/unity3diy