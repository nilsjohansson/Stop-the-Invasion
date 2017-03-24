using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MoveAbility))]
public class GatherAbility : ActiveAbility
{
	#region Configuration
	
	public GameObject handPositions = null;
	
	#endregion
	
	#region Construction
	
	void Start ()
	{
		MovementSpeedProperty msp = null;
		if(!this.MyActor.TryGetProperty<MovementSpeedProperty>(out msp))
			throw new UnityException("Could not get MovementSpeedProperty from actor: " + this.MyActor.PresentationName);
		this.movementSpeedProperty = msp.AsIProperty;

		this.animator = GetComponent<Animator> ();
		this.moveAbilityRef = GetComponent<MoveAbility> ();
		var allBases = ScriptAssistant.GetAllInstancesWithType<MainBaseActor>();
		if(allBases != null)
		{
			FactionProperty actorfp = null;
			this.MyActor.TryGetProperty<FactionProperty>(out actorfp);

			FactionProperty basefp = null;
			foreach(var aBase in allBases)
			{
				if(aBase.TryGetProperty<FactionProperty>(out basefp))
					if(basefp.PropertyValue == actorfp.PropertyValue)
						{
							this.mainBaseActor = aBase;
							break;
						}
			}	
		}

		if(this.mainBaseActor == null) 
			Debug.LogWarning("No MainBaseActor present in scene!");
	}
	
	#endregion
	
	#region ActiveAbility abstract members
	
	public override InputType RequiredInput {
		get {
			return this.currentRequiredInput; // TODO: Ska vara Actors
		}
	}
	
	public override string PresentationName {
		get {
			return "Gather";
		}
	}

