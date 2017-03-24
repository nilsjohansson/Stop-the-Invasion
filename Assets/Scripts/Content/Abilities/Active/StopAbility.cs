using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MoveAbility))]
public class StopAbility : ActiveAbility {
	
	#region Construction
	
	void Start()
	{

	}
	
	#endregion
	
	#region ActiveAbility abstract members
	
	public override InputType RequiredInput {
		get {
			return InputType.None;
		}
	}

	public override string PresentationName {
		get {
			return "Stop";
		}
	}
	
	#endregion
	
	#region Ability abstract members
	
	public override bool InProgress {
		get {
			return this.inProgress;
		}
	}
	
	#endregion
	
	#region Overrides
	
	public override bool CanActivate ()
	{
		return this.MyActor.GetCurrentlyExecutingActiveAbilities().Count > 0;
	}
	
	public override void Activate ()
	{
		var currentAbilities = this.MyActor.GetCurrentlyExecutingActiveAbilities();
		foreach(var ability in currentAbilities)
			ability.Abort();
	}
	
	#endregion
	
	#region Data
	
	private bool inProgress = false;
	
	#endregion
}
