using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	public float _rotationSpeed = 25f;
	public float healthGiven = 0;
	public float healthMax = 100;
	public float armorGiven  = 0;
	public float armorMax  = 50;
	
	public AudioClip clip;
	public AudioClip clip_other;
	
	void Start() {
		transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.Rotate( 0, _rotationSpeed * Time.deltaTime, 0);
	}
	
	void OnTriggerEnter(Collider collider) {
		Debug.Log ("OnTriggerEnter: " + collider.name);
		
		if(collider.tag == "Player") {
			Damageable dmg = collider.GetComponent<Damageable>();
			if( dmg.Health < healthMax ) {
				dmg.Health = Mathf.Clamp(dmg.Health+healthGiven, 0, healthMax);
			}
			if( dmg.Armor < armorMax ) {
				dmg.Armor = Mathf.Clamp(dmg.Armor+armorGiven, 0, armorMax);
			}
			
			// If this player is us (i.e. owned by us and not a bot)
			// Play normal pickup sound
			// Else, play other sound
			
			PhotonView pv = collider.GetComponent<PhotonView>();
			CharacterAI ai = collider.GetComponent<CharacterAI>();
			
			if(pv != null && pv.isMine && ai==null) {
				AudioSource.PlayClipAtPoint(clip, transform.position, 1f);
			}
			else {
				AudioSource.PlayClipAtPoint(clip_other, transform.position, 1f);
			}
			
			if(PhotonNetwork.isMasterClient) {
				PhotonNetwork.Destroy(gameObject);
			}
			
		}
				
	}
	
	/*
	void OnTriggerEnter(Collider collider) {
		Debug.Log ("OnTriggerEnter: " + collider.name);
		
		if(collider.tag == "Player") {
			PhotonView pv = GetComponent<PhotonView>();
			if(pv.isMine) {
				Damageable dmg = collider.GetComponent<Damageable>();
				if( dmg.Health < healthMax ) {
					dmg.Health = Mathf.Clamp(dmg.Health+healthGiven, 0, healthMax);
				}
				if( dmg.Armor < armorMax ) {
					dmg.Armor = Mathf.Clamp(dmg.Armor+armorGiven, 0, armorMax);
				}
				
				if(clip != null && !isBot) {
					AudioSource.PlayClipAtPoint(clip, transform.position, 1f);
				}
				else {
					PlayOtherPlayerPickupSound();
				}
				
				PhotonNetwork.Destroy(gameObject);
			}
			else {
					PlayOtherPlayerPickupSound();
			}
		}
	}
	*/


}
