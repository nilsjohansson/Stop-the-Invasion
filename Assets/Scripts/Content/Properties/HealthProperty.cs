using UnityEngine;
using System.Collections;

public interface IHealthProperty : IProperty
{
	/// <summary>
	/// The current amount of health of this actor.
	/// </summary>
	/// <value>The property value.</value>
	new HealthProperty.HealthInfo PropertyValue { get; }

	/// <summary>
	/// Increases the health by a specified amount. The health will never increase above the max health.
	/// </summary>
	/// <param name="addedHealth">Added health.</param>
	/// <param name="responsible">Responsible actor.</param>
	void Heal(float addedHealth, Actor responsible);

	/// <summary>
	/// Reduces the health by a specified amount. The health will never decrease below 0.
	/// </summary>
	/// <param name="removedHealth">Removed health.</param>
	/// <param name="responsible">Responsible actor.</param>
	/// <param name="damageType">Damage type.</param>
	/// <param name="attackDirection">Direction in which the damage impacts.</param>
	void Damage(float removedHealth, Actor responsible, DamageType damageType = DamageType.Melee, Vector3 attackDirection = default(Vector3));

	/// <summary>
	/// Occurs when the value of a <see cref="Property"/> has changed. 
	/// Arguments passed are damage type <see cref="DamageType"/> and damage impact direction <see cref="Vector3"/>.
	/// </summary>
	event System.EventHandler<Property.PropertyEventArgs> OnValueChanged;
}

public class HealthProperty : Property, IHealthProperty {
	#region Configuration

	public float MaxHealth = 100f;

	#endregion

	#region Construction

	void Start()
	{
		this.propertyValue = new HealthInfo(this.MaxHealth, this.MaxHealth);
	}

	#endregion

	#region Properties

	public new IHealthProperty AsIProperty {
		get {
			return this as IHealthProperty;
		}
	}

	public override string PresentationName {
		get {
			return "Health";
		}
	}

	public new HealthInfo PropertyValue
	{
		get { return (HealthInfo) this.propertyValue; }
	}

	#endregion

	#region Public methods

	public void Heal(float addedHealth, Actor responsible)
	{
		HealthInfo newTotal = this.PropertyValue.CopyNew();
		newTotal.CurrentHealth += addedHealth;
		this.SetProperty(newTotal, responsible);
	}
	
	public void Damage(float removedHealth, Actor responsible, DamageType damageType = DamageType.Melee, Vector3 attackDirection = default(Vector3))
	{
		HealthInfo newTotal = this.PropertyValue.CopyNew();
		newTotal.CurrentHealth -= removedHealth;
		this.SetProperty(newTotal, responsible, damageType, attackDirection);
	}

	#endregion

	#region Overrides

	protected override object ValidateProperty (object setValue, out bool wasChanged)
	{
		wasChanged = false;
		var newValue = (HealthProperty.HealthInfo) setValue;
		newValue.Overkill = newValue.CurrentHealth;
		if(newValue.MaxHealth < 0)
		{
			newValue.MaxHealth = 0f;
			wasChanged = true;
		}

		if(newValue.CurrentHealth > this.PropertyValue.MaxHealth) 
		{
			newValue.CurrentHealth = this.MaxHealth;
			wasChanged = true;
		}

		if(newValue.CurrentHealth < 0) 
		{
			newValue.CurrentHealth = 0f;
			wasChanged = true;
		}

		return newValue;
	}

	#endregion

	#region Data

	#endregion

	#region Internal classes

	public class HealthInfo
	{
		#region Construction

		public HealthInfo (float currentHealth, float maxHealth)
		{
			this.CurrentHealth = currentHealth;
			this.MaxHealth = maxHealth;
		}

		#endregion

		#region Properties

		public float CurrentHealth 
		{
			get;
			set;
		}

		public float MaxHealth 
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates the amount of health below zero after a killing blow.
		/// </summary>
		/// <value>The overkill.</value>
		public float Overkill
		{
			get;
			set;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Gets the normalized health level between MaxHealth and CurrentHealth.
		/// </summary>
		/// <returns>The normalized health level.</returns>
		public float GetNormalizedHealthLevel()
		{
			return this.CurrentHealth / this.MaxHealth;
		}

		/// <summary>
		/// Returns a new instance of <see cref="HealthInfo"/> with the same values as this one.
		/// </summary>
		/// <returns>The new instance.</returns>
		public HealthInfo CopyNew()
		{
			return new HealthInfo(this.CurrentHealth, this.MaxHealth);
		}

		#endregion
	}

	#endregion
}
