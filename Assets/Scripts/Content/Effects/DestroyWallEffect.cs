using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestroyWallEffect : IEffect {
	#region Configuration

	public List<HealthProperty> WallPieces = new List<HealthProperty>();

	#endregion

	#region implemented abstract members of IEffect

	public override void PlayEffect (object arg = null)
	{
		var myActor = this.GetComponent<Actor>();
		foreach(var hp in this.WallPieces)
		{
			hp.AsIProperty.Damage(hp.AsIProperty.PropertyValue.MaxHealth, myActor);
		}
	}

	public override void StopEffect ()
	{

	}

	#endregion
}
