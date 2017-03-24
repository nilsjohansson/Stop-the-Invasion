using UnityEngine;
using System.Collections;

public class FloatingMineralSpotEffect : IEffect {
	#region Configuration

	public float SpinSpeed = 15f;
	public float BounceInterval = 4f;
	public float BounceHeight = 1f;

	#endregion

	#region implemented abstract members of IEffect

	public override void PlayEffect (object arg = null)
	{

		this.animate = false;
	}

	public override void StopEffect ()
	{

	}

	#endregion

	#region Update

	void Update()
	{
		if(!this.animate)
			return;

		this.transform.Rotate(Vector3.up, Time.deltaTime * this.SpinSpeed, Space.World);
		var bouncePosition = Vector3.up * Mathf.Sin(Time.time / this.BounceInterval) * this.BounceHeight;
		this.transform.localPosition = bouncePosition;
	}

	#endregion

	#region Data

	private bool animate = true;

	#endregion
}
