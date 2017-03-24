using UnityEngine;
using System.Collections;

public class AttackSpeedProperty : Property {
	#region Configuration

	public float TimeBetweenAttacks = 1f;

	#endregion

	#region Construction

	void Start()
	{
		this.propertyValue = this.TimeBetweenAttacks;
	}

	#endregion

	#region Properties

	public override string PresentationName {
		get {
			return "Attack speed (time between attacks)";
		}
	}
	
	public new float PropertyValue
	{
		get { return (float) this.propertyValue; }
	}

	#endregion
}
