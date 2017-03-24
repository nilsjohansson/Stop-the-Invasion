using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projector))]
public class LeakingOilEffect : IEffect {
	#region Configuration

	public float NormalizedRunChance = 1f;
	public float LeakDuration = 5f;
	public float FinalFOVMin = 20f;
	public float FinalFOVMax = 55f;
	public float StartDelay = 0f;
	public GameObject LeakingTarget = null;

	#endregion

	#region Construction

	void Start () {
		this.projector = GetComponent<Projector>();
		this.startFOV = this.projector.fieldOfView;
		this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.x, Random.Range(0f, 360f)));
		this.finalFOV = Random.Range(this.FinalFOVMin, this.FinalFOVMax);
		this.targetOffset = new Vector3(0f, this.transform.position.y, 0f);
	}

	#endregion

	#region implemented abstract members of IEffect

	public override void PlayEffect (object arg = null)
	{
		if(Random.value <= Mathf.Clamp01(this.NormalizedRunChance))
			Manager.GetInstance<ConditionalCallbackManager>().RegisterDelayedCallback(this.StartDelay, this.BeginAnimating);
	}

	public override void StopEffect ()
	{
		this.projector.fieldOfView = this.startFOV;
		this.projector.enabled = false;
	}

	#endregion

	#region Implementation

	private void BeginAnimating()
	{
		this.startTime = Time.time;
		this.projector.enabled = true;
		this.projector.fieldOfView = this.startFOV;
		this.transform.position = this.LeakingTarget.transform.position + this.targetOffset;
		this.startPos = this.transform.localPosition - this.transform.localRotation * (Vector3.up * 0.02f);
	}

	#endregion

	#region Update

	void Update () {
		if(this.startTime < 0f || Time.time > this.startTime + this.LeakDuration)
			return;

		var progress = (Time.time - this.startTime) / this.LeakDuration;
		progress = Mathf.Clamp(progress, 0f, 1f);
		this.projector.fieldOfView = startFOV + (this.finalFOV - this.startFOV) * Mathf.Sqrt(progress);
		this.transform.localPosition = this.startPos + this.transform.localRotation * ((this.startPos + Vector3.down * 0.02f) - this.startPos) * Mathf.Sqrt(progress);
	}

	#endregion

	#region Data

	private float startTime = -1f;
	private float startFOV = 0f;
	private float finalFOV = 0f;
	private Vector3 startPos = Vector3.zero;
	private Vector3 targetOffset = Vector3.zero;
	private Projector projector = null;

	#endregion
}
