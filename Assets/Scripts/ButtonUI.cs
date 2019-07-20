using UnityEngine;
using System.Collections;

public class ButtonUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//Debug.Log ("ButtonUI Start");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnPress(bool isDown) {
		if (!isDown) return;
		if (this.name == "ChangeChar1") {
			Debug.Log("ChangeChar1");
			Fight.instance.changeChar(1);
		}else if (this.name == "ChangeChar2") {
			Debug.Log("ChangeChar2");
			Fight.instance.changeChar(2);
		}else if (this.name == "ReadyButton") {
			Fight.instance.Ready();
			Destroy(this.gameObject);
		}else if (this.name == "Reset") {
			Debug.Log ("ButtonUI onclick");
			Application.LoadLevel(0);
		}else{
			Fight.instance.Control(transform.parent.name + this.name);
		}
	}
}
