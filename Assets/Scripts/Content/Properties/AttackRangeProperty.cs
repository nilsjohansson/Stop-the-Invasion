using UnityEngine;
using System.Collections;

public interface IAttackRangeProperty : IProperty
{
	/// <summary>
	/// The minimum distance between the <see cref="Actor"/> and it's target before it can start attacking.
	/// </summary>
	new float PropertyValue { get; }
}

public class AttackRangeProperty : Property {
	#region Configuration

	public float AttackRange = 1f;

	#endregion

	#region Construction

	void Start()
	{
		this.propertyValue = AttackRange;
	}

	#endregion
	
	#region Properties

	public override string PresentationName {
		get {
			return "Attack range";
		}
	}
	
	public new float PropertyValue
	{
		get { return (float) this.propertyValue; }
	}

	#endregion
	
	#region Overrides

	#endregion
	
	#region Data
	
	#endregion
}
