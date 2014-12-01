using UnityEngine;
using System.Collections;

public class CharacterNametag : MonoBehaviour {
	
	string _name = "Dudebro";
	Transform _textTransform;
	
	new public string name {
		get { return _name; }
		set {
			_name = value;
			transform.FindChild("Nametag").GetComponent<TextMesh>().text = _name;
		}
	}
	
	void Start() {
		_textTransform = transform.FindChild("Nametag");
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 camPos = Camera.main.transform.position;
		Vector3 angles = Quaternion.FromToRotation(Vector3.back, camPos - _textTransform.position).eulerAngles;
		angles.x = 0;
		angles.z = 0;
		_textTransform.rotation = Quaternion.Euler(angles);
	}
	
	
}
