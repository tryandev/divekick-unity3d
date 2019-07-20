using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public float gravity = 5;
	public float diveSpeed = 15;
	public float jumpPower = 20;
	public float stageSize = 15;
	public AudioClip hitSound;
	public AudioClip diveSound;
	public AudioClip jumpSound;

	

	private bool jumpPressed;
	private bool divePressed;
	private int newDir;
	private int curDir;
	[HideInInspector]
	public bool dead;

	[HideInInspector]
	public bool deathPending;
	private bool diving;
	private float deathVelocity;
	private bool disabled;
	private ParticleSystem ps;
	// Use this for initialization
	void Start () {
		ps = GetComponentInChildren<ParticleSystem>();	
		ps.Simulate(300000f);
	}
	
	// Update is called once per frame
	void Update () {
//		Transform label = transform.Find ("Text");
//		label.LookAt (Camera.main.transform);
//		label.RotateAround (label.position, Vector3.up, 180f);
	}

	public void FixedUpdate() {
		Vector2 pos = transform.position;
		if (dead) {	
			deathPending = false;
			return;
		}
		if (deathPending) {
			Die ();
			return;
		}
		if(isGrounded()) {
			if (diving) {			
				ps.Emit(10);
				diving = false;
			}

			Vector3 myPos = transform.position;
			myPos.y = 0.019f;

			int dir = doFacing();
			GetComponent<Rigidbody>().useGravity = true;
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			GetComponentInChildren<Animation>().Play("Idle");
			transform.forward = new Vector3(0f, 0f, curDir);
			transform.position = myPos;

			Vector3 myScale = transform.localScale;
			myScale.z = curDir;
			transform.localScale = myScale;


			if (!dead && !disabled) {
				if (jumpPressed) {
					diving = false;
					rigidbody.velocity = new Vector3(0,jumpPower,0);
					GetComponentInChildren<Animation>().Play("Jump");
					//Debug.Log("Velocity: " + this.GetComponent<Rigidbody>().velocity);
					GetComponent<AudioSource>().clip = jumpSound;
					GetComponent<AudioSource>().Play ();
					ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
					ps.Emit(10);
				} else if(divePressed) {
					diving = false;
					rigidbody.velocity = new Vector3(-jumpPower/2f * dir,jumpPower/2f,0);
					GetComponentInChildren<Animation>().Play("Jump");
					GetComponent<AudioSource>().clip = jumpSound;
					GetComponent<AudioSource>().Play ();
					ps.Emit(10);
				}
			}
		} else if(divePressed && !dead && !disabled) {
			GetComponentInChildren<Animation>().Play("Dive");
			divePressed = false;
			rigidbody.useGravity = false;
			rigidbody.velocity = new Vector3(diveSpeed * curDir,-diveSpeed,0);
			diving = true;
			GetComponent<AudioSource>().clip = diveSound;
			GetComponent<AudioSource>().Play ();
		}
		jumpPressed = false;
		divePressed = false;

		pos.x = Mathf.Max(-stageSize, Mathf.Min(stageSize, pos.x));

		transform.position = pos;
	}

	private bool isGrounded() {
		return transform.position.y < 0.02f;
	}

	private int doFacing() {
		curDir = newDir;
		return curDir;
	}

	public void faceQueue(int dir) {
		newDir = dir;
	}

	void OnTriggerEnter(Collider c) {
		if (dead || isGrounded() || !diving) return;
		if (c.gameObject.name == "HurtBox") {
			GameObject victim = c.gameObject.transform.parent.gameObject;
			victim.GetComponent<Player>().PendDeath(rigidbody.velocity.x);
			disabled = true;
			//Time.timeScale = 0.05f;
			//killed = true;
			GetComponent<AudioSource>().clip = hitSound;
			GetComponent<AudioSource>().Play ();
		}
	}

	public void PendDeath(float xVel) {
		deathVelocity = xVel;
		deathPending = true;
	}

	public void Die() {
		dead = true;
		GetComponentInChildren<Animation>().Play("Die");
		rigidbody.useGravity = true;
		Vector3 vec = rigidbody.velocity;
		vec.y = 10;
		vec.x = deathVelocity;
		rigidbody.velocity = vec;
		vec = transform.Find("DiveKicker").localPosition;
		vec.y = -0.4f;
		transform.Find("DiveKicker").localPosition = vec;
		//GetComponent<Collider> ().rigidbody.

	}

	public void Jump() {
		jumpPressed = true;
	}

	public void Dive() {
		divePressed = true;
	}
}
