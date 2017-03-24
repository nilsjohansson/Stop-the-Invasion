using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveAbilityBasic : MoveAbility {
	
	#region Configuration
	
	public float TurnRadius = 0.5f;
	public float AnimationSpeedModifier = 0.75f;
	
	/// <summary>
	/// Sets the minimum distance for assuming that the final target has been reached.
	/// </summary>
	/// <value>
	/// The minimum distance.
	/// </value>
	public float MinimumProximity
	{
		get { return this.minimumProximity; }
		set { this.minimumProximity = value; }
	}
	
	public override bool InProgress {
		get { return this.MovingState == MovementState.Moving; }
	}
	
	#endregion
	
	#region Construction
	
	void Start () {
		MovementSpeedProperty msp = null;
		if(!this.MyActor.TryGetProperty<MovementSpeedProperty>(out msp))
			throw new UnityException("Could not get MovementSpeedProperty from actor: " + this.MyActor.PresentationName);
		this.movementSpeedProperty = msp.AsIProperty;
		
		this.navigationManager = WaypointNavigationManager.GetInstance();
		this.raycastMask = ScriptAssistant.CreateLayerMask(new int[] {8}, true);
		this.charController = GetComponent<CharacterController>();	
		this.charController.detectCollisions = true;
		this.animator = GetComponent<Animator>();
		
		// Make sure that layer 2's weight is 1
		if(animator.layerCount >= 2)
			animator.SetLayerWeight(1, 1);
	}
	
	#endregion
	
	#region Gizmos
	
	void OnDrawGizmos()
	{
		if(this.myPath != null && this.NextWayPointIndex > -1)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(this.transform.position, this.myPath[NextWayPointIndex].WorldPosition);
			for(int i = NextWayPointIndex; i < this.myPath.Count-1; i++)
			{
				Gizmos.DrawLine(this.myPath[i].WorldPosition, this.myPath[i+1].WorldPosition);
			}
		}
		Gizmos.DrawRay(obstacleRay);
	}
	
	#endregion
	
	#region ActiveAbility members
	
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
			return this.myPath != null && this.myPath.Count > 0 ? new Vector3[1] { this.myPath[this.myPath.Count -1].WorldPosition } : null;
		}
	}
	
	public override void Abort ()
	{
		base.Abort ();
		this.MovingState = MovementState.Stopped;
	}
	
	#endregion	
	
	#region Update
	
	void Update () {
		
		switch(this.MovingState)
		{
		case MovementState.Moving:
			Turn(this.isFirstStop);
			MoveTowardsNextPoint();
			break;
		case MovementState.Stopped:
			break;
		}
		
	}
	
	#endregion
	
	#region Operations
	
	private void Move(Vector3 positionOnGround)
	{
		this.myPath = new List<PathStop>();
		var newPath = navigationManager.FindShortestPathBetween(
			navigationManager.FindClosestWaypoint(this.transform.position),
			navigationManager.FindClosestWaypoint(positionOnGround));
		
		if(newPath == null)
		{
			// TODO: No path presentation
			Debug.Log("No path available!");
			this.FinishAbility(false);
			return;
		}
		
		for(int index = 0; index < newPath.Count; index++)
		{
			// Set an offset to each target to allow for a smooth turn
			if(index > 0 && index +1 < newPath.Count)
			{
				PathStop p = new PathStop(newPath[index].WorldPosition);
				myPath.Add(p);
			}
			else this.myPath.Add(new PathStop(newPath[index].WorldPosition));
		}
		// Final destination
		this.myPath.Add(new PathStop(positionOnGround));
		this.NextWayPointIndex = 0;
		this.MovingState = MovementState.Moving;
		isFirstStop = true;
	}
	
	private void Turn(bool forceTurn)
	{	
		if(NextWayPointIndex > this.myPath.Count -1) return;
		
		Vector3 enterDirection = this.transform.forward.normalized;
		Vector3 exitDirection = (this.myPath[NextWayPointIndex].WorldPosition - this.transform.position).normalized;
		
		float moveDistanceThisFrame = Time.deltaTime * this.movementSpeedProperty.PropertyValue;
		float moveAngleThisFrame = 0;
		
		if(NextWayPointIndex == 0 || forceTurn)
		{
			this.transform.rotation = Quaternion.LookRotation(Vector3.Scale(exitDirection, excludeYAxis), Vector3.up);
			return;
		}
		else 
		{	
			moveAngleThisFrame = moveDistanceThisFrame / (Mathf.PI * TurnRadius * 2f) * 360f;
			
		}
		
		this.transform.rotation = Quaternion.LookRotation(Vector3.Scale(Vector3.RotateTowards(enterDirection, exitDirection, moveAngleThisFrame * Mathf.Deg2Rad, 0f), excludeYAxis), Vector3.up);
	}
	
	private void MoveTowardsNextPoint()
	{
		if(NextWayPointIndex > -1)
		{
			// Move towards next node in path
			//this.transform.position += Time.deltaTime * this.MovementSpeed * this.transform.forward;
			this.charController.SimpleMove(this.movementSpeedProperty.PropertyValue * this.transform.forward);
			
			// Test for skipping routes in the path
			if(NextWayPointIndex+1 < myPath.Count)
			{
				for(int i = NextWayPointIndex+1; i<myPath.Count; i++)
				{
					int obstaclesInTheWay = DetectObstacles(myPath[i].TurnStartPosition);
					if(obstaclesInTheWay < 1) 
					{
						NextWayPointIndex = i;
						if(this.isFirstStop) Turn(true);
					} else if(obstaclesInTheWay == 1)
					{
						// If the obstacle lies at the target point, go anyway.
						// This means that there is nothing INBETWEEN the points, but that the character will touch the wall when reaching the target point
						if(DetectObstacleInTargetLocation(myPath[i].TurnStartPosition))
						{
							NextWayPointIndex = i;
							if(this.isFirstStop) Turn(true);
						}
					}
				}
			}
			
			// If character is Very close to the target:
			distanceToTarget = (Vector3.Scale(this.transform.position, this.excludeYAxis) 
			                    - Vector3.Scale(myPath[NextWayPointIndex].WorldPosition, this.excludeYAxis)).magnitude;
			
			if(this.targetActor != null)
				distanceToTarget -= (this.charController.radius * this.transform.lossyScale.x + this.targetActor.AttachedCharController.radius * this.targetActor.transform.lossyScale.x);
			
			if(distanceToTarget < this.MinimumProximity)
			{
				this.NextWayPointIndex++;
				if(this.NextWayPointIndex >= myPath.Count)
				{
					NextWayPointIndex = -1;
					this.MovingState = MovementState.Stopped;
					
					this.FinishAbility(true);
				} 
			} 
			
			this.isFirstStop = false;
		}
	}
	float distanceToTarget;
	
	/// <summary>
	/// Detects obstacles between character and its movement target
	/// </summary>
	/// <returns>
	/// True if there are obstacles between the character and the movement target
	/// </returns>
	/// <param name='targetPosition'>
	/// Raycast target position
	/// </param>
	/// <param name='thickRay'>
	/// If set to <c>true</c> casts a sphere the size of character. If <c>true</c> cast a single ray.
	/// </param>
	private int DetectObstacles(Vector3 targetPosition) // TODO: Optimize
	{
		int obstaclesInTheWay = 0;
		obstacleRay = new Ray(this.transform.position, targetPosition - this.transform.position);
		
		// sphereCastRadius = this.charController.radius * this.transform.localScale.x;
		sphereCastRadius = this.charController.radius * this.transform.localScale.x;
		// float sphereCastRadius = 0.4f;
		if(sphereCastRadius > 0)
		{
			var hits = Physics.SphereCastAll(obstacleRay, sphereCastRadius, Vector3.Distance(targetPosition, this.transform.position), raycastMask);
			obstaclesInTheWay = hits.Length;
		}
		else
		{
			obstaclesInTheWay = Physics.RaycastAll(obstacleRay, Vector3.Distance(targetPosition, this.transform.position), raycastMask).Length;
		}
		
		return obstaclesInTheWay;
	}
	Ray obstacleRay = default(Ray);
	float sphereCastRadius;
	private bool DetectObstacleInTargetLocation(Vector3 targetPosition)
	{
		bool isSpaceFree = false;
		isSpaceFree = Physics.CheckSphere(targetPosition, this.charController.radius * this.transform.localScale.x, raycastMask);
		return isSpaceFree;
	}
	
	#endregion
	
	#region Constants
	
	private const float GroundOffset = 0.1f;
	private const float DefaultMinimumProximity = 0.05f;
	
	#endregion
	
	#region Data
	
	private float minimumProximity = 0.05f;
	
	public enum MovementState
	{
		Stopped,
		Moving,
	}
	
	private Animator animator;
	
	private MovementState myStateMember = MovementState.Stopped;
	private int NextWayPointIndex 
	{
		get { return this.nextWpIndex; }
		set 
		{
			this.nextWpIndex = value; 
		}
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
				this.MinimumProximity = DefaultMinimumProximity;
				this.movementSpeedProperty.ResetToDefault(this.MyActor);
				this.targetActor = null;
				this.animator.SetBool("Walk", false);
			}
		}
	}
	
	private Actor targetActor = null;
	private CharacterController charController;
	private IMovementSpeedProperty movementSpeedProperty = null;
	private Vector3 excludeYAxis = new Vector3(1,0,1);
	
	private bool isFirstStop = false;
	private int raycastMask = 0;
	private List<PathStop> myPath = null;
	private int nextWpIndex = -1;
	private WaypointNavigationManager navigationManager = null;
	
	#endregion
	
	#region Private classes
	
	internal class PathStop
	{
		#region Properties
		
		public Vector3 TurnFinalPosition = Vector3.zero;
		public Vector3 WorldPosition = Vector3.zero;
		public Vector3 TurnStartPosition = Vector3.zero;
		
		#endregion
		
		#region Construction
		
		public PathStop ()
		{
			
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="MovementTester.PathStop"/> class.
		/// </summary>
		/// <param name='worldPosition'>
		/// World position. TurnStartPosition will also be set to this value.
		/// </param>
		public PathStop (Vector3 worldPosition)
		{
			WorldPosition = worldPosition;
			TurnStartPosition = worldPosition;
			TurnFinalPosition = worldPosition;
		}
		public PathStop (Vector3 worldPosition, Vector3 turnStartPosition, Vector3 turnFinalPosition)
		{
			TurnFinalPosition = turnFinalPosition;
			WorldPosition = worldPosition;
			TurnStartPosition = turnStartPosition;
		}
		
		#endregion
	}
	#endregion
}
