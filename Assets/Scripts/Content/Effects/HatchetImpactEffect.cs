using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class HatchetImpactEffect : ImpactEffect {
	#region Construction
	
	void Start()
	{
		this.GetComponent<Light>().enabled = false;
	}
	
	#endregion

	#region Override

	public override void PlayEffect (object arg)
	{
		base.PlayEffect (arg);
		this.GetComponent<Light>().enabled = true;
	}

	public override void StopEffect ()
	{
		base.StopEffect ();
		this.GetComponent<Light>().enabled = false;
	}

	#endregion
}
