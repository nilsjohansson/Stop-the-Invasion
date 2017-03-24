using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class Cannonball : MonoBehaviour 
{
	#region Properties

	public float ImpactDamage = 1f;
	public GameObject sparks;

	#endregion

	#region Operations

	public void SetOwner(Actor owner)
	{
		this.owner = owner;
	}

	#endregion

	#region Messages

	void OnCollisionEnter(Collision collision) 
	{
		HealthProperty targetActorHealthProperty = null;

		var enemy = collision.collider.gameObject;
		var actor = enemy.GetComponent<Actor>();

		if(actor == owner)
			return;

		if(actor != null)
			actor.TryGetProperty<HealthProperty>(out targetActorHealthProperty);

		if (targetActorHealthProperty != null) 
		{
			targetActorHealthProperty.Damage(ImpactDamage, ScriptAssistant.GetAllInstancesWithType<MainBaseActor>().First());

			var impactAnimation = Instantiate (sparks, gameObject.WPos (), Quaternion.identity);
			impactAnimation.GetComponent<ParticleEmitter> ().Emit ();
			GameObject.Destroy (this.gameObject);
		}

		Destroy(this.gameObject, 1f);
	}

	#endregion

	#region Data

	private Actor owner;

	#endregion
}
