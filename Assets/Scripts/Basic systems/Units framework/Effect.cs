using UnityEngine;

public abstract class IEffect : MonoBehaviour{
	/// <summary>
	/// Plays a single occurence of this effect. Typical use could be turning on a light, playing a sound and playing an emission from a ParticleSystem.
	/// </summary>
	abstract public void PlayEffect(object arg = null);

	/// <summary>
	/// Stops the effect.
	/// </summary>
	abstract public void StopEffect();	

	/*
	abstract public void PlayEffectOnce(); */
}
