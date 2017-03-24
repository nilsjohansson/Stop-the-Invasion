using UnityEngine;
using System.Collections;

public abstract class Introduction : MonoBehaviour {
	#region Delegates

	public delegate void StartGameCallback();

	#endregion

	#region Public methods

	public abstract void BeginIntroduction();

	public void RegisterStartGameCallback(StartGameCallback callback)
	{
		this.startGameCallback = callback;
	}

	protected void StartGame()
	{
		this.startGameCallback();
	}

	#endregion

	#region Data

	protected StartGameCallback startGameCallback = null;

	#endregion
}
