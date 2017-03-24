using UnityEngine;
using System.Collections;

public interface IMineralsProperty : IProperty
{
	/// <summary>
	/// The current amount of collected minerals.
	/// </summary>
	/// <value>An Integer representing the total amount of minerals.</value>
	new int PropertyValue { get; }
	
	/// <summary>
	/// Increases the minerals. Use this for 'earn' transactions.
	/// </summary>
	/// <param name="addedAmount">Added amount.</param>
	/// <param name="responsible">Responsible.</param>
	void IncreaseMinerals(int addedAmount, Actor responsible);

	/// <summary>
	/// Reduces the minerals. Use this for 'buy' transactions.
	/// </summary>
	/// <returns><c>true</c>, if there is enough minerals for the requested transaction., <c>false</c> otherwise.</returns>
	/// <param name="removedAmount">Removed amount.</param>
	/// <param name="responsible">Responsible.</param>
	bool ReduceMinerals(int removedAmount, Actor responsible);
}

public class MineralsProperty : Property, IMineralsProperty {
	#region Configuration
	
	public int StartingMinerals = 10;
	
	#endregion
	
	#region Construction
	
	void Start()
	{
		this.propertyValue = StartingMinerals;
		this.OnValueChanged += HandleHandleOnValueChanged;
	}

	void HandleHandleOnValueChanged (object sender, Property.PropertyEventArgs e)
	{
		Debug.Log("OldValue: "+e.OldValue+ " NewValue: "+e.NewValue+" Actor: "+e.Responsible.PresentationName);
	}
	
	#endregion

	#region Properties

	public new IMineralsProperty AsIProperty {
		get {
			return this as IMineralsProperty;
		}
	}

	public new int PropertyValue
	{
		get { return (int)this.propertyValue; }
	}

	#endregion

	#region Public methods

	/// <summary>
	/// Increases the minerals. Use this for 'earn' transactions.
	/// </summary>
	/// <param name="addedAmount">Added amount.</param>
	/// <param name="responsible">Responsible.</param>
	public void IncreaseMinerals(int addedAmount, Actor responsible)
	{
		int newTotal = (int) this.PropertyValue + addedAmount;
		this.SetProperty(newTotal, responsible);
	}

	/// <summary>
	/// Reduces the minerals. Use this for 'buy' transactions.
	/// </summary>
	/// <returns><c>true</c>, if there is enough minerals for the requested transaction., <c>false</c> otherwise.</returns>
	/// <param name="removedAmount">Removed amount.</param>
	/// <param name="responsible">Responsible.</param>
	public bool ReduceMinerals(int removedAmount, Actor responsible)
	{
		int newTotal = (int) this.PropertyValue - removedAmount;
		if(this.SetProperty(newTotal, responsible))
			return true;
		
		return false;
	}

	#endregion
	
	#region Overrides
	
	public override string PresentationName {
		get {
			return "Minerals";
		}
	}
	
	protected override object ValidateProperty (object setValue, out bool wasChanged)
	{
		int total = (int) setValue;
		if(total < 0) 
		{
			wasChanged = true;
			return (int)0;
		}
		wasChanged = false;
		return setValue;
	}
	
	#endregion
	
	#region Data
	
	#endregion
}