	public override Actor[] TargetedActors {
		get {
			return new Actor[1] { this.targetedMaterials };
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
		return this.mainBaseActor != null;
	}

	public override bool CanActivate (Vector3[] positions)
	{
		return this.currentRequiredInput == ActiveAbility.InputType.Positions && this.mainBaseActor != null;
	}
	
	public override void Activate (Vector3[] positions)
	{
		base.Activate(positions);
		// Move back
		this.moveAbilityRef.RegisterFinishedCallback(this.ReturnedToBaseWithMaterialsCallback);
		this.movementSpeedProperty.SetMovementSpeed(this.movementSpeedProperty.PropertyValue * 0.8f, this.MyActor);
		this.moveAbilityRef.Activate(new Vector3[] { positions[0] });
		//this.animator.SetBool("Carry", true);
		this.animator.SetTrigger("Carry");

		this.gatherState = GatherState.ReturningWithMaterials;
	}
	
	public override bool CanActivate (Actor[] actors)
	{
		return actors[0] is AncientCubeActor && this.mainBaseActor != null;
	}
	
	public override void Activate (Actor[] actors)
	{
		base.Activate(actors);
		this.mainBaseLocation = this.mainBaseActor.NavigationPosition;
		
		this.targetedMaterials = actors[0];
		this.moveAbilityRef.RegisterFinishedCallback (this.ArrivedAtMaterialsCallback);
		this.moveAbilityRef.Activate (new Vector3[] { actors [0].NavigationPosition });
		this.gatherState = GatherState.HeadingToMaterialsPatch;
		this.inProgress = true;
	}

	public override void Abort ()
	{
		base.Abort ();
		this.gatherStateData = GatherState.StoppedWithoutMaterial;

		this.animator.SetTrigger("Drop");
		// this.animator.SetBool("Carry", false);
		//this.animator.SetBool("PickUp", false);
		this.targetedMaterials.GetComponent<Rigidbody>().isKinematic = false;
		this.targetedMaterials.GetComponent<Collider>().isTrigger = false;
		this.inProgress = false;

		// Reset physics properties of dropped material
		Manager.GetInstance<ConditionalCallbackManager>().RegisterDelayedCallback(2f, this.ResetDroppedMaterial);
	}

	#endregion
	
	#region Update
	
	void Update ()
	{
		AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(1);			

		switch (this.gatherState) 
		{
		case GatherState.HeadingToMaterialsPatch:
			break;
		case GatherState.BendingDown:
			if(animState.IsName("Next level.PickUp") && animState.normalizedTime >= 0.7f)
			{
				this.targetedMaterials.transform.parent = this.handPositions.transform;
				this.gatherState = GatherState.PickingUp;
			}
			
			break;
		case GatherState.PickingUp:
			
			if(animState.normalizedTime >= 1.0f)
			{
				// Move back
				this.moveAbilityRef.RegisterFinishedCallback(this.ReturnedToBaseWithMaterialsCallback);
				this.movementSpeedProperty.SetMovementSpeed(this.movementSpeedProperty.PropertyValue * 0.8f, this.MyActor);
				this.moveAbilityRef.Activate(new Vector3[] { this.mainBaseLocation });
				// this.animator.SetBool("Carry", true);
				this.animator.SetTrigger("Carry");

				this.gatherState = GatherState.ReturningWithMaterials;
			}
			break;
		case GatherState.ReturningWithMaterials:
			break;
		case GatherState.PuttingDown:
			if(animState.normalizedTime <= 0.25f)
			{
				this.targetedMaterials.transform.parent = null;
				
				this.targetedMaterials.transform.position = 
					ScriptAssistant.GetGroundPosition(this.transform.position, Manager.GetInstance<TouchInteractionManager>().TheGround).Value 
						+ 0.2f * Vector3.up + 0.2f * this.transform.forward;

				// this.animator.SetBool("PickUp", false);
				this.gatherState = GatherState.StandingUp;
				// Don't deliver if the ability was aborted
				if(this.inProgress) 
					this.DeliverMaterials();
			} 
			
			break;
		case GatherState.StandingUp:
			this.gatherState = GatherState.StoppedWithoutMaterial;
			FinishAbility(this.inProgress);
			this.inProgress = false;
			
			break;
		case GatherState.StoppedWithMaterial:
			break;
		case GatherState.StoppedWithoutMaterial:
			break;
		}
	}
	
	#endregion
	
	#region Implementation

	private void ResetDroppedMaterial()
	{
		this.targetedMaterials.GetComponent<Rigidbody>().isKinematic = true;
		this.targetedMaterials.GetComponent<Collider>().isTrigger = true;
		this.targetedMaterials = null;
	}

	private void DeliverMaterials()
	{
		MineralsProperty mainBaseMinerals = null; 
		if(this.mainBaseActor.TryGetProperty<MineralsProperty>(out mainBaseMinerals))
			mainBaseMinerals.IncreaseMinerals(1, this.MyActor);
	}

	#endregion

	#region Callbacks
	
	private void ArrivedAtMaterialsCallback (bool finishedSuccessfully, ActiveAbility ability)
	{
		if (!finishedSuccessfully)
		{
			this.inProgress = false;
			this.gatherState = GatherState.StoppedWithoutMaterial;
			FinishAbility(false);
			return;
		}

		// this.animator.SetBool ("PickUp", true);
		this.animator.SetTrigger("PickUp");
		this.gatherState = GatherState.BendingDown;
	}
	
	private void ReturnedToBaseWithMaterialsCallback(bool finishedSuccessfully, ActiveAbility ability)
	{
		/*
		if(!finishedSuccessfully)
		{
			this.currentRequiredInput = ActiveAbility.InputType.Positions;
			this.inProgress = false;
			this.gatherState = GatherState.StoppedWithMaterial;
			this.animator.SetBool("Carry", false);
			FinishAbility(false);
			return;
		}
		*/
		this.currentRequiredInput = ActiveAbility.InputType.OtherActors;
		if(this.InProgress)
			this.animator.SetTrigger("PutDown");
		this.gatherState = GatherState.PuttingDown;
	}
	
	#endregion
	
	#region State
	
	private enum GatherState
	{
		HeadingToMaterialsPatch,
		BendingDown,
		PickingUp,
		ReturningWithMaterials,
		PuttingDown,
		StandingUp,
		StoppedWithMaterial,
		StoppedWithoutMaterial
	}
	
	#endregion
	
	#region Data
	
	private GatherState gatherState {
		get { return this.gatherStateData; }
		set { this.gatherStateData = value; }
	}

	private IMovementSpeedProperty movementSpeedProperty = null;
	private GatherState gatherStateData = GatherState.StoppedWithoutMaterial;
	private bool inProgress = false;
	private Animator animator = null;
	private MoveAbility moveAbilityRef = null;
	private Actor targetedMaterials = null;
	private Vector3 mainBaseLocation = Vector3.zero;
	
	private MainBaseActor mainBaseActor = null;
	
	private InputType currentRequiredInput = ActiveAbility.InputType.OtherActors;
	
	#endregion
}
