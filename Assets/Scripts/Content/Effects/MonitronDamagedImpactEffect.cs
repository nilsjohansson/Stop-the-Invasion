using UnityEngine;
using System.Collections;

public class MonitronPunchedImpactEffect : ImpactEffect {
	#region Configuration

	public float Volume = 0f;

	public float StartTime1 = 0f;
	public float EndTime1 = 0f;
	public float StartTime2 = 0f;
	public float EndTime2 = 0f;
	public float StartTime3 = 0f;
	public float EndTime3 = 0f;

	#endregion

	#region IEffect implementation

	public override void PlayEffect (object arg)
	{
		if(this.isPlaying)
			return;
		
		this.isPlaying = true;
		if(this.GetComponent<ParticleSystem>() != null)
			this.GetComponent<ParticleSystem>().Play ();
		
		if(this.ImpactSound != null)
			this.ImpactSound.PlayOneShot(ImpactSound.clip);
	}

	#endregion

}
