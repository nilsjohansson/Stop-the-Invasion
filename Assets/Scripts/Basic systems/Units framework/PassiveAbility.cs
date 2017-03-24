using UnityEngine;
using System.Collections;

public abstract class PassiveAbility : Ability {
	#region Construction
	
	#endregion

	#region Properties

	#endregion

	#region Public methods

	/// <summary>
	/// Pauses the <see cref="BackgroundProcess"/> method of this passive ability. 
	/// </summary>
	/// <value><c>true</c> stops running the <see cref="PassiveAbility.BackgroundProcess"/> method ; otherwise, <c>false</c>.</value>
	public virtual bool IsPaused
	{
		get { return this.isPaused; }
		set { this.isPaused = value; }
	}

	/// <summary>
	/// The process run by this passive ability. Use this instead of MonoBehaviour.Update() as it can be controlled via <see cref="IsPaused"/>.
	/// </summary>
	public virtual void BackgroundProcess()
	{

	}

	#endregion

	#region Update

	void Update()
	{
		if(!isPaused) this.BackgroundProcess();
	}

	#endregion
	
	#region Event handling
	
	#endregion
	
	#region Data

	private bool isPaused = false;

	#endregion
}
