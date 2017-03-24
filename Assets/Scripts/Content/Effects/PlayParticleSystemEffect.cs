using UnityEngine;
using System.Collections;

/// <summary>
/// Starts and stops the particle system attached to this GameObject.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class PlayParticleSystemEffect : IEffect {
	#region implemented abstract members of IEffect

	public override void PlayEffect (object arg = null)
	{
		this.GetComponent<ParticleSystem>().Play();
	}

	public override void StopEffect ()
	{
		this.GetComponent<ParticleSystem>().Stop();
	}

	#endregion


}
