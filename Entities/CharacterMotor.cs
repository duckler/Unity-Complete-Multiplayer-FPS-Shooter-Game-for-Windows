using UnityEngine;
using System.Collections;

public class CharacterMotor : Photon.MonoBehaviour {
	
	public float speed = 10;
	public float jumpSpeed = 20;
	public Vector3 runDir = Vector3.zero;
	public bool jumping = false;
	Vector3 velocity = Vector3.zero;
	CharacterController _characterController;
	CharacterAI _ai;
	
	public float shootCooldown = 0.1f;
	public float shootCooldownRemaining = 0f;
	PhotonView _beamOriginPV;
	
	bool _firstPositionUpdate = true;
	Vector3 _realPosition;

	public float shotDamage = 20f;
		
	public Vector3 gunOffset = new Vector3(0f, 1.2f, 0f); // How far off the ground is our gun (for raycasting)
	
	void Start() {
		_realPosition = transform.position;
		_characterController = GetComponent<CharacterController>();
		_ai = GetComponent<CharacterAI>();
	}
	
	void Update() {
		shootCooldownRemaining -= Time.deltaTime;
	}
	
	void FixedUpdate () {
		PickAnimation();
		
		if(!photonView.isMine) {
			InterpolatedFixedUpdate();
			return;
		}
			
		float vertVel = velocity.y;
		velocity = runDir * speed;
		velocity = Vector3.ClampMagnitude(velocity, speed);
		velocity.y = vertVel;
		
		if(_characterController.isGrounded) {
			if(jumping) {
				velocity.y = jumpSpeed;
			}
			else {
				velocity.y = 0;
			}
		}
		else {
			velocity.y += Physics.gravity.y * Time.deltaTime;
		}
		
		_characterController.Move( velocity * Time.deltaTime );
	}
	
	
	Transform _meshTransform;
	void PickAnimation() {
		if(_meshTransform == null) {
			_meshTransform = transform.FindChild("legobro_mesh");
		}
		
		if( Mathf.Abs(velocity.y) > .5f ) {
			_meshTransform.animation.CrossFade("_jump");
		}
		else if(velocity.magnitude > 1f) {
			_meshTransform.animation.CrossFade("_run");
		}
		else {
			_meshTransform.animation.CrossFade("_idle");
		}
	}
	
	void InterpolatedFixedUpdate() {
		transform.position = Vector3.Lerp( transform.position, _realPosition, .1f );
	}
	
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(velocity);
        }
        else
        {
            // Network player, receive data
			transform.position = _realPosition;
            _realPosition = (Vector3) stream.ReceiveNext();
            transform.rotation = (Quaternion) stream.ReceiveNext();
            velocity = (Vector3) stream.ReceiveNext();
			
			if(_firstPositionUpdate) {
				_firstPositionUpdate = false;
				transform.position = _realPosition;
			}
        }
    }
	
	public void ShootGun(Vector3 orig, Vector3 dir) {
		Ray ray = new Ray( orig, dir );
		RaycastHit hitInfo = new RaycastHit();
		if( Physics.Raycast(ray, out hitInfo) ) {
			Damageable dam = hitInfo.collider.GetComponent<Damageable>();
			PhotonView pv = hitInfo.collider.GetComponent<PhotonView>();
			if( dam != null && pv != null ) {
				int player_id = PhotonNetwork.player.ID;
				if(_ai != null ) {
					player_id = -999;
				}
				pv.RPC("TakeDamage",PhotonTargets.All, shotDamage, player_id, gameObject.name);
			}
			
			//FXManager.Instance.photonView.RPC("Beam", PhotonTargets.All, photonView, hitInfo.point, hitInfo.normal);
			if(_beamOriginPV==null)
				_beamOriginPV = transform.FindChild("BeamOrigin").GetComponent<PhotonView>();
			
			_beamOriginPV.RPC("Beam", PhotonTargets.All, hitInfo.point, hitInfo.normal);
			//FXManager.Instance.photonView.RPC("Beam", PhotonTargets.All, photonView, hitInfo.point, hitInfo.normal);
			shootCooldownRemaining = shootCooldown;
		}
	}
	
	[RPC]
	public void SetNameTag(string n) {
		GetComponent<CharacterNametag>().name = n;
		gameObject.name = n;
	}
	
}
