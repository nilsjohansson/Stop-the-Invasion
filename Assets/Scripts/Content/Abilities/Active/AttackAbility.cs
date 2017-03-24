using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AttackRangeProperty))]
[RequireComponent(typeof(AttackSpeedProperty))]
[RequireComponent(typeof(WeaponProperty))]
public class AttackAbility : ActiveAbility, IActiveAbility {
	#region Configuration

	public string AttackStanceAnimBoolean = string.Empty;
	public string AttackStanceAnimName = string.Empty;
	public string AttackAnimTrigger = string.Empty;
	public string AttackAnimName = string.Empty;
	public string UnsheatheWeaponAnimName = string.Empty;
	public string SheatheWeaponAnimName = string.Empty;
	public int AnimationLayer = 1;
	public float DamageDealingMoment = 0.5f;
	public float UnsheatheMoment = 0.7f;

	#endregion

	#region Construction

	void Start()
	{
		if(!this.MyActor.TryGetProperty<AttackRangeProperty>(out this.attackRange))
			throw new UnityException("Could not get AttackRangeProperty from actor '"+this.MyActor+"'.");
		if(!this.MyActor.TryGetProperty<AttackSpeedProperty>(out this.attackSpeed))
			throw new UnityException("Could not get AttackSpeedProperty from actor '"+this.MyActor+"'.");
		if(!this.MyActor.TryGetProperty<IWeaponProperty>(out this.weaponProperty))	
			throw new UnityException("Could not get WeaponProperty from actor '"+this.MyActor+"'.");
		if(this.AttackStanceAnimBoolean == string.Empty)
			throw new UnityException("Missing id of the boolean for attack stance animation!");
		if(this.AttackAnimTrigger == string.Empty)
			throw new UnityException("Missing trigger id for attack animation!");
		if(this.AttackAnimName == string.Empty)
			throw new UnityException("Missing name of the attack animation!");
		if(this.AttackStanceAnimName == string.Empty)
			throw new UnityException("Missing name of the attack stance animation!");
		this.moveAbilityRef = GetComponent<MoveAbility> ();
		this.animator = GetComponent<Animator>();
		this.animator.SetLayerWeight(this.AnimationLayer, 1f);
		this.charController = GetComponent<CharacterController>();
		this.DamageDealingMoment = Mathf.Clamp(this.DamageDealingMoment, 0f, 1f);
		this.UnsheatheMoment = Mathf.Clamp(this.UnsheatheMoment, 0f, 1f);
	}

	#endregion

	#region Properties
	
	public override Vector3[] TargetedPositions
	{
		get { return new Vector3[] { this.targetPosition }; }
	}
	
	public override Actor[] TargetedActors
	{
		get 
		{ 
			List<Actor> allTargets = new List<Actor>();
			if(this.targetActor != null)
			{
				allTargets.Add(this.targetActor);
				allTargets.AddRange(this.targetQueue);
			}
			return allTargets.ToArray(); 
		}
	}

	public override string PresentationName {
		get {
			return "Attack";
		}
	}

	public override InputType RequiredInput {
		get {
			return InputType.OtherActors;
		}
	}

	public override bool InProgress {
		get {
			return !(this.attackState == AttackState.None);
		}
	}

	public new IActiveAbility AsIAbility {
		get {
			return base.AsIAbility;
		}
	}

	#endregion
	
	#region Overrides

	public override void Activate (Actor[] actors)
	{
		base.Activate();

		this.chosenInputType = InputType.OtherActors;

		this.targetQueue.Clear();

		foreach(var actor in actors)
			if(actor != this.MyActor)
				this.targetQueue.Enqueue(actor);

		// This will likely only happen if the actor itself was the only target
		if(this.targetQueue.Count == 0)
		{
			this.FinishAbility(false);
			return;
		}

		this.targetActor = this.targetQueue.Dequeue();

		if(!this.InProgress || !this.IsInrange())
			this.attackState = AttackState.OutOfRange;
	}

	public override void Abort ()
	{
		base.Abort ();

		if(this.attackState == AttackState.UnsheathingWeapon || this.attackState == AttackState.InRange || this.attackState == AttackState.Attack || this.attackState == AttackState.FinalizeAttack)
		{
			this.animator.SetBool(this.AttackStanceAnimBoolean, false);
			if(this.SheatheWeaponAnimName != string.Empty)
				this.attackState = AttackState.SheathingWeapon;
		}
		else
			this.ResetAbility();
	}

