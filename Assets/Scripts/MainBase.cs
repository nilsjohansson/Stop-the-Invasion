﻿using UnityEngine;
using System.Collections;

public interface ITarget
{
	#region Properties

	Vector3 CurrentPosition { get; } 

	#endregion
}

public class MainBase : MonoBehaviour, ITarget
{
	#region Properties

	public float CannonBallForce = 100f;
	public GameObject Cannon;
	public GameObject Ammunition;
	public GameObject cbTrajectoryBase;
	public GameObject cbTrajectoryTarget;

	#endregion

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
		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit = new RaycastHit();
			if(Physics.Raycast(ray, out hit))
			{
				var lookDirection = hit.point - Cannon.transform.position;

				var newRotation = new Quaternion();
				newRotation.SetLookRotation(lookDirection, Vector3.up);

				var quaternion = new Quaternion {
					eulerAngles = new Vector3(0,-90,0)
				};

				newRotation *= quaternion;
				Cannon.transform.rotation = newRotation;

				var cannonBall = Instantiate(Ammunition, cbTrajectoryTarget.WPos(), Quaternion.identity);
				var forceModifier = Vector3.Distance(transform.position, hit.point) * 0.12f;
				cannonBall.GetComponent<Rigidbody> ().AddForce((cbTrajectoryTarget.WPos() - cbTrajectoryBase.WPos()) * CannonBallForce * forceModifier);
				cannonBall.GetComponent<Cannonball>().SetOwner(this.gameObject.GetComponent<MainBaseActor>());
			}
		}
	}

//	private Quaternion TurnVector(Vector3 vector)
//	{
//		var newRotation = new Quaternion();
//		newRotation.SetLookRotation(vector, Vector3.up);
//		var euler = newRotation.eulerAngles;
//		euler.z = 0f;
//		euler.x = 0f;
//		euler.y -= 90;
//		return euler;
//	}

	#endregion
}

public static class GameObjectExtensions
{
	public static Vector3 WPos(this GameObject source)
	{
		return source.transform.position;
	}
}
