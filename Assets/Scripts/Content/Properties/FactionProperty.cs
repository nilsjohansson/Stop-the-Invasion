using UnityEngine;
using System.Collections;

public interface IFactionProperty : IProperty
{
	/// <summary>
	/// Changes the faction of the actor.
	/// </summary>
	/// <param name="newFaction">New faction.</param>
	/// <param name="responsible">Responsible.</param>
	void ChangeFaction(FactionType newFaction, Actor responsible);

	/// <summary>
	/// Gets the current faction this actor is in.
	/// </summary>
	/// <value>The actor's faction.</value>
	new FactionType PropertyValue { get; }
}

public class FactionProperty : Property, IFactionProperty {
	#region Configuration

	public FactionType Faction = FactionType.Red;

	#endregion

	#region Construction

	void Start()
	{
		if(this.propertyValue == null)
			this.propertyValue = this.Faction;
	}

	#endregion

	#region Property

	public new IFactionProperty AsIProperty {
		get {
			return this as IFactionProperty;
		}
	}

	public override string PresentationName {
		get {
			return "Faction: " + ((FactionType)this.propertyValue).ToString();
		}
	}

	public new FactionType PropertyValue {
		get {
			if(this.propertyValue == null)
				this.propertyValue = this.Faction;
			return (FactionType) base.PropertyValue;
		}
	}

	#endregion
	
	#region Public methods

	public void ChangeFaction(FactionType newFaction, Actor responsible)
	{
		this.SetProperty(newFaction, responsible);
	}

	#endregion

	#region Data

	#endregion
}

public enum FactionType
{
	Red,
	Blue,
	/// <summary>
	/// The neutral. Yellow.
	/// </summary>
	Neutral,
	Purple,
	White,
	Black
}