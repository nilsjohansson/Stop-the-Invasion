using UnityEngine;
using System.Collections;

public interface IWeaponProperty : IProperty
{
	/// <summary>
	/// The statistics of the weapon. Base + Top damage, transforms for carrying and wielding & a reference to the GameObject with the graphical representation of the weapon.
	/// </summary>
	/// <value>The property value.</value>
	new WeaponProperty.WeaponInfo PropertyValue { get; }

	/// <summary>
	/// Determines if the weapon is sheathed. Bases this on the parent transform of the Weapon GameObject.
	/// </summary>
	/// <value><c>true</c> if weapon is sheathed; otherwise, <c>false</c>.</value>
	bool IsWeaponSheathed { get; }

	/// <summary>
	/// Gets a value indicating whether the weapon has a graphical representation or not. E.g. knuckles.
	/// </summary>
	/// <value><c>true</c> if this weapon has graphical representation; otherwise, <c>false</c>.</value>
	bool HasWeapon { get; }

	/// <summary>
	/// Unsheathes the weapon. Sets the WeaponGameObject's transform parent to WieldTransform.
	/// </summary>
	void UnsheatheWeapon();

	/// <summary>
	/// Sheathes the weapon. Sets the WeaponGameObject's transform parent to CarryTransform.
	/// </summary>
	void SheatheWeapon();

	/// <summary>
	/// Plays an impact effect when this weapon hits its target.
	/// </summary>
	void PlayImpactEffect();
}

public class WeaponProperty : Property, IWeaponProperty {
	#region Configuration

	public string WeaponName = "Standard weapon";
	public Transform CarryTransform = null;
	public Transform WieldTransform = null;
	public GameObject Weapon = null;
	public IEffect ImpactEffect = null;
	
	public float BaseDamage = 5f;
	public float TopDamage = 5f;
	public DamageType AttackDamageType = DamageType.Melee;

	#endregion

	#region Construction

	void Start()
	{
		if(this.Weapon == null)
		{
			Debug.Log("Weapon not configured for WeaponProperty.");
		}

		this.propertyValue = new WeaponInfo(Weapon, CarryTransform, WieldTransform, new DamageInfo(this.BaseDamage, this.TopDamage, this.AttackDamageType));
		this.SheatheWeapon();
	}

	#endregion

	#region Properties

	/// <summary>
	/// Property boiled down to the public members of <see cref="WeaponProperty"/>.
	/// </summary>
	/// <value>As <see cref="IWeaponProperty"/> interface.</value>
	public new IWeaponProperty AsIProperty
	{
		get { return this as IWeaponProperty; }
	}

	public override string PresentationName {
		get {
			return this.WeaponName;
		}
	}

	public new WeaponInfo PropertyValue {
		get {
			return (WeaponInfo)base.PropertyValue;
		}
	}

	public bool IsWeaponSheathed
	{
		get { return !this.HasWeapon || this.PropertyValue.WeaponGameObject.transform.parent == this.PropertyValue.CarryTransform; }
	}

	public bool HasWeapon 
	{ 
		get { return this.PropertyValue.WeaponGameObject != null; } 
	}

	#endregion

	#region Public methods

	public void PlayImpactEffect()
	{
		if(this.ImpactEffect != null)
			this.ImpactEffect.PlayEffect();
	}

	/// <summary>
	/// Parents the transform of the WeaponGameObject to the WieldTransform and resets its local transform.
	/// </summary>
	public void UnsheatheWeapon()
	{
		if(!this.HasWeapon)
			return;

		this.PropertyValue.WeaponGameObject.transform.parent = this.PropertyValue.WieldTransform;
		this.PropertyValue.WeaponGameObject.transform.localPosition = Vector3.zero;
		this.PropertyValue.WeaponGameObject.transform.localRotation = Quaternion.identity;
	}

	/// <summary>
	/// Parents the transform of the WeaponGameObject to the CarryTransform and resets its local transform.
	/// </summary>
	public void SheatheWeapon()
	{
		if(!this.HasWeapon)
			return;

		this.PropertyValue.WeaponGameObject.transform.parent = this.PropertyValue.CarryTransform;
		this.PropertyValue.WeaponGameObject.transform.localPosition = Vector3.zero;
		this.PropertyValue.WeaponGameObject.transform.localRotation = Quaternion.identity;
	}

	#endregion

	#region Overrides

	protected override object ValidateProperty (object setValue, out bool wasChanged)
	{
		wasChanged = false;
		var wInfo = (WeaponInfo)setValue;
		var newDamageInfo = wInfo.DamageInfo.Validate(out wasChanged);
		if(wasChanged)
			wInfo.DamageInfo = newDamageInfo;

		return wInfo;
	}

	#endregion
	
	#region Data

	#endregion

	#region Private classes
	
	public class WeaponInfo
	{
		#region Construction

		public WeaponInfo (GameObject weapon, Transform carryTransform, Transform wieldTransform, DamageInfo damageInfo)
		{
			this.CarryTransform = carryTransform;
			this.WieldTransform = wieldTransform;
			this.DamageInfo = damageInfo;
			this.WeaponGameObject = weapon;
		}

		#endregion

		#region Properties

		public DamageInfo DamageInfo { get; set; }
		public Transform CarryTransform { get; set; }
		public Transform WieldTransform { get; set; }
		public GameObject WeaponGameObject { get; private set; }

		#endregion
	}

	#endregion
}
