using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RagdollDeathEffect : IEffect {
	#region Configuration

	public ParticleSystem Explosion = null;
	public float ImpactFactor = 1f;

	#endregion

	#region Construction

	void Start()
	{
		this.Init ();
	}

	protected void Init()
	{
		this.ragdollBodies.AddRange(this.GetComponents<Rigidbody>());
		this.ragdollBodies.AddRange(this.GetComponentsInChildren<Rigidbody>());
		this.ragdollColliders.AddRange(GetComponentsInChildren<Collider>());
		foreach(var coll in this.ragdollColliders)
			coll.enabled = false;
		
		if(this.ragdollBodies.Count > 0)
			foreach(var body in this.ragdollBodies)
		{
			body.isKinematic = true;
			body.sleepAngularVelocity = 0.6f;
			body.sleepVelocity = 0.5f;
		}

		this.animator = this.GetComponent<Animator>();
		this.charController = this.GetComponent<CharacterController>();
		if(this.charController != null) 
			this.charController.enabled = true;
	}

	#endregion

	#region implemented abstract members of IEffect

	public override void PlayEffect (object arg)
	{
		if(arg != null)
			this.attackForce = (Vector3) arg;

		// Die
		foreach(var coll in this.ragdollColliders)
			coll.enabled = true;

		if(this.charController != null)
			this.charController.enabled = false;
		
		//this.animator.SetBool("Kill", true);
		if(this.animator != null)
			this.animator.enabled = false;

		if(this.Explosion != null)
			this.Explosion.Play ();

		this.isDead = true;
		return;
	}
	
	public override void StopEffect ()
	{

	}

	#endregion

	#region Implementation

	private void TurnOffPhysics()
	{
		foreach(var body in this.ragdollBodies)
			body.isKinematic = true;
		foreach(var collider in this.ragdollColliders)
			if(!(collider is CharacterController))
				collider.isTrigger = true;
	}

	#endregion

	#region Update
	
	void FixedUpdate()
	{
		this.PhysicsEffect();
	}

	protected void PhysicsEffect()
	{
		if(this.isDead)
		{
			if(this.ragdollBodies != null && this.ragdollBodies.Count > 0)
				//foreach(var body in this.ragdollBodies)
				for(int i = 0; i < this.ragdollBodies.Count; i++)
			{
				this.ragdollBodies[i].isKinematic = false;
				
				this.ragdollBodies[i].AddForce(this.attackForce * this.ImpactFactor, ForceMode.Impulse);
			}

			var cbm = Manager.GetInstance<ConditionalCallbackManager>() as IConditionalCallback;
			cbm.RegisterDelayedCallback(3, this.TurnOffPhysics);

			this.attackForce = Vector3.zero;
			isDead = false;
		}
	}

	#endregion

	#region Data

	protected List<Rigidbody> ragdollBodies = new List<Rigidbody>();
	
	private bool isDead = false;
	private Vector3 attackForce = Vector3.zero;
	private Animator animator = null;
	private CharacterController charController = null;
	private List<Collider> ragdollColliders = new List<Collider>();

	#endregion
}
