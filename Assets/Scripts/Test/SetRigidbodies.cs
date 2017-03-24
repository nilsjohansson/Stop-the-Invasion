using UnityEngine;
using System.Collections;

public class SetRigidbodies : MonoBehaviour 
{

	public bool UseGravity = true;
	public bool IsKinematic = true;

	public Rigidbody head = null;

	void Start () 
	{
		var bodies = this.GetComponentsInChildren<Rigidbody>();

		foreach(var body in bodies) {
			body.useGravity = UseGravity;
			body.isKinematic = IsKinematic;
		}
	}
	
	void Update () {
		if(Input.GetAxis("Fire1") == 1)
		{
			Debug.Log ("Adding force!");
			head.AddForce(new Vector3(15,0,0f), ForceMode.Impulse);
		}
	}
}
