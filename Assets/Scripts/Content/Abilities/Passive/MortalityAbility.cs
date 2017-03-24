using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(HealthProperty))]
public class MortalityAbility : PassiveAbility {
	#region Configuration

	public List<IEffect> BleedEffects = new List<IEffect>();
	public List<float> NormalizedBleedHealthLevels = new List<float>();
	public List<IEffect> DeathEffects = null;

	#endregion

	#region Construction

	void Start()
	{
		if(this.BleedEffects.Count != this.NormalizedBleedHealthLevels.Count)
			throw new UnityException("BleedEffects and NormalizedBleedHealthLevels length mismatch!");
		if(!this.MyActor.TryGetProperty<IHealthProperty>(out health))
			throw new UnityException("Missing HealthProperty on actor "+this.MyActor.PresentationName);

		this.health.OnValueChanged += this.HandleOnHealthChanged;
	}

	#endregion

	#region Properties

	public override string PresentationName {
		get {
			return "Mortality";
		}
	}
	
	public override bool InProgress {
		get {
			return this.IsPaused;
		}
	}

	#endregion

	#region Event handling
	
	private void HandleOnHealthChanged (object sender, Property.PropertyEventArgs e)
	{
		var newHealth = (HealthProperty.HealthInfo) e.NewValue;
		var oldHealth = (HealthProperty.HealthInfo) e.OldValue;

		if(newHealth.CurrentHealth > oldHealth.CurrentHealth)
		{
			// Heal
			// Play bleed effect
			for(int i = 0; i < this.BleedEffects.Count; i++)
			{
				if(newHealth.GetNormalizedHealthLevel() > this.NormalizedBleedHealthLevels[i])
					this.BleedEffects[i].StopEffect();
			}

			return;
		}

		if(newHealth.CurrentHealth == 0)
		{
			var attackStrength = (oldHealth.CurrentHealth - newHealth.Overkill);
			Vector3 attackForce = (this.MyActor.NavigationPosition - e.Responsible.NavigationPosition).normalized;
			attackForce.Scale(new Vector3(attackStrength, 0f, attackStrength));
			// Debug.Log ("AttackForce: "+attackForce.ToString());

			foreach(var deffect in this.DeathEffects)
				deffect.PlayEffect(attackForce);

			this.MyActor.Deactivate();

			return;
		}

		if(newHealth.CurrentHealth < oldHealth.CurrentHealth)
		{
			// Play bleed effect
			for(int i = 0; i < this.BleedEffects.Count; i++)
			{
				if(newHealth.GetNormalizedHealthLevel() < this.NormalizedBleedHealthLevels[i])
					this.BleedEffects[i].PlayEffect();
			}

			return;
		}
	}

	#endregion

	#region Data

	private IHealthProperty health = null;

	#endregion
}
