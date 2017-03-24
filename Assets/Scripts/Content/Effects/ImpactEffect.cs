using UnityEngine;
using System.Collections;

public class ImpactEffect : IEffect {
	#region Configuration
	
	public float PlayDuration = 0.5f;
	public AudioSource ImpactSound = null;
	
	#endregion 

	#region IEffect implementation
	
	public override void PlayEffect (object arg)
	{
		if(this.isPlaying)
			return;
		
		this.isPlaying = true;
		this.startTime = Time.time;
		if(this.GetComponent<ParticleSystem>() != null)
			this.GetComponent<ParticleSystem>().Play ();
		
		if(this.ImpactSound != null)
			this.ImpactSound.PlayOneShot(ImpactSound.clip);
	}
	
	public override void StopEffect ()
	{
		this.isPlaying = false;
		if(this.GetComponent<ParticleSystem>() != null)
			this.GetComponent<ParticleSystem>().Stop ();
	}
	
	#endregion
	
	#region Update
	
	void Update()
	{
		if(this.isPlaying && Time.time > this.startTime + PlayDuration)
		{
			this.StopEffect();
		}
	}
	
	#endregion
	
	#region Data
	
	protected bool isPlaying = false;
	private float startTime = 0f;
	
	#endregion
}
