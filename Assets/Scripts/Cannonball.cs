using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball : MonoBehaviour 
{
	#region Properties

	public float ImpactDamage = 1f;
	public GameObject sparks;

	#endregion

	#region Messages

	void OnCollisionEnter(Collision collision) 
	{
		var enemy = collision.collider.gameObject;
		if (enemy.tag == "Enemy") 
		{
			var script = enemy.GetComponent<MonoBehaviour> () as Enemy;
			script.Impact (this.ImpactDamage);
			var impactAnimation = Instantiate (sparks, gameObject.WPos (), Quaternion.identity);
			impactAnimation.GetComponent<ParticleEmitter> ().Emit ();
			GameObject.Destroy (this.gameObject);
		}
	}

	#endregion
}
