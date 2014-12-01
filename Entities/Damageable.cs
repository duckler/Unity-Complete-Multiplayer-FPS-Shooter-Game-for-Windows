using UnityEngine;
using System.Collections;

public class Damageable : Photon.MonoBehaviour {
	float _health = 100f;
	float _armor  = 50f;
	
	public AudioClip[] hurtClips;
	public AudioClip[] deathClips;
	
	public float Health {
		get { return _health; }
		set { 
			if(!photonView.isMine) {
				return;
			}
			_health = value;
			if(_health <= 0) {
				Die ();
			}
		}
	}
	public float Armor {
		get { return _armor; }
		set { 
			if(!photonView.isMine) {
				return;
			}
			_armor = value;
			UpdateArmorAlpha();
		}
	}
	
	int last_shooterPlayerID = -1;
	float damageFlashTime = 0.3f;
	float damageFlashTimeRemaining = 0;
	
	SkinnedMeshRenderer _armorMesh;

	public Texture2D texDamageFeedback;
	public Texture2D texHealthBarIcon;
	public Texture2D texArmorBarIcon;
	public Texture2D texHealthBar;
	public Texture2D texArmorBar;
	
	void Start() {
		_armorMesh = transform.Find("legobro_mesh/Armor").GetComponent<SkinnedMeshRenderer>();
		UpdateArmorAlpha();
	}
	
	void UpdateArmorAlpha() {
		if(_armorMesh==null)
			return;
		
		Material mat = _armorMesh.material;
		Color col = mat.color;
		if(_armor < 25f) {
			col.a = 0;
		}
		else {
			col.a = _armor / 100f;
		}
		mat.color = col;
		_armorMesh.material = mat;
	}
	
	[RPC]
	void TakeDamage(float d, int shooterPlayerID, string shooterName) {
		//Debug.Log ("ReceiveDamage(): " + d);
		float armorDamage = d * _armor/100f;
		_armor -= armorDamage;
		if(_armor < 0)
			_armor = 0;
		UpdateArmorAlpha();
		
		_health -= d - armorDamage;
		
		AudioSource.PlayClipAtPoint( hurtClips[Random.Range(0, hurtClips.Length)], transform.position, .2f);
		
		damageFlashTimeRemaining = damageFlashTime;
		if(_health <= 0) {
			if(photonView.isMine) {
				NetworkManager.Instance.photonView.RPC("AddChatMessage", PhotonTargets.All, shooterName + " killed " + gameObject.name + "!");
			}
			
			foreach(PhotonPlayer player in PhotonNetwork.playerList) {
				if(player.ID == shooterPlayerID) {
					Hashtable old_props = player.customProperties;
					Hashtable props = new Hashtable();
					props["Kills"] = (int)old_props["Kills"] + 1;
					player.SetCustomProperties( props );
				}
				
				if(player.ID==last_shooterPlayerID && last_shooterPlayerID!=shooterPlayerID) {
					Hashtable old_props = player.customProperties;
					Hashtable props = new Hashtable();
					props["Assists"] = (int)old_props["Assists"] + 1;
					player.SetCustomProperties( props );
				}
			}
			
			Die();
		}
		
		last_shooterPlayerID = shooterPlayerID;
	}
	
	void Die() {
		AudioSource.PlayClipAtPoint( deathClips[Random.Range (0, deathClips.Length)], transform.position );
		
		if(!photonView.isMine) {
			return;
		}
		
//		Debug.Log (gameObject.name + " has died.");
		
		NetworkManager.Instance.DestroyAndRespawn(gameObject);
		
		FXManager.Instance.photonView.RPC("DoDeathAnimation", PhotonTargets.All, transform.position);
	}
	
	void Update() {
		damageFlashTimeRemaining -= Time.deltaTime;
	}
	
	void OnGUI() {
		if(!photonView.isMine || texDamageFeedback==null)
			return;
		
		if(damageFlashTimeRemaining > 0) {
			float alpha = damageFlashTimeRemaining / damageFlashTime;
			
			Color prevColor = GUI.color;
			GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, alpha);
			GUI.DrawTexture( new Rect(0, 0, Screen.width, Screen.height), texDamageFeedback);
			GUI.color = prevColor;
		}
		
		GUI.DrawTexture( new Rect(0, Screen.height - 32, 32, 32), texHealthBarIcon);
		GUI.DrawTexture( new Rect(32, Screen.height - 32, Mathf.FloorToInt(_health), 32), texHealthBar);
		
		GUI.DrawTexture( new Rect(0, Screen.height - 64, 32, 32), texArmorBarIcon);
		GUI.DrawTexture( new Rect(32, Screen.height - 64, Mathf.FloorToInt(_armor), 32), texArmorBar);
	}
	
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(_health);
            stream.SendNext(_armor);
        }
        else
        {
            _health = (float) stream.ReceiveNext();
            _armor = (float) stream.ReceiveNext();
			UpdateArmorAlpha();
        }
    }
	
}