	#endregion

	#region Update

	void Update()
	{
		var animState = this.animator.GetCurrentAnimatorStateInfo(this.AnimationLayer);
		var layerName = this.animator.GetLayerName(this.AnimationLayer);

		switch(this.attackState)
		{
		case AttackState.None:
			break;
		case AttackState.OutOfRange:
			if(this.IsInrange())
			{
				this.attackState = AttackState.InRange;
			}
			else
			{
				// If actor can't move, stop trying to attack.
				if(this.moveAbilityRef != null)
				{
					this.SetupMovementToTarget();
				}
				else
				{
					this.ResetAbility();
				}
			}

			break;
		case AttackState.MovingToTarget:
			if(this.IsInrange())
			{
				this.attackState = AttackState.InRange;
				this.moveAbilityRef.Abort();
			}

			break;
		case AttackState.InRange:
			// Make sure that the attacking can proceed.
			if(!this.ValidateAttackContext()) return;

			// Turn towards target
			this.transform.rotation = Quaternion.LookRotation(Vector3.Scale (this.targetActor.NavigationPosition - this.MyActor.NavigationPosition, new Vector3(1f, 0f, 1f)), Vector3.up);

			// Go into attack stance
			if(this.animator.GetBool(this.AttackStanceAnimBoolean) == false)
			{
				this.animator.SetBool(this.AttackStanceAnimBoolean, true);
				if(this.UnsheatheWeaponAnimName != string.Empty && this.weaponProperty.HasWeapon)
					this.attackState = AttackState.UnsheathingWeapon;
			}

			// Display attack animation
			if(animState.IsName(layerName + "." + this.AttackStanceAnimName) && 
				Time.time > this.lastAttackFinishedTime + this.attackSpeed.PropertyValue)
			{
				this.animator.SetTrigger(this.AttackAnimTrigger);
				this.attackState = AttackState.Attack;
			}

			break;
		case AttackState.UnsheathingWeapon:
			if(animState.IsName(layerName + "." + this.UnsheatheWeaponAnimName))
			{
				if(animState.normalizedTime >= this.UnsheatheMoment)
				{
					this.weaponProperty.UnsheatheWeapon();
					this.attackState = AttackState.InRange;
				}
			}

			break;
		case AttackState.SheathingWeapon:
			if(animState.IsName(layerName + "." + this.SheatheWeaponAnimName))
			{
				if(animState.normalizedTime >= 1f - this.UnsheatheMoment)
					this.weaponProperty.SheatheWeapon();
			}
			else
				if(this.weaponProperty.IsWeaponSheathed) this.ResetAbility();

			break;
		case AttackState.Attack:
			// Turn towards target
			this.transform.rotation = Quaternion.LookRotation(Vector3.Scale (this.targetActor.NavigationPosition - this.MyActor.NavigationPosition, new Vector3(1f, 0f, 1f)), Vector3.up);

			// Deal damage at the proper point in time
			if(animState.IsName(layerName + "." + this.AttackAnimName) && animState.normalizedTime >= this.DamageDealingMoment)
			{
				this.DealDamage();
				// Wait for animation to finish
				this.attackState = AttackState.FinalizeAttack;
			} 

			break;
		case AttackState.FinalizeAttack:
			// Turn towards target
			this.transform.rotation = Quaternion.LookRotation(Vector3.Scale (this.targetActor.NavigationPosition - this.MyActor.NavigationPosition, new Vector3(1f, 0f, 1f)), Vector3.up);

			if(!animState.IsName(layerName + "." + this.AttackAnimName))
			{
				this.lastAttackFinishedTime = Time.time;
				this.attackState = AttackState.InRange;
			}

			break;
		}
	}

	#endregion

	#region Implementation

	private void DealDamage()
	{
		this.targetActorHealthProperty.Damage(this.weaponProperty.PropertyValue.DamageInfo.Randomize(), this.MyActor);
		this.weaponProperty.PlayImpactEffect();
	}

