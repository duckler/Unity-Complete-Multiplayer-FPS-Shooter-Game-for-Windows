using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(Seeker))]
public class CharacterAI : Photon.MonoBehaviour {
	
	Transform _transform;
	Vector3 _targetPosition;
	Transform _targetEnemy;
	Seeker _seeker;
	ABPath _path;
	
	CharacterMotor _characterMotor;

	int _waypointIndex = 0;
	int _numWaypoints = 0;
	
	bool _isSearching = false;
	
	float waypointMinDistance = .5f;
	float targetMinDistance = 5f;
	
	public float shootInnacuracy = 2f;
	
	// Use this for initialization
	void Start () {
		_transform = transform;
		_targetPosition = _transform.position;
		_seeker = GetComponent<Seeker>();
		_seeker.pathCallback += OnPathComplete;
		_characterMotor = GetComponent<CharacterMotor>();
		_targetPosition = _transform.position;
	}
	
	Vector3 GetRandomPosition() {
		GameObject[] gos = GameObject.FindGameObjectsWithTag("AI_Waypoint");
		return gos[Random.Range(0, gos.Length-1)].transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if(!photonView.isMine) {
			return;
		}
		
		if(!_isSearching) {
			UpdateTargetEnemy();
			
			if( _targetEnemy==null && Vector3.Distance(_transform.position, _targetPosition) < waypointMinDistance ) {
				_targetPosition = GetRandomPosition();
			}
			
			if(Vector3.Distance( _transform.position , _targetPosition  ) > waypointMinDistance ) {
				_isSearching = true;
				_seeker.StartPath( _transform.position , _targetPosition);
			}
		}

		ShootGun();
	}
	
	void FixedUpdate() {
		if(!photonView.isMine) {
			return;
		}
		
		// If we're in shooting range of a target, stop so we don't headbutt them...
		if(_targetEnemy!= null && Vector3.Distance(_targetEnemy.position, _transform.position) < targetMinDistance) {
			_characterMotor.runDir = Vector3.zero;
		}
		else {
			_characterMotor.runDir = GetNewWaypointDirection();
		}
		
		Vector3 euler = Quaternion.FromToRotation(Vector3.forward,  _targetPosition - _transform.position ).eulerAngles;
		_transform.rotation = Quaternion.Euler( 0, euler.y, 0);
	}
	
	// Find the closest visible enemy.
	void UpdateTargetEnemy() {
		Transform closest = null;
		float dist=0;
		
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach(GameObject player in players) {
			if(
				player.transform && 
				( closest == null || Vector3.Distance(player.transform.position, _transform.position) < dist)
				) {
				Vector3 dir = player.transform.position - _transform.position;
				Ray ray = new Ray(_transform.position + _characterMotor.gunOffset, dir);
				RaycastHit hitInfo;
				if( Physics.Raycast( ray, out hitInfo ) ) {
					if(hitInfo.transform == player.transform) {
						closest = player.transform;
						dist = Vector3.Distance(player.transform.position, _transform.position);
					}
				}
			}
		}
		
		if(closest != null) {
			_targetEnemy = closest;
			_targetPosition = _targetEnemy.position;
		}
		else {
			_targetEnemy = null;
		}
	}
	
	void OnPathComplete(Path p) {
		_path = (ABPath)p;
		_path.Claim(this);
		_numWaypoints = _path.vectorPath.Count;
		_isSearching = false;
		_waypointIndex = 1;
	}
	
	Vector3 GetNewWaypointDirection() {
		if(_path == null || _waypointIndex >= _numWaypoints) {
			return Vector3.zero;
		}
		
		List<Vector3> waypoints = _path.vectorPath;
		
		Vector3 runDir = waypoints[_waypointIndex] - _transform.position;
		if(runDir.magnitude < waypointMinDistance) {
			_waypointIndex++;
		}
		
		//Debug.Log ("RunDir: " + runDir);
		
		return runDir;
	}
	
	void ShootGun() {
		if(_targetEnemy == null || _characterMotor.shootCooldownRemaining > 0 ) {
			return;
		}
		//Debug.Log (gameObject.name + " is shooting.");
		Vector3 orig = _transform.position + _characterMotor.gunOffset;
		Vector3 dir = _targetEnemy.collider.bounds.center - orig;
		
		dir = Quaternion.Euler(0, Random.Range (-shootInnacuracy, shootInnacuracy), 0) * dir;
		
		_characterMotor.ShootGun(orig, dir);
	}
	
}
