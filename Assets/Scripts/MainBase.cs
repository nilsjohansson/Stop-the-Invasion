using UnityEngine;
using System.Collections;

public interface ITarget
{
	#region Properties

	Vector3 CurrentPosition { get; } 

	#endregion
}

public class MainBase : MonoBehaviour, ITarget
{
	#region Construction

	void Start () 
	{
	
	}

	#endregion

	#region ITarget implementation

	public Vector3 CurrentPosition 
	{
		get { return this.transform.position; }
	}

	#endregion

	#region Messages

	void Update () 
	{

	}

	#endregion
}