	private bool IsInrange(Actor actor = null)
	{
		if(actor == null) 
			actor = targetActor;

		if(this.chosenInputType == InputType.OtherActors)
		{
			/*
			var hits = Physics.OverlapSphere(this.MyActor.NavigationPosition, this.attackRange.PropertyValue);
			foreach(var hit in hits)
				if(hit.gameObject.GetComponent<Actor>() == actor)
					return true;
			*/
			float distanceToTarget = (Vector3.Scale(this.MyActor.NavigationPosition, new Vector3(1f, 0f, 1f)) 
			                          - Vector3.Scale(this.targetActor.NavigationPosition, new Vector3(1f, 0f, 1f))).magnitude;
			
			if(this.targetActor != null)
				distanceToTarget -= (this.charController.radius * this.transform.lossyScale.x + this.targetActor.AttachedCharController.radius * this.targetActor.transform.lossyScale.x);

			if(distanceToTarget < this.attackRange.PropertyValue)
				return true;
			//var distanceVector = this.MyActor.NavigationPosition - actor.NavigationPosition;

			//if(Vector3.SqrMagnitude(distanceVector) < this.attackRange.PropertyValue * this.attackRange.PropertyValue)
			//	return true;
		}
		else
		{
			if(Vector3.SqrMagnitude(this.MyActor.NavigationPosition - this.targetPosition) < this.attackRange.PropertyValue * this.attackRange.PropertyValue)
				return true;
		}

		return false;
	}

	private void SetupMovementToTarget()
	{
		if(this.chosenInputType == InputType.OtherActors)
		{
			this.moveAbilityRef.RegisterFinishedCallback(this.OnMovementFinishedCallback);
			this.moveAbilityRef.Activate(new Actor[1] { this.targetActor });
			this.attackState = AttackState.MovingToTarget;
		}
		else
		{
			// TODO : A - Move
		}
	}

	/// <summary>
	/// Validates the attack context.
	/// </summary>
	/// <returns><c>true</c>, if attack can proceed, <c>false</c> otherwise.</returns>
	private bool ValidateAttackContext()
	{
		// Does the target actor have health?
		if(!this.targetActor.TryGetProperty(out this.targetActorHealthProperty))
		{
			Debug.Log ("Cannot attack target: "+targetActor.PresentationName+"! Missing HealthProperty.");
			this.Abort ();
			return false;
		}

		// Is the target Actor's health 0?
		if(this.targetActorHealthProperty.PropertyValue.CurrentHealth <= 0f)
		{
			if(this.targetQueue.Count > 0)
			{
				var nextTarget = this.targetQueue.Dequeue() as Actor;
				if(nextTarget != null && this.IsInrange(nextTarget))
				{
					this.targetActor = nextTarget;
					this.attackState = AttackState.InRange;
					this.lastAttackFinishedTime = Time.time;

					return true;
				}
			}
			else
				this.Abort ();

			return false;
		}

		// Is the target Actor within range
		if(!IsInrange()) 
		{
			this.Abort ();
			return false;
		}

		return true;
	}

	/// <summary>
	/// Resets the ability to initial state. Removes target, resets animation states etc. Call this method to abort ability.
	/// </summary>
	private void ResetAbility()
	{
		if(this.attackState == AttackState.MovingToTarget) this.moveAbilityRef.Abort();

		this.targetQueue.Clear();
		this.targetActor = null;
		this.animator.SetBool(this.AttackStanceAnimBoolean, false);
		this.targetActorHealthProperty = null;
		this.targetPosition = Vector3.zero;
		this.attackState = AttackState.None;
		this.chosenInputType = InputType.None;
	}

	#endregion

	#region Event handlers

	private void OnMovementFinishedCallback (bool successful, ActiveAbility ability)
	{
		// Debug.Log ("OnMovementFinishedCallback()");
		// If we got to the target point uninterrupted but with no target in sight, the attack failed.
		if(!this.IsInrange() && successful) 
		{
			this.Abort ();
		}
	}

	#endregion

	#region State

	private enum AttackState
	{
		None,
		OutOfRange,
		MovingToTarget,
		InRange,
		UnsheathingWeapon,
		SheathingWeapon,
		Attack,
		FinalizeAttack
	}

	private AttackState attackState = AttackState.None;

	#endregion

	#region Data
	
	private Queue<Actor> targetQueue = new Queue<Actor>();
	private InputType chosenInputType = InputType.None;
	private Actor targetActor = null;
	private Vector3 targetPosition = Vector3.zero;
	private AttackRangeProperty attackRange = null;
	private AttackSpeedProperty attackSpeed = null;
	private IWeaponProperty weaponProperty = null;
	private MoveAbility moveAbilityRef = null;
	private Animator animator = null;
	private HealthProperty targetActorHealthProperty = null;
	private float lastAttackFinishedTime = 0f;
	private CharacterController charController = null;

	#endregion
}
