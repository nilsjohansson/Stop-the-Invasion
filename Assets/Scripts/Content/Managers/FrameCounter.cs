using UnityEngine;
using System.Collections;

public class FrameCounter : MonoBehaviour {
	#region Configuration

	public float SamplePeriod = 5f;

	#endregion

	#region Construction

	void Start () {
	
	}

	#endregion

	#region GUI

	void OnGUI()
	{
		Manager.GetInstance<TextGUIManager>().GUIDisplayText(new Rect(Screen.width * 0.1f, Screen.height * 0.05f, 0f, 0f), this.outputString);
		//Manager.GetInstance<TextGUIManager>().GUIDisplayText(new Rect(Screen.width * 0.1f, Screen.height * 0.1f, 0f, 0f), "GetInstance calls: "+Manager.GetInstanceCalls + " Time: "+Manager.GetInstanceCallsTotaltime);
	}

	#endregion

	#region Update

	void Update () {
		frames ++;

		if(Time.time > this.lastSampleTime + SamplePeriod)
		{
			outputString = string.Format("{0} FPS (average {1}s)", Mathf.CeilToInt((float)frames / SamplePeriod), SamplePeriod);
			frames = 0;
			lastSampleTime = Time.time;
		}
	}

	#endregion 

	#region Data

	private int frames = 0;
	private float lastSampleTime = 0f;
	private string outputString = "";

	#endregion
}
