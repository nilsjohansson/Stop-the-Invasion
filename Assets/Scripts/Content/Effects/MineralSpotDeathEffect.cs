using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineralSpotDeathEffect : RagdollDeathEffect {
	#region Configuration

	public float EffectDuration = 2f;
	public GameObject MineralPrefab = null;
	public float ExplosionForce = 35f;

	#endregion

	#region Construction

	void Start()
	{
		this.Init();

		for(int i = 0; i < this.transform.childCount; i++)
		{
			var go = this.transform.GetChild(i);
			this.stoneShards.Add(go.gameObject);
		}

		var actor = this.GetComponent<Actor>();
		MineralsProperty mp = null;
		if(!actor.TryGetProperty<MineralsProperty>(out mp))
			Destroy (this);
		this.mineralsProperty = mp.AsIProperty;
	}

	#endregion

	#region Overrides

	public override void PlayEffect (object arg)
	{
		base.PlayEffect (null);
		if(arg != null) 
			this.forceDirection = (Vector3)arg;
		this.hasDied = true;
	}

	#endregion

	#region Update

	void FixedUpdate()
	{
		this.PhysicsEffect();

		if (this.hasDied)
		{
			for(int i = 0; i < this.mineralsProperty.PropertyValue; i++)
			{
				var mineral = (GameObject)Instantiate(this.MineralPrefab, this.transform.position - 0.5f * this.forceDirection.normalized + i * 0.05f * Vector3.right, Quaternion.identity);
				mineral.GetComponent<Rigidbody>().isKinematic = false;
				mineral.GetComponent<Rigidbody>().GetComponent<Collider>().isTrigger = false;
				mineral.transform.parent = null;
				this.ragdollBodies.Add(mineral.GetComponent<Rigidbody>());
				this.newMinerals.Add(mineral);
			}

			foreach(var body in this.ragdollBodies)
			{
				body.AddExplosionForce(this.ExplosionForce, this.transform.position + this.forceDirection, 20f, 0.5f, ForceMode.Impulse);
			}

			this.hasDied = false;

			Manager.GetInstance<ConditionalCallbackManager>().RegisterDelayedCallback(this.EffectDuration, this.CleanUp);
		}
	}

	#endregion

	#region Implementation

	private void CleanUp()
	{
		foreach(var go in this.stoneShards)
		{
			Destroy (go);
		}
		
		foreach(var body in this.ragdollBodies)
			body.isKinematic = true;
		
		foreach(var go in this.newMinerals)
			go.GetComponent<Collider>().isTrigger = true;
		
		Destroy(this);
	}

	#endregion

	#region Data

	private IMineralsProperty mineralsProperty = null;
	private List<GameObject> stoneShards = new List<GameObject>();
	private List<GameObject> newMinerals = new List<GameObject>();
	private Vector3 forceDirection = Vector3.zero;
	private bool hasDied = false;

	#endregion
}
