using UnityEngine;
using System.Collections;

public class FXManager : Photon.MonoBehaviour {
	
	public GameObject shrapnelPrefab;
	public GameObject deathAnimPrefab;
	public GameObject beamPrefab;
	
	static FXManager _instance;
	static public FXManager Instance {
		get { return _instance; }
	}
	
	void Start() {
		_instance = this;
	}
	
	[RPC]
	void DoDeathAnimation( Vector3 pos) {
		//Debug.Log ("Shrapnel!");
		Instantiate(deathAnimPrefab, pos, Quaternion.identity);
	}
}
