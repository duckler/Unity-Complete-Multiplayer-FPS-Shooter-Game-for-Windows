using UnityEngine;
using System.Collections;

public class ShrapnelFX : MonoBehaviour {
	
	public float force = 10f;
	public float timeToLive = 1f;
	
	// Use this for initialization
	void Start () {
		Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
		foreach(Rigidbody rb in rbs) {
//			Debug.Log (rb.name);
			rb.AddExplosionForce(force, transform.position - transform.up + Random.insideUnitSphere/3f, 5f);
			Destroy(rb.gameObject, timeToLive + Random.Range(0f,.5f));
		}
		
		Destroy(gameObject, timeToLive*2);
	}
	
}
