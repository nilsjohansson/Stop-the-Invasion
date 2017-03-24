using UnityEngine;
using System.Collections;

public class KillTester : MonoBehaviour {

	public bool Kill = false;
	// Use this for initialization
	void Start () {
		this.animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Kill) this.animator.SetBool("Kill", true);
	}

	private Animator animator = null;
}
