using UnityEngine;
using System.Collections;

public class Level1WinCondition : WinCondition {
	#region Configuration

	public int RequiredMinerals = 3;

	#endregion

	#region Construction

	void Start()
	{
		this.gameManager = Manager.GetInstance<GameManager>() as IGameManager;

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
			this.EndGame(FactionType.Neutral);
		}
		else if(this.playersBase == null)
		{
			foreach(var actor in playerFaction)
			{
				var mba = actor as MainBaseActor;
				if(mba != null)
				{
					this.playersBase = mba;
					MineralsProperty mp = null;
					if(mba.TryGetProperty<MineralsProperty>(out mp))
					{
						mp.OnValueChanged += PlayersMineralsChanged;
					}
				}
			}
		}
	}

	#endregion

	#region Update
	
	void PlayersMineralsChanged (object sender, Property.PropertyEventArgs e)
	{
		int minerals = (int) e.NewValue;
		if(minerals >= this.RequiredMinerals)
			this.EndGame(this.gameManager.PlayerFaction);
	}

	#endregion

	#region Data

	private IGameManager gameManager = null;
	private MainBaseActor playersBase = null;

	#endregion
}
