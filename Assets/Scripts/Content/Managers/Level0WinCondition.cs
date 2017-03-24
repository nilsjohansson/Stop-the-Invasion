using UnityEngine;
using System.Collections;

public class Level0WinCondition : WinCondition {
	#region Configuration

	public Actor TheBoss = null;

	#endregion

	#region Construction

	void Start()
	{
		this.gameManager = Manager.GetInstance<GameManager>() as IGameManager;
		TheBoss.TryGetProperty<HealthProperty>(out this.bossHp);
	}

	#endregion

	#region Abstract members of WinCondition

	public override void CheckConditions ()
	{
		if(this.gameManager == null || !this.gameManager.Factions.ContainsKey(this.gameManager.PlayerFaction))
			return;

		var playerFaction = this.gameManager.Factions[this.gameManager.PlayerFaction];
		if(playerFaction.Count == 0)
		{
			FactionProperty ft = null;
			if(TheBoss.TryGetProperty<FactionProperty>(out ft))
			{
				this.EndGame(ft.PropertyValue);
			}
			else
				throw new UnityException("Missing FactionProperty on TheBoss");
		}
	}

	#endregion

	#region Update

	void Update()
	{
		if(this.bossHp != null)
		{
			if(this.bossHp.AsIProperty.PropertyValue.CurrentHealth == 0f)
			{
				this.EndGame(this.gameManager.PlayerFaction);
				this.bossHp = null;
			}
		}
	}

	#endregion

	#region Data

	private IGameManager gameManager = null;
	private HealthProperty bossHp = null;

	#endregion
}
