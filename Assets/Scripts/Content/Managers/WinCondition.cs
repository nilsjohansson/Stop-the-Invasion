using UnityEngine;
using System.Collections;

public interface IWinCondition
{
	void RegisterEndGameCallback(WinCondition.EndGameCallback callback);

	void CheckConditions();
}

public abstract class WinCondition : MonoBehaviour, IWinCondition 
{
	#region Delegates

	public delegate void EndGameCallback(FactionType winningFaction);

	#endregion

	#region Construction

	void Start()
	{

	}

	#endregion

	#region Public methods

	public abstract void CheckConditions();

	public void RegisterEndGameCallback(EndGameCallback callback)
	{
		this.endGameCallback = callback;
	}

	protected void EndGame(FactionType winningFaction)
	{
		this.endGameCallback(winningFaction);
	}

	#endregion

	#region Properties

	#endregion

	#region Data

	private EndGameCallback endGameCallback = null;

	#endregion
}
