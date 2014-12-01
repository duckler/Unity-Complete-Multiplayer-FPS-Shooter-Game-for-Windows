using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : Photon.MonoBehaviour {
	
	string _versionString = "ld26 1.0.0";
	
	public bool botEnabled = true;
	int numBots = 0;
	
	public Texture2D menu_background;
	public Texture2D menu_instructions;
	public GUISkin guiSkin;
	public Font titleFont;
	
	float deathDuration = 5f;
	
	int numPlayersAllowed = 16;	// Note: We might go over this number temporarily with bots.
	
	string _userName;
	
	public string playerPrefabName;
	public string botPrefabName;
	
	public GameObject crosshairPrefab;
	
	public Camera myCamera;
	
	Scoreboard _scoreboard;
	
	List<string> _chatMessages;
	float _chatMessageMax = 5;
	float _chatMessageDisplayTime = 2f;
	float _chatMessageDisplayRemaining = 0;

	// If we are a late joiner to the game, we have to wait one frame before
	// we know if a round is in progress.
	bool _checkRoundInProgress = false;
	
	//public List<PlayerData> players;
	
	bool _connecting = false;
	
	List<SpawnPoint> spawnPoints;
	
	Vector3 _cameraPos;
	Quaternion _cameraRot;
	
	static NetworkManager _instance;
	static public NetworkManager Instance {
		get { 
			if(_instance==null) {
				_instance = (NetworkManager)FindObjectOfType(typeof(NetworkManager));
			}
			return _instance;
		}
	}

	// Use this for initialization
	void Start () {
		_instance = this;
		_userName = PlayerPrefs.GetString("userName", "Awesome Person");	
		_scoreboard = GameObject.Find ("Scoreboard").GetComponent<Scoreboard>();
		_chatMessages = new List<string>();
		
		_cameraPos = myCamera.transform.position;
		_cameraRot = myCamera.transform.rotation;
		//players = new List<PlayerData>();
	}

	void OnDestroy() {
		PlayerPrefs.SetString("userName", _userName);
	}
	
	// Update is called once per frame
	OptionsScreen _optionsScreen;
	void Update () {
		
		if(_optionsScreen == null) {
			_optionsScreen = GameObject.Find ("OptionsScreen").GetComponent<OptionsScreen>();
		}
		
		if(Input.GetMouseButtonDown(0) && PhotonNetwork.connected && !Screen.lockCursor && !_optionsScreen.display) {
			Screen.lockCursor = true;
		}
		
		if(_checkRoundInProgress && RoundManager.Instance.RoundInProgress()) {
			_checkRoundInProgress = false;
			SpawnCharacters();
		}
		_chatMessageDisplayRemaining -= Time.deltaTime;
		
		/*if(!PhotonNetwork.connected && !_connecting) {
			ConnectSingle();	///////////////////////////////////
		}*/		
	}
	
	public void RegisterSpawnPoint(SpawnPoint sp) {
		if(spawnPoints == null) {
			spawnPoints = new List<SpawnPoint>();
		}
		spawnPoints.Add(sp);
	}
	
	void OnGUI() {
		if(!PhotonNetwork.connected && !_connecting) {
			int width=400;
			int height=500;

			GUIStyle leftTitle = new GUIStyle(guiSkin.GetStyle("label"));
			leftTitle.font = titleFont;
			
			
			Rect bg_rect = new Rect( Screen.width/2 - width/2, Screen.height/2 - height/2, width, height );
			GUI.DrawTexture(bg_rect, menu_background);
			
			Rect instr_rect = new Rect( Screen.width/2 - width/2, Screen.height/2 + height/2-254, width, 254 );
			GUI.DrawTexture(instr_rect, menu_instructions);
			
			Rect rect = new Rect( Screen.width/2 - width/2 + 10, Screen.height/2 - height/2 +40, width-20, height-40 );
			
			GUILayout.BeginArea( rect );
			GUILayout.BeginVertical();
			
			/*GUILayout.BeginHorizontal();
			GUILayout.Label("Shoot.", leftTitle);													/////////////////////////////////////////////////////
			GUILayout.EndHorizontal();*/
		
			GUILayout.Space(10);
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Name: ");
			_userName = GUILayout.TextField(_userName, 24);
			if(_userName.Length > 20) {
				_userName = _userName.Substring(0, 20);
			}
			PhotonNetwork.playerName = _userName;
			GUILayout.EndHorizontal();
			
			GUILayout.Space(20);
			if(_userName.Length > 0 && GUILayout.Button("Multiplayer!")) {
				ConnectMulti();
			}
			
			GUILayout.Space(20);
			if(_userName.Length > 0 && GUILayout.Button("Offline vs Bots")) {
				ConnectSingle();
			}
			
			GUILayout.Space(20);
			if(_userName.Length > 0 && GUILayout.Button("Options")) {
				_optionsScreen.display = true;
			}
			
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		else if (_connecting) {
			GUILayout.BeginArea( new Rect(Screen.width/2 - 100, 0, 200, Screen.height) );
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();
			GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString());
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		else {
			if(_chatMessageDisplayRemaining > 0) {
				Color old_color = GUI.color;
				float alpha = 1f;
				if(_chatMessageDisplayRemaining < 1f) {
					alpha = _chatMessageDisplayRemaining;
				}
				Color c = new Color(old_color.r, old_color.g, old_color.b, alpha);
				GUI.color = c;
				GUILayout.BeginArea( new Rect(Screen.width - 200, 0, 200, Screen.height) );
				GUILayout.BeginVertical();
				
				foreach(string s in _chatMessages) {
					GUILayout.Label(s);
				}
				
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndArea();
				GUI.color = old_color;
			}
			else {
				_chatMessages.Clear();
			}
			
		}
	}
	
	[RPC]
	void AddChatMessage(string s) {
		_chatMessages.Add(s);
		_chatMessageDisplayRemaining = _chatMessageDisplayTime;
		while(_chatMessages.Count > _chatMessageMax) {
			_chatMessages.RemoveAt(0);
		}
	}
	
	void ConnectMulti() {
		Debug.Log ("ConnectMulti()");
		PhotonNetwork.ConnectUsingSettings( _versionString );
		_connecting = true;
		RoundManager.Instance.roundCountdown = 10f;
	}
	
	void ConnectSingle() {
		PhotonNetwork.offlineMode = true;
		PhotonNetwork.CreateRoom(null);
		RoundManager.Instance.roundCountdown = 4f;
	}
	
	void OnJoinedLobby() {
		Debug.Log ("OnJoinedLobby()");
		PhotonNetwork.JoinRandomRoom();
	}
	
	void OnPhotonRandomJoinFailed() {
		Debug.Log ("OnPhotonRandomJoinFailed()");
		PhotonNetwork.CreateRoom(null, true, true, numPlayersAllowed);
	}
	
	void OnJoinedRoom() {
		Debug.Log ("OnJoinedRoom()");
		
		_connecting = false;
		Screen.lockCursor = true;
		_scoreboard.display = true;
		
		ResetScores();
		
		photonView.RPC ("AddChatMessage", PhotonTargets.All, PhotonNetwork.player.name + " has joined the game!");
		
		if(PhotonNetwork.isMasterClient) {
			//RoundManager.Instance.photonView.RPC("NewRound", PhotonTargets.All);\
			RoundManager.Instance.NewRound();
		}
		else {
			_checkRoundInProgress = true;
		}
	}
	
	void ResetScores() {
		Hashtable props = new Hashtable();
		props.Add("Kills", 0);
		props.Add("Deaths", 0);
		props.Add("Assists", 0);
		PhotonNetwork.player.SetCustomProperties( props );
	}
	
	void SpawnBots() {
		if(!botEnabled || !PhotonNetwork.isMasterClient) {
			return;
		}
		
		while( PhotonNetwork.playerList.Length + numBots < numPlayersAllowed ) {
			SpawnBot();
		}
	}
	
	void OnPhotonPlayerDisconnected(PhotonPlayer p) {
		if(PhotonNetwork.isMasterClient) {
			photonView.RPC ("AddChatMessage", PhotonTargets.All, p.name + " has left.");
		}
		SpawnBots ();
	}
	
	void SpawnPlayer() {
		Debug.Log ("SpawnPlayer");
		
		// Create the player
		Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
		GameObject myPlayer = (GameObject)PhotonNetwork.Instantiate( playerPrefabName, GetSpawnLocation(), rot, 0 );
		//myPlayer.GetComponent<CharacterNametag>().name = PhotonNetwork.playerName;
		myPlayer.GetComponent<CharacterMotor>().photonView.RPC ("SetNameTag",PhotonTargets.AllBuffered,PhotonNetwork.playerName);
		
		// Enable local Character inputs
		CharacterInput ci = myPlayer.GetComponent<CharacterInput>();
		ci.enabled = true;
		ci.myCamera = myCamera;
		//myCamera.GetComponent<FPSCameraFollow>().target = myPlayer.transform;
		
		//ci.myCamera.transform.parent = myPlayer.transform;
		//ci.myCamera.transform.localPosition = new Vector3(0, 1.8f, 0);
		//ci.myCamera.transform.rotation = Quaternion.identity;
		
		Instantiate(crosshairPrefab);
		
		// Disable local character mesh renderers
		MeshRenderer[] mrs = myPlayer.GetComponentsInChildren<MeshRenderer>();
		
		foreach(MeshRenderer mr in mrs) {
			mr.enabled = false;
		}
		
		SkinnedMeshRenderer[] smrs = myPlayer.GetComponentsInChildren<SkinnedMeshRenderer>();
		
		foreach(SkinnedMeshRenderer smr in smrs) {
			smr.enabled = false;
		}
		_scoreboard.display = false;		
	}
	
	void SpawnBot() {
		//Debug.Log ("SpawnBot");
		Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
		GameObject bot = (GameObject)PhotonNetwork.Instantiate( botPrefabName, GetSpawnLocation(), rot, 0 );
		bot.GetComponent<CharacterMotor>().photonView.RPC ("SetNameTag",PhotonTargets.AllBuffered, RandomBotName());

		numBots++;
	}
	
	Vector3 GetSpawnLocation() {
		SpawnPoint sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
		while(!sp.IsClear()) {
			sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
		}
		Vector3 pos = sp.transform.position;
		return pos;
	}
	
	
	public void DestroyAndRespawn(GameObject go) {
		// Only gets called by the owner the character (player or bot)
		//Debug.Log("DestroyAndRespawn");
		CharacterAI ai = go.GetComponent<CharacterAI>();
		PhotonNetwork.Destroy(go);
		
		if(ai!=null) {
			numBots--;
			SpawnBots();
		}
		else {
			Hashtable old_props = PhotonNetwork.player.customProperties;
			Hashtable props = new Hashtable();
			props["Deaths"] = (int)old_props["Deaths"] + 1;
			PhotonNetwork.player.SetCustomProperties( props );

			ResetCameraPosition();
			StopCoroutine("DelayedPlayerSpawn");
			StartCoroutine("DelayedPlayerSpawn", deathDuration );
		}
	}
	
	void ResetCameraPosition() {
		myCamera.transform.position = _cameraPos;
		myCamera.transform.rotation = _cameraRot;
	}
	
	// This is called after player death, while waiting for respawn
	IEnumerator DelayedPlayerSpawn(float delay) {
		_scoreboard.display = true;
		RoundManager.Instance.deathTimeRemaining = delay;
		//yield return new WaitForSeconds(delay);
		while(RoundManager.Instance.deathTimeRemaining > 0) {
			yield return null;	// wait one frame
		}
		SpawnPlayer();
	}
	
	[RPC]	// All Clients
	public void StartOfRound() {
		_checkRoundInProgress = false;
		
		// Hide scoreboard
		_scoreboard.display=false;
		// Reset Scores
		ResetScores();
		// Spawn characters
		SpawnCharacters();
	}
	
	[RPC]	// All Clients
	public void EndOfRound() {
		PhotonNetwork.DestroyPlayerObjects( PhotonNetwork.player );
		CleanUpRound();	
		
		if(PhotonNetwork.isMasterClient) {
			/*GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
			foreach(GameObject go in gos) {
				
				PhotonNetwork.Destroy(go);
			}*/
			
			if(PhotonNetwork.offlineMode) {
				RoundManager.Instance.roundCountdown = 10f;
			}
			RoundManager.Instance.NewRound();
		}
		
		// Tell the scoreboard to display
		_scoreboard.display=true;
	}
	
	void CleanUpRound() {
		StopCoroutine("DelayedPlayerSpawn");
		RoundManager.Instance.CleanUpRound();
		ResetCameraPosition();
		numBots = 0;
	}
	
	[RPC]
	public void SpawnCharacters() {
		SpawnPlayer();
		SpawnBots();
	}
	
	public void Disconnect() {
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.Disconnect();
		PhotonNetwork.offlineMode = false;
		CleanUpRound();
	}

	/*
	string[] bot_names = {
"YOLO",
"MelonHead",
"Derek",
"Foxxy",
"Alax",
"Jansen24",
"Jonas",
"Shazbot!",
"Quillbot",
"SWAG",
"Quillbots",
"MrGreen",
"Koridian",
"Smiley",
"Brussels",
"Quill18",
"Nokah",
"WooferZ",
"Quilliam",
"Gun Bro",
"Sam the wham",
"Zak West",
"Gilbert's Minion",
"Potato",
"VikingYaffa",
"Evilnator",
"BubblegumBalloon",
"brussels",
"MattSRodine",
"French_Brussels",
"Latenz",
"Brussels",
"xX420SWAGXx",
"CyntaxX",
"Hello",
"Solace",
"JiggleWiggle",
"Nicolas Cage",
"Voldemort",
"Quillabots",
"Inflicted Chaos",
"Enter a name!",
"Kees Nijenhuis",
"Evilnator",
"Kris",
"murkyshadow",
"Derrenger Donny",
"Quiller 1",
"Kim-Jong-Un",
"DoomGuy",
"DaMonkey",
"Wizardboy97",
"Zak West",
"P-Body",
"Sancho",
"HAL 9001",
"Dennis",
"Cyberman",
"Nox",
"Sofaguy",
"Epic Gun Bro",
"Heart",
"Virainix",
"Francis",
"Gerald",
"Trinity",
"Addy",
"John Smith",
"Lord Quillington",
"Wantuhy",
"Dalek Emperor",
"7_yo_girl",
"TheZenGarden",
"Katia_Managan",
"Teddbudd4",
"Laura Bow",
"AAA_FPS_GOTY_1",
"Call of Duty",
"Lara Kroft",
"Lindsay Lohan",
"Bernard",
"Essentia",
"Neo",
"BroBot",
"Alpha Unit",
"Katzilla jr.",
"Cucumber",
"Johnny",
"Talos",
"Derringer Donny",
"HAL 9001",
"Spuds",
"Minimal Massacre",
"QuillBot",
"terminator",
"Me",
"Rhys Jhonnson",
"McLovin",
"Call of Duty",
"XxX1337807XxX",
"justin bieber",
"Sir_Tomato",
"Golem",
"Smitty",
"El Presidente",
"grey hawk",
"MichelinGubben",
"kallgren",
"Atlas",
"Qill Me",
"Christoph",
"The Dictator",
"Gort",
"Muhammed Chang",
"Jack Bauer",
"botman",
"Gummy Worm XI",
"LudumDare",
"Batman",
"Big Tuna",
"xX_N00B_SL4Y3R_Xx",
"Clements",
"Awesome Robot",
"Quincy",
"Brussels",
"Jack",
"LadyGugu",
"Bob",
"Lord Quillinator",
"Quil-a-nator",
"Dr_Pepper",
"h4xor",
"[ *~* ]",
"gandhi",
"Geoffrey",
"Dummies",
"Penultimo",
"Farley",
"Battlefield",
"Tik-Tok",
"Chatterbot",
"Bob",
"Shane",
"saddam hussein",
"Gunbro",
"3.1415926535",
"Essentia-a-nator",
"i'M a bOT",
"wireless [NO SOUND]",
"Foster",
"QuillsMinion",
"Jimbob jr",
"Joffrey",
"Stephen Hawking",
"QuillBot",
"Welsh_Sydney",
"BotyMigu",
"Medal of Honor",
"I <3 Eggplant",
"Marty McFly",
"Maximum Minimalism",
"Just Cause",
"I'm Botman",
"American_Toronto",
"Groan III",
"Zena",
"McBot",
"420YOLOSWAG Patrol",
"Marius",
"Mattc212",
"Quillbot 1",
"Tobias",
"Vanilla Ice 1337",
"Helena",
"Sniper Sammy",
"SlutDrop",
"Gilbert Bottfried",
"Darcy",
"Aspire",
"MelonHeadMonStar",
"BOTZX",
"Euro Pauni",
"Radius",
"Gamemaster3",
"Viking of Sweden",
"Buster",
"C3-PO",
"Sulla",
"TheOther",
"Khak Aman",
"Jan2442",
"#YoloSwag",
"xXxXx-Sephiroth-xXxXx",
"[pod]quill",
"Hitler",
"R2-D2",
"AAA_FPS_GOTY Dummy",
"Hitler",
"halflifethegame",
"Lucille",
"Spin Bot",
"Aboba992",
"God Bot",
"Evilsanta",
"Lindsay",
"Bot Romance",
"Gob",
"Reginald",
"BB Bieber",
"God",
"liosif stalin",
"Notch",
"Britany Spears",
"Michael",
"Bad guy",
"Jason Bourne",
"Forever last Bot",
"Ninja",
"Mother Russia",
"Michael Moorebot",
"Brussels",
"Chuck Norris",
"Botter Noob",
"Casval Deikun",
"Kill Me",
"Brothen",
"T-0.1",
"Crusa Derki",
"Thunderfish",
"Imposter Quill18",
"Potato",
"Jack D. Wright",
"Quill18 - bot",
"LadyGaga",
"Llama Buggy",
"Ydido",
"Margarhita",
"Quill18 - bot",
"Ulysses",
"Adam Link",
"BajanCanadian",
"Nuclear Dan",
"Carbon",
"R2D2",
"qill19",
"Quill18",
"Joe Wheat",
"Heartso Firon",
" Chuck Norris ",
"Xo-V5",
"PorygonMMS",
"LudumDare2013",
"T-H",
"Quill18 bot",
"CyberStalin",
"Long John McSilver",
"Luke Botwalker",
"Blackwell",
"Desmond",
"Victoria Tucson",
"ZwRawR",
"The Bottator",
"Sid Meier",
"Photojack",
"Bot",
"Chewbottca",
"Voldemort",
"Margaret Thatcher",
"Autoaim Andy",
"Terminator",
"Mathew",
"Gnut",
"R2DBot",
"Captain Everywhere",
"not an enemy",
"Bannana Hunter",
"Josiah",
"Daneel",
"Leia Botganna",
"not an ally",
"Giskard",
"Norby",
"quilled-by-a-bot",
"Artificial Intelligence",
"Bot1",
"Jenkins",
"Hacken Ze Deutsch",
"Smiley Boloney",
"Blue Footed Booby",
"Bot2",
"SymplBot",
"Jesus",
"Quill Johnson",
"Sir Shootalot",
"Mr. FeelGood",
"Bert Higgins",
"I Am Joe",
"Fegelein ",
"ObamaChildCare",
"This is a bad name",
"Sir Derpz",
"Darth Vader",
"Grumpy Cat",
"CallingAllBots",
"Dick Chainy",
"Willem Alexander",
"miyamoto",
"Peter Botyneux",
"Chani",
"Penis Man",
"Terry Grayson",
"Tobor",
"I'm bad at this game.",
"wtfbbqpwnin",
"Bond. James Bond.",
"Patrick Stewart",
"Ezreal",
"Loque",
"Mecha-Llama",
"DoYouDareLudum",
"James Bond",
"Robert",
"2cool4school",
"Yo Mamma",
"3.1415",
"Batman",
"Xx1337B07xXx1337xX",
"Ezreal",
"spiderman",
"Pew!",
"BUT WHO WAS PHONE",
"superman",
"Basically a Bot",
"dummy",
"Don't shoot me!",
"bob",
"Link",
"Khaaaan",
"Bertrand",
"Frank",
"Tim",
"Shepard",
"Data",
"Arnold Schwarzenegger",
"George",
"####",
"Xx420BlazeDatKushxX ",
"thief",
"Bob",
"Bane Of Trevor",
"Xx420BlazeDatKushxX ",
"sniper",
"Glados",
"Net",
"Leonhard Euler",
"McButler",
"Willard",
"Nostradomus",
"terminator",
"GladOs",
"Xx420BlazeDatKushxX",
"Cybermen",
	};
*/	
	string RandomBotName() {
		return "Bot " + Random.Range(100, 999).ToString();
//		return bot_names[Random.Range(0, bot_names.Length)];
	}
}
