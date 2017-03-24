using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchInteractionManager : Manager {
	
	#region Configuration
	
	public GameObject TheGround;
	public GameObject MovementMarker;
	public GameObject TargetMarker;
	public float CircularMenuRadius = 60f;
	public float InnerLimitSelectionDistance = 40f;
	public float OuterLimitSelectionDistance = 150f;
	
	#endregion

	#region Construction

	void Awake()
	{
		Init (this);
	}

	void Start () {
		this.textGUIManager = (ITextGUIManager)Manager.GetInstance<TextGUIManager>();
		var gameManager = Manager.GetInstance<GameManager>();
		gameManager.OnGameOver += delegate(object sender, GameManager.GameOverEventArgs e) 
			{
				this.selectedActor = null;
				this.selectedAbility = null;
				this.inputState = InputState.Disabled;
			};
		this.gestureManager = Manager.GetInstance<GestureManager>();
		this.CircularMenuRadius *= (float)Screen.height / 580f;
		this.InnerLimitSelectionDistance *= (float)Screen.height / 580f;
		this.OuterLimitSelectionDistance *= (float)Screen.height / 580f;
	}
	
	#endregion

	#region Update
	
	void Update () {
		switch(this.inputState)
		{
			case InputState.NoState:
				this.DetectActor();
				break;
			case InputState.SelectingAbility:
				this.SelectAbility();
				break;
			case InputState.HasSelectedAbility:
				this.TryExecuteAbility();
				break;
			case InputState.HasExecutedAbility:
				if(this.gestureManager.ReleasedSingleTouch()) this.inputState = InputState.NoState;
				break;
			default:
				break;
		}
	}
	
	#endregion
	
	#region GUI
	
	void OnGUI()
	{
		if(this.inputState != InputState.SelectingAbility || this.circularMenuItems == null)
			return;
		
		// Display abilities in a circle of buttons around the Actor
		foreach(var menuItem in this.circularMenuItems.Values)
		{
			Rect btnRect = new Rect();
			btnRect.center = menuItem.GuiPosition;
			btnRect.width = 0;
			btnRect.height = 0;
			/*
			Rect r = new Rect();
			r.center = Camera.main.WorldToScreenPoint(selectedActor.NavigationPosition + new Vector3(0f, selectedActor.Height * 0.625f, 0f));
			r.y = Screen.height - r.y;
			this.textGUIManager.GUIDisplayText(r, "X");
			*/
			if(menuItem.Highlight)
			{
				this.textGUIManager.GUIDisplayText(btnRect, menuItem.RepresentedAbility.PresentationName, Color.white, Color.yellow, 50);
			} 
			else if(!menuItem.CanActivate)
			{
				this.textGUIManager.GUIDisplayText(btnRect, menuItem.RepresentedAbility.PresentationName, Color.gray, new Color(0.2f, 0.2f, 0.2f, 1.0f));
			}
			else
			{
				this.textGUIManager.GUIDisplayText(btnRect, menuItem.RepresentedAbility.PresentationName, Color.white, Color.black, 35);
			}
		}
		
	}
	
	#endregion
	
	#region Operations

	private void CalculateGUI()
	{
		if(!this.gestureManager.GetSingleTouchPosition().HasValue)
			return;

		if(this.circularMenuItems == null)
			this.circularMenuItems = new Dictionary<ActiveAbility, CircularMenuItem>();
		
		var actorsAbilities = selectedActor.GetActiveAbilities();

		Vector2 mousePosition = this.gestureManager.GetSingleTouchPosition().Value;
		mousePosition.y = Screen.height - mousePosition.y;
		var minDistance = Screen.width*2f;
		
		Vector2 actorPositionOnScreen = Camera.main.WorldToScreenPoint(selectedActor.NavigationPosition + new Vector3(0f, selectedActor.Height * 0.625f, 0f));
		actorPositionOnScreen.y = Screen.height - actorPositionOnScreen.y;
		var angleDisplacement = 360f / (float) actorsAbilities.Count;
		var currentDistance = minDistance;
		
		float limitDistance = Vector2.Distance(mousePosition, actorPositionOnScreen);
		for(int i = 0; i < actorsAbilities.Count; i++)
		{
			var radius = this.CircularMenuRadius;
			var buttonAngle = angleDisplacement * i;
			var buttonPosition = actorPositionOnScreen +
				radius * new Vector2(Mathf.Cos(buttonAngle * Mathf.Deg2Rad), Mathf.Sin(buttonAngle * Mathf.Deg2Rad));
			
			currentDistance = Vector2.Distance(mousePosition, buttonPosition);
			if(currentDistance < minDistance && actorsAbilities[i].CanActivate())
			{
				this.hoveredAbility = actorsAbilities[i];
				minDistance = currentDistance;
			}

			if(limitDistance < this.InnerLimitSelectionDistance || limitDistance > this.OuterLimitSelectionDistance)
			{
				this.hoveredAbility = null;
			}

			this.circularMenuItems[actorsAbilities[i]] = new CircularMenuItem(actorsAbilities[i], buttonPosition, false, actorsAbilities[i].CanActivate());
		}
		
		if(hoveredAbility != null && limitDistance > this.InnerLimitSelectionDistance && limitDistance < this.OuterLimitSelectionDistance)
			this.circularMenuItems[hoveredAbility].Highlight = true;
	}
	
	private RaycastHit? HitTestUnderPointer(ActiveAbility.InputType requiredInput)
	{
		if(this.gestureManager.CurrentTouchState == GestureManager.TouchState.SingleTouch)
		{
			Ray ray = Camera.main.ScreenPointToRay(this.gestureManager.GetSingleTouchPosition().Value);
			RaycastHit hit = new RaycastHit();
			
			if(requiredInput == ActiveAbility.InputType.Positions)
			{
				// Only shoot rays at the ground
				if(Physics.Raycast(ray, out hit, 1000f, ScriptAssistant.CreateLayerMask(new int[]{ TheGround.layer }, false)))
					return hit;
			}
			else
			// Shoot rays at anything
	    	if(Physics.Raycast(ray, out hit))
				return hit;
		}

		return null;
	}
	
	private void SelectAbility()
	{
		CalculateGUI();
		
		if(this.gestureManager.ReleasedSingleTouch())
		{
			// Hovered ability is set in CalculateGUI
			if(this.hoveredAbility == null)
			{
				this.inputState = InputState.NoState;
				this.selectedAbility = null;
				this.selectedActor = null;
				return;
			}
			
			this.selectedAbility = hoveredAbility;
			this.inputState = InputState.HasSelectedAbility;

			// Case: Ability requires no additional input.
			if(this.selectedAbility.RequiredInput == ActiveAbility.InputType.None) 
				this.TryExecuteAbility();
		}
	}
	
	private void DetectActor()
	{
		var hit = HitTestUnderPointer(ActiveAbility.InputType.OtherActors);
		if(!hit.HasValue) 
			return;
		
		var potentialActor = hit.Value.collider.gameObject.GetComponent<Actor>();
		if(potentialActor == null)
			return;

		if(potentialActor.Faction != (Manager.GetInstance<GameManager>() as IGameManager).PlayerFaction)
			return;

		if(potentialActor.ActiveAbilityCount == 0)
			return;
		
		this.selectedActor = potentialActor;
		this.selectedAbility = null;

		this.inputState = InputState.SelectingAbility;
	}
	
	private void TryExecuteAbility()
	{
		RaycastHit? hit = null;
		switch(this.selectedAbility.RequiredInput)
		{
			case ActiveAbility.InputType.None:
				if(!this.selectedAbility.CanActivate())
				{
					this.inputState = InputState.NoState; 
					return;
				}
			
				Destroy (this.actorMarker);
				Destroy (this.targetMarker);
				this.selectedAbility.RegisterFinishedCallback(this.MoveFinished);
				this.selectedAbility.Activate(); 
			break;
			
			case ActiveAbility.InputType.OtherActors:
				hit = HitTestUnderPointer(ActiveAbility.InputType.OtherActors);
				if(!hit.HasValue) 
				{
					return;
				}
		
				var potentialActor = hit.Value.collider.gameObject.GetComponent<Actor>();
				if(potentialActor == null)
				{
					this.inputState = InputState.NoState; 
					return;
				}
				
				if(!this.selectedAbility.CanActivate(new Actor[1] { potentialActor }))
				{
					this.inputState = InputState.NoState; 
					return;
				}
				
				// Select the right type of marker
				this.targetMarkerPrefab = potentialActor.Faction == this.selectedActor.Faction || potentialActor.Faction == FactionType.Neutral ? MovementMarker : TargetMarker;
				Destroy (this.targetMarker);
				this.targetMarker = (GameObject) Instantiate(this.targetMarkerPrefab);
				this.targetMarker.GetComponent<IEffect>().PlayEffect(potentialActor);
				
				Destroy (this.actorMarker);
				this.selectedAbility.RegisterFinishedCallback(this.MoveFinished);
				this.selectedAbility.Activate(new Actor[1] { potentialActor }); 
			break;
			
			case ActiveAbility.InputType.Positions:
				hit = HitTestUnderPointer(ActiveAbility.InputType.Positions);
				if(!hit.HasValue) 
				{
					return;
				}
		
				if(hit.Value.collider.gameObject != TheGround)
				{
					this.inputState = InputState.NoState; 
					return;
				}
				
				if(!this.selectedAbility.CanActivate(new Vector3[1] { hit.Value.point }))
				{
					this.inputState = InputState.NoState; 
					return;
				}

				Destroy (this.targetMarker);
				this.targetMarker = (GameObject) Instantiate(this.MovementMarker);
				this.targetMarker.GetComponent<IEffect>().PlayEffect(hit.Value.point);
			
				Destroy (this.actorMarker);
				this.selectedAbility.RegisterFinishedCallback(this.MoveFinished);
				this.selectedAbility.Activate(new Vector3[1] { hit.Value.point }); 
			break;
			
		default:
			return;
		}
		
		this.circularMenuItems = null;
		this.inputState = InputState.HasExecutedAbility;
	}

	#endregion
	
	#region Callbacks
	
	private void MoveFinished(bool success, ActiveAbility ability)
	{
		Destroy (this.targetMarker);
	}
	
	#endregion
	
	#region State
	
	private enum InputState
	{
		// Waiting for an actor to be selected
		NoState,
		// Actor is selected, waiting for user to select an ability 
		SelectingAbility,
		// Ability is selected, waiting for direction input
		HasSelectedAbility,
		// Wait for the user to release the touch
		HasExecutedAbility,
		// Nothing happens during this state
		Disabled
	}
	
	private InputState inputState
	{
		get { return this.inputStateData; }
		set 
		{ 
			if(value == InputState.NoState) this.circularMenuItems = null;
			this.inputStateData = value; 
		}
	}

	private Actor selectedActor
	{
		get { return this.selectedActorData; }
		set 
		{
			if(value == this.selectedActorData)
				return;
			// If the actor is changed or removed, clear the actor marker
			DestroyImmediate(this.actorMarker);

			if(value != null)
			{
				HealthProperty actorHp = null;
				if(value.AsIActor.TryGetProperty<HealthProperty>(out actorHp))
					actorHp.AsIProperty.OnValueChanged += delegate(object sender, Property.PropertyEventArgs e) {
						if(((HealthProperty.HealthInfo)e.NewValue).CurrentHealth == 0f)
							DestroyImmediate(this.targetMarker);
					};

				// If a new actor is selected, first set up a new actor marker around it
				this.actorMarker = (GameObject)Instantiate(this.MovementMarker);
				this.actorMarker.GetComponent<IEffect>().PlayEffect(value);

				// .. and clear the previous target marker
				DestroyImmediate(this.targetMarker);

				// See if the actor is doing something
				if(value.GetCurrentlyExecutingActiveAbilities().Count > 0)
				{
					var currentAbility = value.GetCurrentlyExecutingActiveAbilities()[0];
					if(currentAbility.RequiredInput == ActiveAbility.InputType.OtherActors)
					{
						this.targetMarkerPrefab = currentAbility.TargetedActors[0].Faction == value.Faction || currentAbility.TargetedActors[0].Faction == FactionType.Neutral ? MovementMarker : TargetMarker;
						this.targetMarker = (GameObject) Instantiate(this.targetMarkerPrefab);
						this.targetMarker.GetComponent<IEffect>().PlayEffect(currentAbility.TargetedActors[0]);
					}
					else if(currentAbility.RequiredInput == ActiveAbility.InputType.Positions)
					{
						this.targetMarker = (GameObject) Instantiate(this.MovementMarker);
						this.targetMarker.GetComponent<IEffect>().PlayEffect(currentAbility.TargetedPositions[0]);
					}

				}
			}
			this.selectedActorData = value; 
		}
	}

	#endregion
	
	#region Data

	private InputState inputStateData = InputState.NoState;
	private GameObject actorMarker = null;
	private GameObject targetMarker = null;
	private GameObject targetMarkerPrefab = null;

	private Actor selectedActorData = null;
	private ActiveAbility selectedAbility = null;

	private ActiveAbility hoveredAbility = null;
	private ITextGUIManager textGUIManager = null;
	private GestureManager gestureManager = null;

	private Dictionary<ActiveAbility, CircularMenuItem> circularMenuItems = null;
	
	#endregion
	
	#region Private classes
	
	private class CircularMenuItem
	{
		public CircularMenuItem (ActiveAbility ability, Vector2 guiPosition, bool highlight, bool canActivate)
		{
			this.CanActivate = canActivate;
			this.RepresentedAbility = ability;
			this.GuiPosition = guiPosition;
			this.Highlight = highlight;
		}
		
		public ActiveAbility RepresentedAbility { get; set; }
		public Vector2 GuiPosition { get; set; }
		public bool Highlight { get; set; }
		public bool CanActivate { get; set; }
	}
	
	#endregion
}
