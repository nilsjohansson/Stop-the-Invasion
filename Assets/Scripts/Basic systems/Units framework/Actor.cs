using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IActor
{
	/// <summary>
	/// Gets all active abilities from this <see cref="Actor"/>.
	/// </summary>
	/// <returns>The active abilities.</returns>
	List<ActiveAbility> GetActiveAbilities();

	/// <summary>
	/// Gets the name of this <see cref="Actor"/>. Used for presentation.
	/// </summary>
	/// <value>The name of the actor.</value>
	string PresentationName { get; }

	/// <summary>
	/// Gets this <see cref="Actor"/>'s navigation position on the ground.
	/// </summary>
	/// <value>The navigation position.</value>
	Vector3 NavigationPosition { get; }

	/// <summary>
	/// Gets the height of this <see cref="Actor"/>.
	/// </summary>
	/// <value>The height from the Base to the top of the Actor's mesh. Default is 0.</value>
	float Height { get; }

	/// <summary>
	/// Gets the width of this <see cref="Actor"/>.
	/// </summary>
	/// <value>The width (diameter) of this actor.</value>
	float Width { get; }

	/// <summary>
	/// The number of active abilities on this actor.
	/// </summary>
	/// <value>The active ability count.</value>
	int ActiveAbilityCount { get; }

	/// <summary>
	/// Tries to get a <see cref="Property"/> of a certain type.
	/// </summary>
	/// <returns><c>true</c>, if get property could be found, <c>false</c> otherwise.</returns>
	/// <param name="property">Property.</param>
	/// <typeparam name="T">The type of <see cref="Property"/>.</typeparam>
	bool TryGetProperty<T>(out T property);

	/// <summary>
	/// A list of currently executing <see cref="ActiveAbility"/>'s on actor. Use this for determining which abilities can activate.
	/// </summary>
	/// <returns>The currently executing active abilities.</returns>
	List<ActiveAbility> GetCurrentlyExecutingActiveAbilities();

	/// <summary>
	/// Tells the <see cref="Actor"/> that a certain ability is active. The actor registers a finished callback in the ability to know when the ability has finished.
	/// </summary>
	/// <param name="ability">Ability.</param>
	void SetCurrentAbility(ActiveAbility ability);
}

/// <summary>
/// Base class for all interactable units in game. 
/// E.g. Heroes, Enemies, Neutrals, Interactable objects (towers, ballistas etc).
/// Gathers all scripts in its children of type Ability && Property and publishes them to the world.
/// </summary>
public abstract class Actor : MonoBehaviour, IActor {
	#region Construction
	
	void Awake () {
		this.activeAbilities = new List<ActiveAbility>(transform.GetComponentsInChildren<ActiveAbility>(true));
		this.passiveAbilities = new List<PassiveAbility>(transform.GetComponentsInChildren<PassiveAbility>(true));
		this.properties = new List<Property>(transform.GetComponentsInChildren<Property>(true));
		foreach(var prop in this.properties)
		{
			prop.OnValueChanged += HandlePropertyValueChanged;	
		}
	}

	void Start ()
	{
		this.gameManager = (Manager.GetInstance<GameManager>() as IGameManager);
		this.gameManager.RegisterActor(this);
	}

	#endregion

	#region Delegates

	public delegate void PostAbortAction();

	#endregion

	#region Update

	void Update () {
		if(this.deactive)
		{
			foreach(var ability in this.currentlyActiveAbilities)
				ability.Abort();
			
			foreach(var ability in this.activeAbilities)
				ability.enabled = false;
			
			foreach(var ability in this.passiveAbilities)
				ability.enabled = false;
		}
	}
	
	#endregion
	
	#region Properties
	
	public abstract string PresentationName 
	{
		get;
	}
	
	public virtual Vector3 NavigationPosition
	{
		get
		{
			return this.transform.position;
		}
	}

	public virtual float Height
	{
		get
		{
			return 0f;
		}
	}

	public virtual float Width
	{
		get
		{
			return 0f;
		}
	}

	public int ActiveAbilityCount
	{
		get { return this.activeAbilities != null ? this.activeAbilities.Count : 0; }
	}

	public List<ActiveAbility> GetCurrentlyExecutingActiveAbilities()
	{
		return new List<ActiveAbility>(this.currentlyActiveAbilities); 
	}

	public IActor AsIActor 
	{
		get { return this as IActor; }
	}

	public CharacterController AttachedCharController
	{
		get 
		{
			if(this.attachedCharacterController == null)
				this.attachedCharacterController = this.GetComponent<CharacterController>();
			return this.attachedCharacterController;
		}
	}

	public FactionType Faction {
		get 
		{
			if(this.attachedFactionProperty == null)
				this.TryGetProperty<FactionProperty>(out this.attachedFactionProperty);
			return this.attachedFactionProperty == null ? FactionType.Neutral : this.attachedFactionProperty.PropertyValue;
		}
	}

	#endregion
	
	#region Public methods
	
	/// <summary>
	/// Gets the property value of the specified property type. Also works with IProperty interfaces.
	/// Use this to read property values between actors or from Abilities.
	/// If this <see cref="Actor"/> does not contain any property of type T, returns default(T).
	/// </summary>
	/// <returns>
	/// If it was successful or not.
	/// </returns>
	/// <param name='property'>
	/// The Property result.
	/// </param>
	/// <typeparam name='T'>
	/// The sought Property type.
	/// </typeparam>
	public bool TryGetProperty<T>(out T property)
	{
		if(this.properties == null) 
		{
			property = default(T);
			return false;
		}

		foreach(var prop in this.properties)
		{
			if(prop is T) 
			{
				property = (T)(object)prop;
				return true;
			}
		}
		
		property = default(T);
		return false;
	}
	
	public List<ActiveAbility> GetActiveAbilities()
	{
		return this.activeAbilities;
	}

	public void SetCurrentAbility(ActiveAbility ability)
	{
		this.currentlyActiveAbilities.Add(ability);
		ability.RegisterFinishedCallback(this.AbilityFinished);
	}

	public void Deactivate()
	{

	}

	#endregion
	
	#region Event handlers

	protected virtual void HandlePropertyValueChanged (object sender, Property.PropertyEventArgs e)
	{
		
	}
	
	#endregion

	#region Implementation

	void AbilityFinished (bool finishedSuccessfully, ActiveAbility ability)
	{
		this.currentlyActiveAbilities.Remove(ability);
		if(this.postAbortAllAction != null && this.currentlyActiveAbilities.Count == 0)
		{
			this.postAbortAllAction();
			this.postAbortAllAction = null;
		}
	}

	#endregion

	#region Data
	
	protected List<ActiveAbility> activeAbilities = null;
	protected List<PassiveAbility> passiveAbilities = null;
	protected List<Property> properties = null;

	private PostAbortAction postAbortAllAction = null;
	private List<ActiveAbility> currentlyActiveAbilities = new List<ActiveAbility>();
	private IGameManager gameManager = null;
	private CharacterController attachedCharacterController = null;
	private FactionProperty attachedFactionProperty = null;
	private bool deactive = false;

	#endregion
}
