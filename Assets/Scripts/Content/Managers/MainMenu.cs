using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	#region Configuration
	
	#endregion
	
	#region Construction
	
	void Start () {
		this.textGUIManager = Manager.GetInstance<TextGUIManager>() as ITextGUIManager;
	}
	
	#endregion

	#region GUI

	void OnGUI()
	{
		switch (this.menuState)
		{
		case MenuState.Main:
			this.MainDisplay ();
			break;
		}
	}

	#endregion

	#region Implementation

	private void MainDisplay()
	{
		this.textGUIManager.GUIDisplayText(new Rect(Screen.width / 2f, this.menuLogoYCoords, 0f, 0f), "Gatherers", Color.white, Color.black, 120);
		if(this.textGUIManager.GUIButton(new Rect(Screen.width / 2f, this.firstButtonYCoords, 400f, 100f), "Start", Color.white, Color.black, 60))
		{
			Manager.GetInstance<ProgressManager>().LoadNextLevel();
			Destroy (this);
		}
		
		if(this.textGUIManager.GUIButton(new Rect(Screen.width / 2f, this.secondButtonYCoords, 400f, 100f), "Quit", Color.white, Color.black, 60))
		{
			Application.Quit();
		}
	}

	#endregion
	
	#region Update
	
	void Update () {
		
	}
	
	#endregion

	#region State
	
	private enum MenuState
	{
		Main
	}

	#endregion

	#region Data

	// Pre calculated coords
	private float menuLogoYCoords = Screen.height * 3f / 8f;
	private float firstButtonYCoords = Screen.height * 5f / 8f;
	private float secondButtonYCoords = Screen.height * 5f / 8f + Screen.height * 0.15f;

	private MenuState menuState = MenuState.Main;
	private ITextGUIManager textGUIManager = null;

	#endregion
}
