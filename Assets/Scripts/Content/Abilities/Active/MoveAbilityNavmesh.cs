using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveAbilityNavmesh : MoveAbility {
	#region Configuration

	#endregion
	
	#region Construction

	void Start () 
	{
		this.callbackManager = Manager.GetInstance<ConditionalCallbackManager>() as IConditionalCallback;
		this.navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		this.animator = GetComponent<Animator>();

		if(this.animator.applyRootMotion)
		{
			this.navMeshAgent.updatePosition = true;
		}
		this.navMeshAgent.updateRotation = true;
	}
	
	#endregion
	
	#region Properties
	
	public override bool InProgress {
		get { return this.MovingState == MovementState.Moving; }
	}

	#endregion
	
	#region Operations
	
	public override string PresentationName
	{
		get { return "Move"; }
	}
	
	public override InputType RequiredInput {
		get {
			return InputType.Positions;
		}
	}
	
	public override bool CanActivate ()
	{
		return true;
	}
	
	public override void Activate (Vector3[] positions)
	{
		if(positions.Length == 0)
			return;
		
		this.targetActor = null;
		base.Activate();
		this.Move(positions[0]);
	}
	
	public override void Activate (Actor[] actors)
	{
		base.Activate();
		this.targetActor = actors[0];
		this.Move(actors[0].NavigationPosition);
	}
	
	public override Actor[] TargetedActors {
		get {
			return this.targetActor == null ? null : new Actor[1] { this.targetActor };
		}
	}
	
	public override Vector3[] TargetedPositions {
		get {
			return this.MovingState == MovementState.Moving ? new Vector3[1] { this.navMeshAgent.destination } : null;
		}
	}
	
	public override void Abort ()
	{
		base.Abort ();
		this.navMeshAgent.Stop(true);
		this.MovingState = MovementState.Stopped;
		this.callbackManager.UnRegisterCallback(this.callbackId);
	}

	#endregion
	
	#region Implementation

	private void Move(Vector3 moveTarget)
	{
		this.navMeshAgent.SetDestination(moveTarget);
		this.MovingState = MovementState.Moving;
		this.callbackId = this.callbackManager.RegisterCallback(this.AgentDone, this.CompleteMovement);
	}

	private bool AgentDone()
	{
		if(this.navMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
			return true;
		return !this.navMeshAgent.pathPending && this.navMeshAgent.remainingDistance <= this.navMeshAgent.stoppingDistance;
	}

	private void CompleteMovement()
	{
		this.MovingState = MovementState.Stopped;
		this.FinishAbility(true);
	}

	#endregion

	#region Update

	#endregion

	#region Event handler
	
	#endregion

	#region State

	public enum MovementState
	{
		Stopped,
		Moving,
	}

	private MovementState MovingState
	{
		get { return this.myStateMember; }
		set 
		{ 
			this.myStateMember = value;
			if(value == MovementState.Moving) 
			{
				this.animator.SetBool("Walk", true);
			} 
			else 
			{
				this.targetActor = null;
				this.animator.SetBool("Walk", false);
			}
		}
	}

	#endregion

	#region Data

	private MovementState myStateMember;
	private UnityEngine.AI.NavMeshAgent navMeshAgent = null;
	private IConditionalCallback callbackManager = null;
	private Actor targetActor = null;
	private Animator animator = null;
	// private IMovementSpeedProperty movementSpeedProperty = null;
	private int callbackId = -1;

	#endregion
}
