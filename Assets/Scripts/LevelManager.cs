using System.Collections.Generic;

using UnityEngine;

public interface ILevelManager
{
	#region Operations
	
	#endregion
	
	#region Properties

	IEnumerable<ITarget> PlayerDefendedTargets { get; }

	IEnumerable<ITarget> OpponentDefendedTargets { get; }

	#endregion
	
	#region Events
	
	#endregion
}

public class LevelManager : Manager, ILevelManager
{
	#region Configuration

	public MainBase MainBaseInstance;

	#endregion

	#region Construction

	void Awake ()
	{
		Init (this);
	}

	#endregion

	#region Properties

	public IEnumerable<ITarget> PlayerDefendedTargets 
	{
		get { return new List<ITarget> { MainBaseInstance }; }
	}

	public IEnumerable<ITarget> OpponentDefendedTargets 
	{
		get {
			throw new System.NotImplementedException ();
		}
	}

	#endregion

	#region Operations

	#endregion

	#region Implementation

	#endregion

	#region Event handlers

	#endregion

	#region Data

	#endregion
}