using UnityEngine;
using System.Collections;

public class Level1Intro : Introduction {
	#region Construction

	public float IntroTime = 4f;

	#endregion

	#region Construction

	void Start()
	{
		this.gestureManager = Manager.GetInstance<GestureManager>();
	}

	#endregion

	#region Abstract implementation

	public override void BeginIntroduction ()
	{
		this.introBegun = true;
		this.textDisplay = Manager.GetInstance<TextGUIManager>() as ITextGUIManager;
		this.touchCallbackID = Manager.GetInstance<ConditionalCallbackManager>().RegisterCallback(this.DetectAnyInput, this.EndIntroduction);
		this.delayedCallbackID = Manager.GetInstance<ConditionalCallbackManager>().RegisterDelayedCallback(this.IntroTime, this.EndIntroduction);
	}

	#endregion

	#region Implementation

	private bool DetectAnyInput()
	{
		return this.gestureManager.ReleasedSingleTouch() || this.gestureManager.ReleasedDoubleTouch();
	}

	private void EndIntroduction()
	{
		var ccb = Manager.GetInstance<ConditionalCallbackManager>();
		ccb.UnRegisterCallback(this.delayedCallbackID);
		ccb.UnRegisterCallback(this.touchCallbackID);

		this.StartGame();
		this.introBegun = false;
	}

	#endregion

	#region GUI

	void OnGUI()
	{
		if(this.introBegun)
		{
			var centerX = Screen.width / 2f;
			//var centerY = Screen.height / 2f;

			this.textDisplay.GUIDisplayText(new Rect(centerX, Screen.height * 3f / 8f, 0f, 0f), "Level 0 - Training", Color.white, Color.black, 50);
			this.textDisplay.GUIDisplayText(new Rect(centerX, Screen.height * 5f / 8f, 0f, 0f), "Objective: Gather 3 mineral stacks.", Color.white, Color.black, 30);
			// this.textDisplay.GUIDisplayText(new Rect(centerX, Screen.height * 5.8f / 8f, 0f, 0f), "Adventure awaits on the other side.", Color.white, Color.black, 25);
		}
	}

	#endregion

	#region Data

	private int delayedCallbackID = 0;
	private int touchCallbackID = 0;
	private bool introBegun = false;
	private ITextGUIManager textDisplay = null;
	private GestureManager gestureManager = null;

	#endregion
}
