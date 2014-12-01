using UnityEngine;
using System.Collections;

// join my blog > http://bit.ly/unity3diy

public class RoundManager : Photon.MonoBehaviour {
	
	float roundDuration = 5f*60f;
	public float roundDurationLeft = -1;
	public float roundCountdown = 10f;
	public float roundCountdownLeft = -1;
	public float deathTimeRemaining = -1;
	
	static RoundManager _instance;
	static public RoundManager Instance {
		get {
			if(_instance == null) {
				_instance = (RoundManager)GameObject.FindObjectOfType(typeof(RoundManager));
			}
			return _instance;
		}
	}
	
	bool roundHasStarted=false;
	
	public void CleanUpRound() {
		deathTimeRemaining = -1;
		roundDurationLeft = -1;
		roundCountdownLeft = -1;
		HideTimer();
	}
	
	//[RPC]
	public void NewRound() {
		// Run directly by master
		roundDurationLeft = roundDuration;
		roundCountdownLeft = roundCountdown;
		roundHasStarted = false;
		
		PickupSpawner[] pss = (PickupSpawner[])GameObject.FindObjectsOfType(typeof(PickupSpawner));
		foreach(PickupSpawner ps in pss) {
			ps.Respawn();
		}
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		// Update Death clock
		deathTimeRemaining -= Time.deltaTime;
		
		// Only update other clocks if master client
		if(PhotonNetwork.isMasterClient) {
			roundCountdownLeft -= Time.deltaTime;
			if(roundCountdownLeft <= 0) {
				if(!roundHasStarted) {
					roundHasStarted = true;
					NetworkManager.Instance.photonView.RPC("StartOfRound", PhotonTargets.All);
				}
				roundDurationLeft -= Time.deltaTime;
				if(roundDurationLeft <= 0) {
					NetworkManager.Instance.photonView.RPC("EndOfRound", PhotonTargets.All);
				}
			}
		}
		
		if(roundCountdownLeft > 0) {
			ShowTimer("Next round in:", roundCountdownLeft);
		}
		else if(roundDurationLeft > 0 && roundDurationLeft <= 5) {
			ShowTimer("Round ends:", roundDurationLeft);
		}
		else if(deathTimeRemaining > 0) {
			ShowTimer("Respawn in:", deathTimeRemaining);
		}
		else {
			HideTimer();
		}
	}
	
	public bool RoundInProgress() {
		/*if( roundCountdownLeft <= 0 && roundDurationLeft >= 0 ) {
			return true;
		}
		
		return false;*/
		
		return roundHasStarted;
	}
	
	float last_timer_time = 999;
	public void ShowTimer(string title, float amt) {
		if(last_timer_time > 3.1 && amt <= 3.1) {
			audio.Play();
		}
		
		last_timer_time = amt;
		foreach(GUIText t in GetComponentsInChildren<GUIText>()) {
			t.enabled = true;
			if(t.name.IndexOf("title") > 0) {
				t.text = title;
			}
			else {
				t.text = amt.ToString("F2");
			}
		}
	}
	
	public void HideTimer() {
		foreach(GUIText t in GetComponentsInChildren<GUIText>()) {
			t.enabled = false;
		}
		
		last_timer_time = 999;
	}
	
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(roundDurationLeft);
            stream.SendNext(roundCountdownLeft);
            stream.SendNext(roundHasStarted);
        }
        else
        {
            // Network player, receive data
            roundDurationLeft = (float) stream.ReceiveNext();
            roundCountdownLeft = (float) stream.ReceiveNext();
            roundHasStarted = (bool) stream.ReceiveNext();
		}
    }
	
}


// join my blog > http://bit.ly/unity3diy