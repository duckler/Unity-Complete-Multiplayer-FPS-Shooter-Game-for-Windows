using UnityEngine;
using System.Collections;

public class OptionsScreen : MonoBehaviour {
	
	public GUISkin guiSkin;
	public Font titleFont;
	public Texture2D background;
	public bool display = false;
	float mouseSensitivityX;
	float mouseSensitivityY;
	bool mouseInvert;
	bool soundEnabled;
	bool musicEnabled;
	
	CharacterInput _characterInput;
	public CharacterInput characterInput {
		set {
			_characterInput = value;
			_characterInput.mouseSensitivityX = mouseSensitivityX;
			_characterInput.mouseSensitivityY = mouseSensitivityY;
			_characterInput.mouseInvert = mouseInvert;
		}
	}
	
	void Start() {
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach(GameObject player in players) {
			_characterInput = player.GetComponent<CharacterInput>();
			if(_characterInput != null) 
				break;
		}
		
		setSound( PlayerPrefs.GetInt ("soundEnabled", 1) == 1);
		setMusic( PlayerPrefs.GetInt ("musicEnabled", 1) == 1);
		setMouseInvert( PlayerPrefs.GetInt ("mouseInvert", 0) == 1);
		
		setMouseSensitivity(
			PlayerPrefs.GetFloat("mouseSensitivityX", 5),
			PlayerPrefs.GetFloat("mouseSensitivityY", 5)
			);
		
	}
	
	void SavePrefs() {
		PlayerPrefs.SetInt ("soundEnabled", soundEnabled ? 1 : 0);
		PlayerPrefs.SetInt ("musicEnabled", musicEnabled ? 1 : 0);
		PlayerPrefs.SetInt ("mouseInvert", mouseInvert ? 1 : 0);
		PlayerPrefs.SetFloat("mouseSensitivityX", mouseSensitivityX);
		PlayerPrefs.SetFloat("mouseSensitivityY", mouseSensitivityY);
		PlayerPrefs.Save();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			display = !display;
			Screen.lockCursor = !display;
		}
	}
	
	void setMouseSensitivity(float x, float y) {
		mouseSensitivityX = x;
		mouseSensitivityY = y;
		if(_characterInput != null) {
			_characterInput.mouseSensitivityX = mouseSensitivityX;
			_characterInput.mouseSensitivityY = mouseSensitivityY;
		}
	}
		
	void setMouseInvert(bool v) {
		mouseInvert = v;
		if(_characterInput != null) {
			_characterInput.mouseInvert = mouseInvert;
		}
	}
	
	void setSound(bool v) {
		soundEnabled = v;
		AudioListener.volume = soundEnabled ? 1 : 0;
	}
	
	void setMusic(bool v) {
		musicEnabled = v;
		// FIXME: Modify the music player
	}
	
	string QualityString() {
		return QualitySettings.names[ QualitySettings.GetQualityLevel() ];
	}
	
	void OnGUI() {
		if(!display)
			return;
		
		int height = 300;
		int width = 300;
		
		Rect bg_rect = new Rect( Screen.width/2 - width/2, Screen.height/2 - height/2, width, height );
		GUI.DrawTexture(bg_rect, background);
		
		Rect rect = new Rect( Screen.width/2 - width/2 + 10, Screen.height/2 - height/2, width-20, height );
		
		GUI.skin = guiSkin;
		GUIStyle rightTitle = new GUIStyle(guiSkin.GetStyle("label"));
		rightTitle.alignment = TextAnchor.MiddleRight;
		rightTitle.font = titleFont;
		GUIStyle leftTitle = new GUIStyle(guiSkin.GetStyle("label"));
		leftTitle.font = titleFont;
		

		GUILayout.BeginArea( rect );
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Options", leftTitle);
		GUILayout.EndHorizontal();
		
			GUILayout.Space(10);
		
			GUILayout.BeginHorizontal();
				GUILayout.Label("Mouse Sensitivity X: ");
				float newX = GUILayout.HorizontalSlider(mouseSensitivityX, 1, 10);
				if(newX != mouseSensitivityX) {
					setMouseSensitivity(newX, mouseSensitivityY);
				}
			GUILayout.EndHorizontal();
		
			GUILayout.BeginHorizontal();
				GUILayout.Label("Mouse Sensitivity Y: ");
				float newY = GUILayout.HorizontalSlider(mouseSensitivityY, 1, 10);
				if(newY != mouseSensitivityY) {
					setMouseSensitivity(mouseSensitivityX, newY);
				}
			GUILayout.EndHorizontal();
		
			if(GUILayout.Button("Mouse is: " + (mouseInvert ? "Inverted" : "Normal"))) {
				setMouseInvert(!mouseInvert);
			}
		
			GUILayout.Space(10);
		
			if(GUILayout.Button("Sound is: " + (soundEnabled ? "On" : "Off"))) {
				setSound(!soundEnabled);
			}
		
			/*if(GUILayout.Button("Music is: " + (musicEnabled ? "On" : "Off"))) {
				setMusic(!musicEnabled);
			}*/
		
			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Graphic Level: " + QualityString());
				if(GUILayout.Button("More")) {
					QualitySettings.IncreaseLevel();
				}
				if(GUILayout.Button("Less")) {
					QualitySettings.DecreaseLevel();
				}
			GUILayout.EndHorizontal();
		
			GUILayout.Space(20);
		
			GUILayout.BeginHorizontal();
				if(GUILayout.Button("Return to Game")) {
					display = false;
					if(RoundManager.Instance.RoundInProgress()) {
						Screen.lockCursor=true;
					}
				}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
				if(GUILayout.Button("Disconnect / Exit")) {
					display = false;
					NetworkManager.Instance.Disconnect();
				}
			GUILayout.EndHorizontal();
		
		
		
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		
		SavePrefs();
		
	}
	
}
