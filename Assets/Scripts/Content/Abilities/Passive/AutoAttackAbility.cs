using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(FactionProperty))]
[RequireComponent(typeof(AttackAbility))]
public class AutoAttackAbility : PassiveAbility {
	#region Configuration

	public bool Retaliate = true;
	public bool Aggressive = false;
	public bool DefendAllies = false;
	public float VisionRange = 3f;

	#endregion

	#region Construction

	void Start()
	{
		this.gameManager = Manager.GetInstance<GameManager>();
		this.moveAbilityRef = this.GetComponent<MoveAbility>();
		this.MyActor.TryGetProperty(out this.myFaction);

		this.attackAbilityRef = GetComponent<AttackAbility> ();
		if(!this.MyActor.TryGetProperty<IHealthProperty>(out health))
			throw new UnityException("Missing HealthProperty on actor "+this.MyActor.PresentationName);
		
		this.health.OnValueChanged += this.HandleOnMyHealthChanged;
	}

	#endregion

	#region Properties

	public override string PresentationName {
		get {
			return "AutoAttack";
		}
	}

	public override bool InProgress {
		get {
			return true;
		}
	}

	#endregion

	#region Update

	void Update()
	{
		if(this.DefendAllies)
		{
			// Add allies within vision range
			foreach(var actor in this.gameManager.Factions[this.myFaction.PropertyValue])
			{
				// Important to ignore the Actor itself!
				if(!this.alliesWithinVision.Contains(actor) && actor != this.MyActor)
				{
					if(Vector3.Magnitude (actor.NavigationPosition - this.MyActor.NavigationPosition) < this.VisionRange)
					{
						HealthProperty hp = null;
						if(actor.TryGetProperty<HealthProperty>(out hp))
						{
							this.alliesWithinVision.Add(actor);
							hp.OnValueChanged += this.HandleAllyHealthChanged;
						}
					}
				}
			}

			// Remove allies out of vision range 
			foreach(var actor in this.alliesWithinVision)
			{
				if(Vector3.Magnitude (actor.NavigationPosition - this.MyActor.NavigationPosition) > this.VisionRange)
				{
					HealthProperty hp = null;
					if(actor.TryGetProperty<HealthProperty>(out hp))
					{
						hp.OnValueChanged -= this.HandleAllyHealthChanged;
					}

					this.alliesWithinVision.Remove(actor);
				}
			}
		}
	}

	#endregion

	#region Event handling 

	void HandleAllyHealthChanged (object sender, Property.PropertyEventArgs e)
	{
		if(!this.moveAbilityRef.InProgress)
		{
			if(e.Responsible.Faction != this.MyActor.Faction)
			{
				var killList = new System.Collections.Generic.List<Actor>(this.attackAbilityRef.TargetedActors);
				if(!(killList.Contains(e.Responsible)))
				{
					killList.Add(e.Responsible);
					this.attackAbilityRef.Activate(killList.ToArray());
				}
			}
		}
	}
	
	void HandleOnMyHealthChanged (object sender, Property.PropertyEventArgs e)
	{
		if(((HealthProperty.HealthInfo)e.NewValue).CurrentHealth < ((HealthProperty.HealthInfo)e.OldValue).CurrentHealth)
		{
			if(this.Retaliate)
				if(e.Responsible.Faction != this.MyActor.Faction)
					if(!this.moveAbilityRef.InProgress)
					{
						var killList = new System.Collections.Generic.List<Actor>(this.attackAbilityRef.TargetedActors);
						if(!(killList.Contains(e.Responsible)))
						{
							killList.Add(e.Responsible);
							this.attackAbilityRef.Activate(killList.ToArray());
						}
					}
		}
	}

	#endregion

	#region Data

	private IHealthProperty health = null;
	private AttackAbility attackAbilityRef = null;
	private IGameManager gameManager = null;
	private MoveAbility moveAbilityRef = null;
	private FactionProperty myFaction = null;
	private List<Actor> alliesWithinVision = new List<Actor>();

	#endregion
}
