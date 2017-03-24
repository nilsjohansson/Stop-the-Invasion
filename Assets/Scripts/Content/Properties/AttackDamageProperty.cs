using UnityEngine;
using System.Collections;

public interface IAttackDamageProperty : IProperty
{
	/// <summary>
	/// Gets the property value.
	/// </summary>
	/// <value>The property value.</value>
	new DamageInfo PropertyValue { get; }
}

public class AttackDamageProperty : Property, IAttackDamageProperty {
	#region Configuration

	public float BaseDamage = 5f;
	public float TopDamage = 5f;
	public IEffect EffectOnImpact = null;
	public DamageType AttackDamageType = DamageType.Melee;

	#endregion

	#region Construction

	void Start()
	{
		this.propertyValue = new DamageInfo(this.BaseDamage, this.TopDamage, this.AttackDamageType);
	}

	#endregion

	#region Properties

	public new IAttackDamageProperty AsIProperty {
		get {
			return this as IAttackDamageProperty;
		}
	}

	public override string PresentationName {
		get {
			return "Damage";
		}
	}

	public new DamageInfo PropertyValue {
		get {
			return (DamageInfo)base.PropertyValue;
		}
	}

	#endregion

	#region Overrides

	protected override object ValidateProperty (object setValue, out bool wasChanged)
	{
		return this.PropertyValue.Validate(out wasChanged);
	}

   	#endregion
}

/// <summary>
/// Damage specification of a single strike, shot or attack. 
/// </summary>
public class DamageInfo
{
	#region Construction
	
	public DamageInfo (float baseDamage = 0f, float topDamage = 0f, DamageType damageType = DamageType.Melee)
	{
		this.BaseDamage = baseDamage;
		this.TopDamage = topDamage;
		this.DamageType = damageType;
	}
	
	#endregion
	
	#region Properties
	
	public float BaseDamage { get; set; }
	public float TopDamage { get; set; }
	public DamageType DamageType { get; set; }
	
	#endregion
	
	#region Public methods
	
	/// <summary>
	/// Calculates a random attack damage of a single attack. Ranges between BaseDamage and TopDamage.
	/// </summary>
	/// <returns>The attack damage.</returns>
	public float Randomize()
	{
		return Random.Range(this.BaseDamage, this.TopDamage);
	}

	/// <summary>
	/// Validates this <see cref="DamageInfo"/> instance and returns an instance that was adjusted to pass validation.
	/// </summary>
	/// <param name="wasChanged">If there was any changes.</param>
	public DamageInfo Validate(out bool wasChanged)
	{
		wasChanged = false;
		var tempDamageInfo = this;
		if(this.BaseDamage < 0) 
		{
			tempDamageInfo.BaseDamage = 0;
			wasChanged |= true;
		}
		
		if(this.TopDamage < this.BaseDamage)
		{
			tempDamageInfo.TopDamage = tempDamageInfo.BaseDamage;
			wasChanged |= true;
		}
		return tempDamageInfo;
	}

	#endregion
}

/// <summary>
/// Damage type.
/// </summary>
public enum DamageType
{
	Melee
}
