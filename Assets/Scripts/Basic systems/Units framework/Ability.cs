using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IAbility
{
	string PresentationName { get; }
	bool InProgress { get; }
}

[RequireComponent(typeof(Actor))]
public abstract class Ability : MonoBehaviour, IAbility {
	
	#region Properties
	
	public abstract string PresentationName {
		get;
	}
	
	public abstract bool InProgress
	{
		get;
	}
	
	protected virtual Actor MyActor {
		get
		{
			if(actor == null)
				this.actor = GetComponent<Actor>();
			
			return actor;
		}
	}

	public virtual IAbility AsIAbility
	{
		get { return this as IAbility; }
	}

	#endregion
	
	#region Data
	
	private Actor actor = null;
	
	#endregion
}