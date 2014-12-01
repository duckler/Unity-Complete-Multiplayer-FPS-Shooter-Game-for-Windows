using UnityEngine;
using System.Collections;

public class PickupSpawner : MonoBehaviour {
	
	public float respawnTimer = 20f;
	float respawnTimerRemaining;
	public string pickupPrefabName;
	GameObject currentPickup;

	// Update is called once per frame
	void Update () {
		if(!PhotonNetwork.isMasterClient || currentPickup != null) {
			return;
		}
		
		respawnTimerRemaining -= Time.deltaTime;
		if(respawnTimerRemaining < 0 && RoundManager.Instance.RoundInProgress()) {
			Respawn();
		}
	}
	
	public void Respawn() {
		if(!PhotonNetwork.isMasterClient || currentPickup != null) {
			return;
		}
		
		if(pickupPrefabName==null || pickupPrefabName.Length==0) {
			Debug.LogError(gameObject.name + " has no pickupPrefabName.");
			respawnTimerRemaining = 99999;
			return;
		}
		
		currentPickup = PhotonNetwork.Instantiate(pickupPrefabName, transform.position, Quaternion.identity, 0);
		respawnTimerRemaining = respawnTimer;
	}
	
	void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere (transform.position + Vector3.up, 1);
	}
}
