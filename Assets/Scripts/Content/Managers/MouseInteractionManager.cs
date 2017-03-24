using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseInteractionManager : Manager {
	
	#region Properties
	
	public GameObject TheGround;
	public GameObject MovementMarker;
	
	#endregion

	#region Construction
	
	void Start () {
		Init(this);
		this.textGUIManager = (ITextGUIManager)Manager.GetInstance<TextGUIManager>();
	}
	
	#endregion
	
	#region Update
	
	void Update () {
		if(Input.GetMouseButtonDown(0))
		{
	    	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit = new RaycastHit();
	    	if(Physics.Raycast(ray, out hit))
			{
				switch(this.inputState)
				{
					case InputState.NoState:
						this.DetectActor(hit);
						break;
					case InputState.HasActor:
						// Ability menu is shown, if user doesnt click a menu item it will be closed in the next frame.
						this.inputState = InputState.AbortAbilitySelection;
						Debug.Log("User clicked somewhere with an actor selected!");
						break;
					case InputState.AbortAbilitySelection:
						this.selectedActor = null;
						this.selectedAbility = null;
						this.inputState = InputState.NoState;
						Debug.Log("User cancelled item selection");
						break;
					case InputState.HasSelectedAbility:
						this.TryExecuteAbility(hit);
						break;
					default:
						break;
				}
			}
			
		}
	}
	
	#endregion
	
	#region GUI
	
	void OnGUI()
	{
		if(this.inputState == InputState.NoState || this.inputState == InputState.HasSelectedAbility)
			return;
		
		var actorsAbilities = (List<ActiveAbility>) selectedActor.GetActiveAbilities();
		if(actorsAbilities.Count == 0) 
			return;
		
		Vector2 actorPositionOnScreen = Camera.main.WorldToScreenPoint(selectedActor.gameObject.transform.position);
		actorPositionOnScreen.y = Screen.height - actorPositionOnScreen.y;
		var angleDisplacement = 360f / (float) actorsAbilities.Count;
		for(int i = 0; i < actorsAbilities.Count; i++)
		{
			if(!actorsAbilities[i].CanActivate())
				continue;
			
			var radius = 35f;
			var buttonPosition = actorPositionOnScreen +
				radius * new Vector2(Mathf.Cos(angleDisplacement * i * Mathf.Deg2Rad) * 2f - 1f, Mathf.Sin(angleDisplacement * i * Mathf.Deg2Rad) * 2f -1f);
			
			Rect btnRect = new Rect();
			btnRect.center = buttonPosition;
			btnRect.width = 100;
			btnRect.height = 25;
			
			// Display abilities in a circle of buttons around the Actor
			if(this.textGUIManager.GUIButton(btnRect, actorsAbilities[i].PresentationName))
			{
				this.selectedAbility = actorsAbilities[i];
				this.inputState = InputState.HasSelectedAbility;
				Debug.Log("User selected ability: "+actorsAbilities[i].PresentationName);
				
				// Case: Ability requires no additional input.
				if(this.selectedAbility.RequiredInput == ActiveAbility.InputType.None) 
					this.TryExecuteAbility(default(RaycastHit));
			}
		}
	}
	#endregion
	
	#region Operations
	
	private void DetectActor(RaycastHit hit)
	{
		var potentialActor = hit.collider.gameObject.GetComponent<Actor>();
		if(potentialActor == null)
			return;
		
		if(potentialActor.ActiveAbilityCount == 0)
			return;
		
		this.selectedActor = potentialActor;
		this.inputState = InputState.HasActor;
		Debug.Log("User selected actor: "+potentialActor.PresentationName);
	}
	
	private void TryExecuteAbility(RaycastHit hit)
	{
		switch(this.selectedAbility.RequiredInput)
		{
			case ActiveAbility.InputType.None:
				if(!this.selectedAbility.CanActivate())
				{
					this.inputState = InputState.NoState; 
					return;
				}
				
				this.selectedAbility.RegisterFinishedCallback(this.MoveFinished);
				this.selectedAbility.Activate(); 
				MarkPosition(hit.point);
				Debug.Log("User activated ability! Input type: None");
			break;
			
			case ActiveAbility.InputType.OtherActors:
				var potentialActor = hit.collider.gameObject.GetComponent<Actor>();
				if(potentialActor == null)
					return;
				
				if(!this.selectedAbility.CanActivate(new Actor[1] { potentialActor }))
				{
					this.inputState = InputState.NoState; 
					return;
				}
				
				this.selectedAbility.RegisterFinishedCallback(this.MoveFinished);
				this.selectedAbility.Activate(new Actor[1] { potentialActor }); 
				MarkPosition(potentialActor.NavigationPosition);
				Debug.Log("User activated ability! Input type: Actor");
			break;
			
			case ActiveAbility.InputType.Positions:
				if(hit.collider.gameObject != TheGround)
					return;
				
				if(!this.selectedAbility.CanActivate(new Vector3[1] { hit.point }))
				{
					this.inputState = InputState.NoState; 
					return;
				}
				
				this.selectedAbility.RegisterFinishedCallback(this.MoveFinished);
				this.selectedAbility.Activate(new Vector3[1] { hit.point }); 
				MarkPosition(hit.point);
				Debug.Log("User activated ability! Input type: Positions");
			break;
			
		default:
			return;
		}
		
		this.selectedAbility = null;
		this.selectedActor = null;
		this.inputState = InputState.NoState;
	}
	
	private void MarkPosition(Vector3 position)
	{
		if(myMarker == null) 
		{
			myMarker = Instantiate(MovementMarker) as GameObject;
		}
	
		myMarker.transform.position = position + new Vector3(0, 0.08f, 0);
		myMarker.transform.rotation = Quaternion.identity;
	}
	
	#endregion
	
	#region Callbacks
	
	private void MoveFinished(bool success, ActiveAbility ability)
	{
		GameObject.Destroy(this.myMarker);
	}
	
	#endregion
	
	#region State
	
	private enum InputState
	{
		// Waiting for an actor to be selected
		NoState,
		// Actor is selected, waiting for user to select an ability 
		HasActor,
		// Return to no state unless user selects an ability
		AbortAbilitySelection,
		// Ability is selected, waiting for direction input
		HasSelectedAbility
	}
	
	private InputState inputState = InputState.NoState;
	
	#endregion
	
	#region Data
	
	private GameObject myMarker = null;
	private Actor selectedActor = null;
	private ActiveAbility selectedAbility = null;
	private ITextGUIManager textGUIManager = null;
	
	#endregion
}
