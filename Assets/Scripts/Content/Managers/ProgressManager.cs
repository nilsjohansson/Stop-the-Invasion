using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProgressManager : Manager {
	#region Configuration

	public List<int> LevelsInOrder = new List<int>();

	#endregion
	
	#region Construction

	void Awake()
	{
		DontDestroyOnLoad(this);
		Init (this);
	}
	
	#endregion
	
	#region Properties

	public int CurrentLevel 
	{ 
		get { return this.LevelsInOrder[currentLevelIndex]; } 
	}

	#endregion

	#region Operations
	
	public void ReloadLevel()
	{
		Manager.ResetAll();
		Application.LoadLevel(currentLevelIndex);
	}

	public void LoadNextLevel()
	{
		if(currentLevelIndex + 1 <= this.LevelsInOrder.Count -1)
		{
			completedLevels.Add(currentLevelIndex);

			Manager.ResetAll();
			Debug.Log ("Loading level "+ this.LevelsInOrder[currentLevelIndex]);
			Application.LoadLevel(this.LevelsInOrder[currentLevelIndex]);
		}
	}
	
	#endregion

	#region Event handler
	
	#endregion
	
	#region Data

	private static int currentLevelIndex = 0;
	private static List<int> completedLevels = new List<int>();

	#endregion
}
