using UnityEngine;
using System.Collections;

public class BeamLock : MonoBehaviour {
	
	public Transform orig;
	public Vector3 dest;
	Transform _transform;
	
	void Start() {
		_transform = transform;
		AudioSource.PlayClipAtPoint(audio.clip, _transform.position, .1f);
	}

	// Update is called once per frame
	void Update () {
		if(orig == null) {
			Destroy (gameObject);
			return;
		}
		
		_transform.position = orig.position;
		_transform.rotation = Quaternion.FromToRotation(Vector3.forward, dest-orig.position);
		Vector3 s = new Vector3(1, 1, Vector3.Distance(orig.position, dest) );
		_transform.localScale = s;
	
	}
}
