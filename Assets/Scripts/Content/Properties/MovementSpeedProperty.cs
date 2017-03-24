using UnityEngine;
using System.Collections;

public interface IMovementSpeedProperty : IProperty
{
	/// <summary>
	/// Gets the current movement speed.
	/// </summary>
	/// <value>The property value.</value>
	new float PropertyValue { get; }

	/// <summary>
	/// Sets the movement speed.
	/// </summary>
	/// <param name="newSpeed">New speed.</param>
	/// <param name="responsible">Responsible actor.</param>
	void SetMovementSpeed(float newSpeed, Actor responsible);

	/// <summary>
	/// Sets the movement speed to default.
	/// </summary>
	/// <param name="responsible">Responsible actor.</param>
	void ResetToDefault(Actor responsible);
}

public class MovementSpeedProperty : Property, IMovementSpeedProperty {
	#region Configuration

	public float InitialMovementSpeed = 1f;

	#endregion

	#region Construction

	void Start()
	{
		bool wasChanged = false;
		this.defaultSpeed = (float)this.ValidateProperty(this.InitialMovementSpeed, out wasChanged);
		this.propertyValue = this.defaultSpeed;
	}

	#endregion

	#region Properties

	public new IMovementSpeedProperty AsIProperty {
		get {
			return this as IMovementSpeedProperty;
		}
	}

	public override string PresentationName {
		get {
			return "Movement speed";
		}
	}

	public new float PropertyValue
	{
		get { return (float) this.propertyValue; }
	}

	#endregion

	#region Public methods

	public void SetMovementSpeed(float newSpeed, Actor responsible)
	{
		this.SetProperty(newSpeed, responsible);
	}

	public void ResetToDefault(Actor responsible)
	{
		this.SetProperty(this.defaultSpeed, responsible);
	}

	#endregion

	#region Overrides

	protected override object ValidateProperty (object setValue, out bool wasChanged)
	{
		wasChanged = false;
		float newValue = (float)setValue;
		if(newValue < 0)
		{
			wasChanged = true;
			newValue = 0f;
		}

		return newValue;
	}

	#endregion

	#region Data

	private float defaultSpeed = 0f;

	#endregion
}
