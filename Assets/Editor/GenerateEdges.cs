using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class GenerateEdges : EditorWindow {

	#region Construction
	
	[MenuItem ("Navigation/Generate edges")]
	public static void Init () {
		GenerateEdges window = (GenerateEdges)EditorWindow.GetWindow (typeof (GenerateEdges));
	}
	
	#endregion
	
	#region GUI
	
	public void OnGUI()
	{
	    GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Selected node(s): "+selectedWaypoints.Count);
				string totalCount = "0";
				if(this.allWaypoints != null) totalCount = this.allWaypoints.Count.ToString(); 
				GUILayout.Label("Total waypoint(s): "+totalCount);
			GUILayout.EndHorizontal();
			
		    GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
					GUILayout.Label("Circle radius");
				    this.circleRadius = EditorGUILayout.FloatField(this.circleRadius, GUILayout.Width(50));
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
					GUILayout.Label("Line of sight width");
				    this.waypointRadius = EditorGUILayout.FloatField(this.waypointRadius, GUILayout.Width(50));
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			if(GUILayout.Button("Connect selected!"))
			{
				Connect();
			}
			
			GUILayout.Label("Line of sight");
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Connect selected"))
			{
				ConnectLineOfSight(this.selectedWaypoints);
			}
			if(GUILayout.Button("Connect all"))
			{
				ConnectLineOfSight(this.allWaypoints);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
				if(GUILayout.Button("Clear selected")) Clear();
				if(GUILayout.Button("Clear all")) ClearAll();
			GUILayout.EndHorizontal();
			GUILayout.Label(output);
		GUILayout.EndVertical();
	}
	
	#endregion
	 
	#region Editor events
	
	void OnFocus()
	{
		if(this.navigationManager == null) this.navigationManager = WaypointNavigationManager.GetInstance();
		this.allWaypoints = ScriptAssistant.GetAllInstancesWithType<Waypoint>();
		this.layerMask = ScriptAssistant.CreateLayerMask(new int[] {1}, true);
		this.neigboursAdded = 0;
	}
	
	void OnSelectionChange()
	{
		selectedWaypoints.Clear();
		foreach(GameObject go in Selection.gameObjects)
		{
			var wp = go.GetComponent<Waypoint>();
			if(wp != null) selectedWaypoints.Add(wp);
		}
		this.neigboursAdded = 0;
	}
		
	#endregion
	
	#region Operations
	
	private void Clear()
	{
		int count = 0;
		foreach(var selectedWp in this.selectedWaypoints)
		{
			count++;
			selectedWp.Neighbours.Clear();
		}
		this.output = "Removed "+count+" neighbours from "+this.selectedWaypoints.Count+" waypoints.";
	}
	
	private void ClearAll()
	{
		int count = 0;
		foreach(var selectedWp in this.allWaypoints)
		{
			count+= selectedWp.Neighbours.Count;
			selectedWp.Neighbours.Clear();
		}
		this.output = "Removed "+count+" neighbours from all waypoints.";
	}
	
	private void Connect()
	{
		int count = 0;
		foreach(var wpA in this.selectedWaypoints)
		{
			foreach(var wpB in this.selectedWaypoints)
			{
				if(AddNeighbourLink(wpA, wpB))
				{
					count++;
				}
			}
		}
		this.output = "Added "+count+" neighbours.";
	}
	
	private void ConnectLineOfSight(List<Waypoint> connectList)
	{
		int count = 0;
		foreach(var wpA in connectList)
		{
			foreach(var wpB in connectList)
			{
				if(Vector3.Distance(wpA.WorldPosition, wpB.WorldPosition) < this.circleRadius || this.circleRadius == 0)
				{
					if(!DetectObstacles(wpA, wpB))
						if(AddNeighbourLink(wpA, wpB))
							count++;
				}
			}
		}
		this.output = "Added "+count+" neighbours.";
	}
	
	private bool DetectObstacles(Waypoint a, Waypoint b) 
	{
		bool obstaclesInTheWay = false;
		Ray ray = new Ray(a.WorldPosition, b.WorldPosition - a.WorldPosition);
		
		obstaclesInTheWay = Physics.SphereCast(ray, this.waypointRadius, Vector3.Distance(a.WorldPosition, b.WorldPosition), this.layerMask);
		
		return obstaclesInTheWay;
	}
	
	private bool AddNeighbourLink(Waypoint iLiveHereSinceBefore, Waypoint iJustMovedIn)
	{
		if(!iLiveHereSinceBefore.Neighbours.Contains(iJustMovedIn))
		{
			if(!(iJustMovedIn == iLiveHereSinceBefore))
			{
				iLiveHereSinceBefore.Neighbours.Add(iJustMovedIn);
				PrefabUtility.DisconnectPrefabInstance(iLiveHereSinceBefore);
				this.neigboursAdded++;
				return true;
			} else
			{
				return false;
			}
			
		}
		return false;
	}
	
	#endregion

	#region Data
	
	private List<Waypoint> allWaypoints = null;
	private WaypointNavigationManager navigationManager;
	private float circleRadius = 0f;
	private List<Waypoint> selectedWaypoints = new List<Waypoint>();
	private float waypointRadius = 0f;
	private int neigboursAdded = 0;
	private int layerMask = 0;
	private string output = "";
	
	#endregion
}
