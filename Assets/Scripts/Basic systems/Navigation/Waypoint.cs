using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour {
	
	#region Properties
	
	public Color ClosedColor;
	public Color OpenColor;
	public Color CurrentColor;
	
	/// <summary>
	/// All the neighbours of this waypoint.
	/// </summary>
	public List<Waypoint> Neighbours = new List<Waypoint>();
	
	/// <summary>
	/// Gets the waypoints' world position.
	/// </summary>
	/// <value>
	/// The world position.
	/// </value>
	public Vector3 WorldPosition {
		get { return transform.position; }
	}
	
	public bool IsClosed {
		get { return this.isClosed; }
		set 
		{ 
			this.isClosed = value; 
			if(this.isClosed) 
			{
				this.GetComponent<Renderer>().material.color = this.ClosedColor;
			} else
			{
				this.GetComponent<Renderer>().material.color = this.standardColor;
			}
		}
	}
	public bool IsOpen {
		get { return this.isOpen; }
		set 
		{ 
			this.isOpen = value; 
			if(this.isOpen) 
			{
				this.GetComponent<Renderer>().material.color = this.OpenColor;
			} else
			{
				this.GetComponent<Renderer>().material.color = this.standardColor;
			}
		}
	}
	public bool IsCurrent {
		get { return this.isCurrent; }
		set 
		{ 
			this.isCurrent = value; 
			if(this.isCurrent) 
			{
				this.GetComponent<Renderer>().material.color = this.CurrentColor;
			} else
			{
				this.GetComponent<Renderer>().material.color = this.standardColor;
			}
		}
	}
	#endregion
	
	#region Construction
	
	void Start () {
		// Shoot a ray downwards to detect the ground
		var groundPosition = ScriptAssistant.GetGroundPosition(this.transform.position, WaypointNavigationManager.GetInstance().TheGround);
		if(groundPosition == null) 
		{
			Destroy(this);
		}
		this.transform.position = groundPosition.Value;

		this.standardColor = this.GetComponent<Renderer>().material.color;
	}
	
	#endregion
	
	#region Public methods
	
	
	
	#endregion
	
	#region Update
	void Update () {
	
	}
	#endregion
	
	#region Data
	
	private bool isClosed = false;
	private bool isOpen = false;
	private bool isCurrent = false;
	private Color standardColor;
	
	#endregion
}
