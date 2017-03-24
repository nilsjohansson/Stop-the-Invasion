using UnityEngine;
using System.Collections;

public class TurnTester : MonoBehaviour {

	void Start () {
		this.charController = GetComponent<CharacterController>();
	}
	
	void Update () {
		this.transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * 180f, Vector3.up);
		
		this.charController.Move(Time.deltaTime * transform.forward * 0.5f);
	}
	
	#region Data
	
	private CharacterController charController;
	
	
	#endregion
}
