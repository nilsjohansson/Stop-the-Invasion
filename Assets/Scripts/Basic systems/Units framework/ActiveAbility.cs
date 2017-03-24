using UnityEngine;
using System.Collections;

public interface IActiveAbility : IAbility
{
	Vector3[] TargetedPositions { get; }
	Actor[] TargetedActors { get; }
	ActiveAbility.InputType RequiredInput { get; }
	bool CanActivate();
	bool CanActivate(Vector3[] positions);
	bool CanActivate(Actor[] actors);
	void Activate();
	void Activate(Vector3[] positions);
	void Activate(Actor[] actors);
	void Abort();
	void RegisterFinishedCallback (ActiveAbility.AbilityFinishedCallback callback);
}

public abstract class ActiveAbility : Ability, IActiveAbility {
	
	#region Delegates
	
	public delegate void AbilityFinishedCallback(bool finishedSuccessfully, ActiveAbility ability);
	
	#endregion
	
	#region Public stuff
	
	public enum InputType
	{
		None,
		Positions,
		OtherActors,
		PositionsOrOtherActors
	}
	
	#endregion
	
	#region Properties

	public virtual Vector3[] TargetedPositions
	{
		get { return null; }
	}

	public virtual Actor[] TargetedActors
	{
		get { return null; }
	}

	public abstract InputType RequiredInput {
		get;
	}
	
	public abstract override string PresentationName
	{
		get;
	}

	public new IActiveAbility AsIAbility
	{
		get { return this as IActiveAbility; }
	}
	
	#endregion
	
	#region Public abstract methods
	
	public virtual bool CanActivate()
	{
		return true;
	}
	
	public virtual bool CanActivate(Vector3[] positions)
	{
		return true;
	}
	
	public virtual bool CanActivate(Actor[] actors)
	{
		return true;
	}
	
	public virtual void Activate()
	{
		this.MyActor.SetCurrentAbility(this);
		return;
	}
	
	public virtual void Activate(Vector3[] positions)
	{
		this.MyActor.SetCurrentAbility(this);
		return;
	}
	
	public virtual void Activate(Actor[] actors)
	{
		this.MyActor.SetCurrentAbility(this);
		return;
	}

	public virtual void Abort()
	{
		this.FinishAbility(false);
	}

	#endregion
	
	#region Public methods

	/// <summary>
	/// Registers a delegate that will be called when this <see cref="ActiveAbility"/> is aborted or finishes.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void RegisterFinishedCallback (AbilityFinishedCallback callback)
	{
		if(this.abilityFinishedCallback  == null)
			this.abilityFinishedCallback = new System.Collections.Generic.List<AbilityFinishedCallback>();

		this.abilityFinishedCallback.Add(callback);
	}
	
	#endregion

	#region Inherited methods

	protected void FinishAbility(bool finishedSuccessfully)
	{
		if(this.abilityFinishedCallback != null)
			foreach(var callback in this.abilityFinishedCallback)
				callback(finishedSuccessfully, this);

		this.abilityFinishedCallback = null;
	}

	#endregion
	
	#region Data
	
	private System.Collections.Generic.List<AbilityFinishedCallback> abilityFinishedCallback = null;
	
	#endregion
}
