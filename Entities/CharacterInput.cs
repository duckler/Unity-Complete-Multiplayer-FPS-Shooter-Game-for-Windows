using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
public class CharacterInput : MonoBehaviour {
	CharacterMotor _characterMotor;
	
	public float mouseSensitivityX = 5f;
	public float mouseSensitivityY = 5f;
	public bool mouseInvert = false;
	public Camera myCamera;
	
	float _vertRotation = 0;
	float _vertMaxAngle = 45;
	
	void Start () {
		// Cache stuff
		_characterMotor = GetComponent<CharacterMotor>();
		GameObject.Find ("OptionsScreen").GetComponent<OptionsScreen>().characterInput = this;
	}
	
	void Update () {
		Vector3 runDir = Vector3.zero;
		
		// Rotate Character
		transform.Rotate( 0, Input.GetAxis("Mouse X") * mouseSensitivityX, 0);
		
		_vertRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY * (mouseInvert ? -1 : 1);
		
		_vertRotation = Mathf.Clamp(_vertRotation, -_vertMaxAngle, _vertMaxAngle);
		
		myCamera.transform.localRotation = Quaternion.Euler( -_vertRotation, transform.rotation.eulerAngles.y, 0 );
		myCamera.transform.position = transform.position + Vector3.up * 1.8f;
		//Debug.Log (myCamera.transform.rotation.eulerAngles);
		// Move Character
		
		
		runDir += transform.forward * Input.GetAxis("Vertical");// * speed;
		runDir += transform.right * Input.GetAxis("Horizontal");// * speed;
		
		//velocity = Vector3.ClampMagnitude( velocity, speed );
		
		if( Input.GetButton("Jump") ) {
			_characterMotor.jumping = true;
		}
		else {
			_characterMotor.jumping = false;
		}
		
		_characterMotor.runDir = runDir;
		
		// PEW PEW
		if(Input.GetMouseButton(0)) {
			ShootGun();
		}
	}
	
	void ShootGun() {
//		Debug.Log ("ShootGun()");
		if( _characterMotor.shootCooldownRemaining > 0) {
			return;
		}
		
		_characterMotor.ShootGun(myCamera.transform.position + myCamera.transform.forward, myCamera.transform.forward);
	}
}
