using UnityEngine;
using System.Collections;

public class NavigationTester : MonoBehaviour {
	
	#region Properties
	
	public Waypoint StartPoint;
	
	public Waypoint Destination;
	
	#endregion
	
	#region Construction
	
	void Start () {
		
	}
	
	#endregion
	
	#region OnGUI
	public int searchIteration = 0;
	void OnGUI () {
		if(GUI.Button(new Rect(15,15,100,40), "Navigate x 100k!"))
		{
			var wpManager = WaypointNavigationManager.GetInstance();
			
			System.Collections.Generic.List<Waypoint> path = null;
			path = wpManager.FindShortestPathBetween(StartPoint, Destination);
			
			foreach(var node in path)
				node.IsCurrent = true;
			
			//wpManager.FindShortestPathBetweenUsingUpdate(StartPoint, Destination);
			
		}
		if(GUI.Button(new Rect(15,65,200,40), "Navigate visually"))
		{
			var wpManager = WaypointNavigationManager.GetInstance();
		
			wpManager.FindShortestPathBetweenUsingUpdate(StartPoint, Destination);
		}
	}
	
	#endregion
	
	#region Data
	
	#endregion
}
