using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IGameManager
{
	/// <summary>
	/// Registers a new actor. This is done when an <see cref="Actor"/> instance is started.
	/// </summary>
	/// <param name="actor">The instantiated actor.</param>
	void RegisterActor(Actor actor);

	/// <summary>
	/// Unregisters an actor. This is done when an <see cref="Actor"/> instance is destroyed.
	/// </summary>
	/// <param name="actor">The destroyed actor.</param>
	void UnRegisterActor(Actor actor);

	/// <summary>
	/// The Faction that is controlled by the player.
	/// </summary>
	/// <value>The player faction.</value>
	FactionType PlayerFaction { get; }

	/// <summary>
	/// All factions and actors that belong to them.
	/// </summary>
	Dictionary<FactionType, List<Actor>> Factions { get; }

	/// <summary>
	/// Occurs when the game is over.
	/// </summary>
	event System.EventHandler<GameManager.GameOverEventArgs> OnGameOver;

	/// <summary>
	/// Occurs when game is started and player can start interacting with the actors.
	/// </summary>
	event System.EventHandler OnStartGame;

	/// <summary>
	/// Occurs when an actor is created.
	/// </summary>
	event System.EventHandler<GameManager.ActorUpdatedEventArgs> OnActorCreated;

	/// <summary>
	/// Occurs when an actor is destroyed.
	/// </summary>
	event System.EventHandler<GameManager.ActorUpdatedEventArgs> OnActorDestroyed;
}

public class GameManager : Manager, IGameManager {
	#region Configuration

	public FactionType PlayersFaction = FactionType.Red;
	public WinCondition EndGameCondition = null;
	public Introduction LevelIntroduction = null;

	#endregion

	#region Construction

	void Awake ()
	{
		Init (this);
		
		this.currentEndGameCondition = this.EndGameCondition as IWinCondition;
		this.currentEndGameCondition.RegisterEndGameCallback(this.EndGame);
	}

	void Start () {

		if(this.EndGameCondition == null)
			throw new UnityException("No End game condition is set!");

		this.playersFaction = PlayerFaction;
		if(this.LevelIntroduction == null)
		{
			this.StartGame();
		}
		else
		{
			this.LevelIntroduction.RegisterStartGameCallback(this.StartGame);
			this.LevelIntroduction.BeginIntroduction();
		}
	}

	#endregion

	#region Events

	public event System.EventHandler<GameOverEventArgs> OnGameOver;
	public event System.EventHandler OnStartGame;
	public event System.EventHandler<ActorUpdatedEventArgs> OnActorCreated;
	public event System.EventHandler<ActorUpdatedEventArgs> OnActorDestroyed;

	#endregion

	#region Properties

	public FactionType PlayerFaction 
	{
		get { return this.playersFaction; }
	}

	public Dictionary<FactionType, List<Actor>> Factions
	{
		get { return this.actorCount; }
	}

	#endregion

	#region Public methods

	public void RegisterActor(Actor actor)
	{
		FactionType faction = FactionType.Neutral;
		FactionProperty fp = null;
		if(actor.TryGetProperty<FactionProperty>(out fp))
			faction = fp.AsIProperty.PropertyValue;

		this.actors[actor] = new ActorInfo(faction, false);
		if(!this.actorCount.ContainsKey(faction))
		{
			this.actorCount[faction] = new List<Actor>() { actor };
		}
		else
			this.actorCount[faction].Add(actor);

		// Debug.Log("New Actor registered: "+actor.PresentationName + " faction: " + faction.ToString());

		HealthProperty hp = null;
		if(actor.TryGetProperty<HealthProperty>(out hp))
			hp.OnValueChanged += HandleOnHealthChanged;
		
		if(this.OnActorCreated != null)
			this.OnActorCreated(this, new ActorUpdatedEventArgs(actor));

		this.currentEndGameCondition.CheckConditions();
	}

	public void UnRegisterActor(Actor actor)
	{
		this.actorCount[this.actors[actor].Faction].Remove(actor);
		this.actors.Remove(actor);

		Debug.Log("Actor UNregistered: "+actor.PresentationName);

		if(this.OnActorDestroyed != null)
			this.OnActorDestroyed(this, new ActorUpdatedEventArgs(actor));

		this.currentEndGameCondition.CheckConditions();
	}

	#endregion

	#region Event handling

	void HandleOnHealthChanged (object sender, Property.PropertyEventArgs e)
	{
		if(((HealthProperty.HealthInfo) e.NewValue).CurrentHealth == 0f)
		{
			var actor = ((HealthProperty) sender).AttachedActor;
			this.actorCount[this.actors[actor].Faction].Remove(actor);
			this.actors[actor].IsDead = true;
			// Debug.Log("Actor died: "+actor.PresentationName + " faction " + this.actors[actor].Faction.ToString());
			this.currentEndGameCondition.CheckConditions();

			if(this.OnActorDestroyed != null)
				this.OnActorDestroyed(this, new ActorUpdatedEventArgs(actor));
		}
		else if(((HealthProperty.HealthInfo) e.NewValue).CurrentHealth > 0f && ((HealthProperty.HealthInfo) e.OldValue).CurrentHealth == 0f)
		{
			// Actor was brought back to life
			var actor = ((HealthProperty) sender).AttachedActor;
			this.actorCount[this.actors[actor].Faction].Add(actor);
			this.actors[actor].IsDead = false;
			// Debug.Log("Actor revived: "+actor.PresentationName);
			this.currentEndGameCondition.CheckConditions();
		}
	}

	#endregion

	#region Implementation

	private void StartGame()
	{
		if(this.OnStartGame != null)
			this.OnStartGame(this, new System.EventArgs());
	}

	private void EndGame(FactionType WinningFaction)
	{
		if(this.OnGameOver != null)
			this.OnGameOver(this, new GameOverEventArgs(WinningFaction));
	}

	#endregion

	#region Data

	private Dictionary<Actor, ActorInfo> actors = new Dictionary<Actor, ActorInfo>();
	private Dictionary<FactionType, List<Actor>> actorCount = new Dictionary<FactionType, List<Actor>>();
	private FactionType playersFaction;
	private IWinCondition currentEndGameCondition = null;

	#endregion

	#region Private classes

	private class ActorInfo
	{
		#region Construction

		public ActorInfo (FactionType faction, bool isDead)
		{
			this.Faction = faction;
			this.IsDead = isDead;
		}

		#endregion

		#region Properties

		public FactionType Faction { get; set; }

		public bool IsDead { get; set; }

		#endregion
	}

	public class GameOverEventArgs : System.EventArgs
	{
		#region Construction

		public GameOverEventArgs (FactionType winningFaction)
		{
			this.WinningFaction = winningFaction;
		}

		#endregion

		#region Properties

		public FactionType WinningFaction;

		#endregion
	}

	public class ActorUpdatedEventArgs : System.EventArgs
	{
		#region Construction

		public ActorUpdatedEventArgs (Actor actor)
		{
			this.AffectedActor = actor;
		}

		#endregion

		#region Properties

		public Actor AffectedActor;

		#endregion
	}

	#endregion
}
