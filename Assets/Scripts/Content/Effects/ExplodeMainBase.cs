using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MainBaseActor))]
public class ExplodeMainBase : IEffect {
	#region implemented abstract members of IEffect
	
	public override void PlayEffect (object arg = null)
	{
		this.GetComponent<ParticleSystem>().Play();
		Destroy(this.gameObject.transform.parent.gameObject, 0.6f);
	}
	
	public override void StopEffect ()
	{
		this.GetComponent<ParticleSystem>().Stop();
	}
	
	#endregion
}
