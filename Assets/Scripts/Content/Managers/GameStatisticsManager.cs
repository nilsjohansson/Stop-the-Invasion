using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Collects statistics about the current level. Damage dealt, healing done etc.
/// </summary>
public class GameStatisticsManager : Manager {
	#region Configuration
	
	#endregion
	
	#region Construction
	
	void Start () {
		Init(this);

		this.gameManager = Manager.GetInstance<GameManager>() as IGameManager;
		this.gameManager.OnActorCreated += delegate(object sender, GameManager.ActorUpdatedEventArgs e) {
			HealthProperty hp = null;
			if(e.AffectedActor.TryGetProperty<HealthProperty>(out hp))
			{
				hp.OnValueChanged += HealthChangedOnActor;
			}
		};

		foreach(var faction in this.gameManager.Factions.Keys)
		{
			this.factionDamageDistribution.Add(faction, 0f);
			this.factionHealingDistribution.Add(faction, 0f);

			foreach(var actor in this.gameManager.Factions[faction])
			{
				HealthProperty hp = null;
				if(actor.TryGetProperty<HealthProperty>(out hp))
				{
					hp.OnValueChanged += HealthChangedOnActor;
				}
			}
		}
	}

	#endregion
	
	#region Operations

	/// <summary>
	/// The total amount of damage dealt this level per faction.
	/// </summary>
	/// <returns>The damage dealt.</returns>
	/// <param name="faction">Faction.</param>
	public float GetDamageDealt(FactionType faction)
	{
		float result = 0f;
		this.factionDamageDistribution.TryGetValue(faction, out result);

		return result;
	}

	/// <summary>
	/// The total amount of damage dealt this level per faction.
	/// </summary>
	/// <returns>The healing done.</returns>
	/// <param name="faction">Faction.</param>
	public float GetHealingDone(FactionType faction)
	{
		float result = 0f;
		this.factionHealingDistribution.TryGetValue(faction, out result);
		
		return result;
	}

	#endregion

	#region Event handling

	void HealthChangedOnActor (object sender, Property.PropertyEventArgs e)
	{
		if(((HealthProperty)sender).AttachedActor is MineralSpotActor)
			return;

		var newVal = e.NewValue as HealthProperty.HealthInfo;
		var oldVal = e.OldValue as HealthProperty.HealthInfo;
		float totalDamage = oldVal.CurrentHealth - newVal.CurrentHealth;

		if(totalDamage > 0)
		{
			if(!this.factionDamageDistribution.ContainsKey(e.Responsible.Faction))
				this.factionDamageDistribution.Add(e.Responsible.Faction, 0f);
			this.factionDamageDistribution[e.Responsible.Faction] += totalDamage;
		}

		if(totalDamage < 0)
		{
			if(!this.factionHealingDistribution.ContainsKey(e.Responsible.Faction))
				this.factionHealingDistribution.Add(e.Responsible.Faction, 0f);
			// TotalDamage is negative here
			this.factionHealingDistribution[e.Responsible.Faction] -= totalDamage;
		}
	}

	#endregion

	#region Update
	
	void Update () {
		
	}
	
	#endregion
	
	#region Data

	private Dictionary<FactionType, float> factionDamageDistribution = new Dictionary<FactionType, float>();
	private Dictionary<FactionType, float> factionHealingDistribution = new Dictionary<FactionType, float>();
	private IGameManager gameManager = null;

	#endregion
}
