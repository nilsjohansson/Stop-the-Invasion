using UnityEngine;
using System.Collections;

public class ReactToDamageAbility : PassiveAbility {
	#region Configuration

	public int DamageAnimationLayer = 1;
	public string FrontDamageAnimationName = "";
	public string BackDamageAnimationName = "";
	public string LeftDamageAnimationName = "";
	public string RightDamageAnimationName = "";

	#endregion

	#region Construction

	void Start()
	{
		this.animator = this.GetComponent<Animator>();
		this.MyActor.TryGetProperty<IHealthProperty>(out health);
		
		this.health.OnValueChanged += OnActorHealthChanged;
		this.animator.SetLayerWeight(this.DamageAnimationLayer, 0f);
	}

	#endregion

	#region Properties

	public override string PresentationName {
		get {
			return "Receive damage";
		}
	}

	public override bool InProgress {
		get {
			return this.IsPaused;
		}
	}

	#endregion

	#region Event handlers
	
	void OnActorHealthChanged (object sender, Property.PropertyEventArgs e)
	{
		var newHealth = (HealthProperty.HealthInfo) e.NewValue;
		var oldHealth = (HealthProperty.HealthInfo) e.OldValue;
		var normalizedDamageDealt = (oldHealth.CurrentHealth - newHealth.CurrentHealth) / newHealth.MaxHealth;

		if(newHealth.CurrentHealth == 0f)
			return;

		if(newHealth.CurrentHealth < oldHealth.CurrentHealth)
		{
			if(e.Arguments == null || e.Arguments.Length == 0)
			{
				Debug.Log ("no arguments");
				return;
			}

			if(!(e.Arguments[1] is Vector3))
			{
				Debug.Log ("no direction");
				return;
			}

			Vector3 impactDirection = (Vector3) e.Arguments[1];
			if(impactDirection == default(Vector3))
			{
				impactDirection = this.MyActor.NavigationPosition - e.Responsible.NavigationPosition;
				Debug.Log("recalculating impact direction from actors positions.");
			}

			impactDirection = this.transform.InverseTransformDirection(impactDirection);
			Debug.Log("Impactdirection: " + impactDirection);
			var frontDirection = impactDirection.z;
			var sideDirection = impactDirection.x;

			this.animator.SetLayerWeight(this.DamageAnimationLayer, normalizedDamageDealt / 2f);

			// Determine dominant direction
			if(Mathf.Abs(frontDirection) > Mathf.Abs(sideDirection))
			{
				if(frontDirection < 0)
				{
					this.animator.SetTrigger(this.FrontDamageAnimationName);
					return;
				}
				this.animator.SetTrigger(this.BackDamageAnimationName);
			}
			else
			{
				if(sideDirection < 0)
				{
					this.animator.SetTrigger(this.LeftDamageAnimationName);
					return;
				}
				this.animator.SetTrigger(this.RightDamageAnimationName);
			}
		}
	}

	#endregion

	#region Data

	private Animator animator = null;
	private IHealthProperty health = null;

	#endregion
}
