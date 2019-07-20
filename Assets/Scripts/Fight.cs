using UnityEngine;
using System.Collections;

public class Fight : MonoBehaviour {

	public Player p1;
	public Player p2;

	public GameObject playerPrefab;

	public float yOffset = 2.0f;

	public float minZoom = -22.0f;
	public float maxZoom = -5.0f;

	public UIRoot uiRoot;
	public GameObject finishScreenPrefab;
	public static Fight instance;

	private float stunTime;
	private float fightEndTime;
	private bool cameraShake;

	private int p1Score;
	private int p2Score;
	private float roundDelay = 2f;
	private bool roundDelaying;
	private bool ready;

	public Material[] skins;


	// Use this for initialization
	void Start () {
		instance = this;
		//Debug.Log(p1.transform.Find("DiveKicker").Find("Model").GetComponent<SkinnedMeshRenderer>()..materials[0].name);
		updateLabels ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (roundDelaying) {
			roundDelay -= Time.deltaTime;
			//Debug.Log(roundDelay);
			if (roundDelay <= 0) {
				//Debug.Log("resetting Prefabs");
				roundDelay = 2f;
				roundDelaying = false;
				Material m1 = p1.transform.Find("DiveKicker").Find("Model").GetComponent<SkinnedMeshRenderer>().material;
				Material m2 = p2.transform.Find("DiveKicker").Find("Model").GetComponent<SkinnedMeshRenderer>().material;
				Destroy(p1.gameObject);
				Destroy(p2.gameObject);
				p1 = (Instantiate(playerPrefab) as GameObject).GetComponent<Player>();
				p2 = (Instantiate(playerPrefab) as GameObject).GetComponent<Player>();
				p1.gameObject.name = "Player1";
				p2.gameObject.name = "Player2";
				p1.transform.position += Vector3.left * 5f;
				p2.transform.position += Vector3.left * -5f;
				p1.transform.Find("DiveKicker").Find("Model").GetComponent<SkinnedMeshRenderer>().material = m1;
				p2.transform.Find("DiveKicker").Find("Model").GetComponent<SkinnedMeshRenderer>().material = m2;
			}
		}

		float posDiff = p2.transform.position.x - p1.transform.position.x;
		int facing = posDiff > 0 ? 1:-1;
		p1.faceQueue(facing);
		p2.faceQueue(-facing);
		if (p1.deathPending || p2.deathPending) {
			//Debug.Log ("stunTime");
			stunTime -= Time.deltaTime;
			Time.timeScale = stunTime < 0 ? 1f:0.03f;
			if (!cameraShake) {
				fightEndTime = Time.time;
				cameraShake = true;
			}
		} else {
			//Debug.Log ("Alive");
			Time.timeScale = 1f;
			stunTime = 0.04f;
		}
		this.UpdateCamera ();
	}

	void UpdateCamera () {
		Vector3 shakeVec = new Vector3 ();
		if (cameraShake) {
			float dt = Time.time - fightEndTime;
			float amp = (0.25f - dt);
			//Debug.Log (amp);
			if (amp < 0) {
				cameraShake = false;
				p1Score++;
				p2Score++;
				if(p1.dead) p1Score--;
				if(p2.dead) p2Score--;
				//Debug.Log (p1Score + " vs " + p2Score);
				uiRoot.transform.Find("Wins1").GetComponent<UISprite>().spriteName = "stars" + p1Score;
				uiRoot.transform.Find("Wins2").GetComponent<UISprite>().spriteName = "stars" + p2Score;
				roundDelaying = true;
				if (Mathf.Max(p1Score,p2Score) >= 5) {
					GameObject finishScreen = Instantiate(finishScreenPrefab) as GameObject;
					finishScreen.transform.parent = uiRoot.transform;
					finishScreen.transform.localScale = Vector3.one;
					finishScreen.GetComponentInChildren<UILabel>().text = getPlayerName(p2Score > p1Score ? p2:p1) +" Wins";
					roundDelay = float.NaN;
				}
			}else{
				shakeVec.x = amp * Mathf.Sin(Time.time*100f);
				shakeVec.y = amp * Mathf.Cos(Time.time*100f);
			}
		}

		float x = (p1.transform.position.x + p2.transform.position.x) / 2;
		float y = (p1.transform.position.y + p2.transform.position.y) / 2;

		float diff = Mathf.Abs(p1.transform.position.x - p2.transform.position.x);
		float z = maxZoom + ((minZoom - maxZoom) * (diff / 40.0f));

		transform.position = new Vector3(x, y + yOffset, z) + shakeVec;
	}

	void Update() {
		if (Input.GetKeyDown ("z")) {
			Control("controls1up");
		}
		
		if (Input.GetKeyDown ("x")) {
			Control("controls1down");
		}

		if (Input.GetKeyDown (".")) {
			Control("controls2up");
		}
		
		if (Input.GetKeyDown ("/")) {
			Control("controls2down");
		}
	}

	public void Control(string msg) {
		if (!ready) {
			Debug.Log("NOT READY");
				return;
		}
		string command = msg.ToLower();
		switch (command) {
		case "controls1up":
			p1.Jump();
			break;
		case "controls1down":
			p1.Dive();
			break;
		case "controls2up":
			p2.Jump();
			break;
		case "controls2down":
			p2.Dive();
			break;
		}
	}

	public void Ready() {
		ready = true;
		Destroy(uiRoot.transform.Find("Wins1").Find("ChangeChar1").gameObject);
		Destroy(uiRoot.transform.Find("Wins2").Find("ChangeChar2").gameObject);
	}

	public void changeChar(int charNum) {
		Player p;
		if (charNum == 1) {
			p = p1;
		} else {
			p = p2;
		}

		string currentName = getPlayerName(p);
		for (int i = 0; i < skins.Length; i++) {
			Material skin = skins[i];
			if (currentName == skin.name) {
				Material newSkin = skins[(i+1) % skins.Length];
				Debug.Log("nextName = " + newSkin.name);
				p.transform.Find("DiveKicker").Find("Model").GetComponent<SkinnedMeshRenderer>().material = newSkin;
				updateLabels();
				return;
			}
		}

	}

	private void updateLabels() {
		uiRoot.transform.Find("Wins1").Find("CharLabel").GetComponent<UILabel>().text = getPlayerName(p1);
		uiRoot.transform.Find("Wins2").Find("CharLabel").GetComponent<UILabel>().text = getPlayerName(p2);
	}

	private string getPlayerName(Player player) {
		string name = player.transform.Find("DiveKicker").Find("Model").GetComponent<SkinnedMeshRenderer>().materials[0].name;
		return name.Replace (" (Instance)","");
	}
}
