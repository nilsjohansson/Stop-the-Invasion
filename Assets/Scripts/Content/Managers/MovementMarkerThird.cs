using UnityEngine;
using System.Collections;

public class MovementMarkerThird : IEffect {
	
	#region Properties
	
	public GameObject firstThird;
	public GameObject secondThird;
	public GameObject thirdThird;
	public float rotationAnglesPerSecond = 45f;
	public float separationDistance = 0.5f;
	public float pulseFrequency = 1f;
	public float pulseDistance = 1f;
	
	#endregion
	
	#region Construction
	
	void Start () {
		if(firstThird != null && secondThird != null)
		{
			totalSeparationDistance = separationDistance * 0.5f 
				+ (firstThird.transform.localScale.z 
					* firstThird.GetComponent<MeshFilter>().sharedMesh.bounds.size.z) * 0.833f;
			secondThird.transform.localRotation = Quaternion.AngleAxis(120f, Vector3.up);
			thirdThird.transform.localRotation = Quaternion.AngleAxis(240f, Vector3.up);
		}

		// this.renderer.enabled = false;
	}
	
	#endregion

	#region implemented abstract members of IEffect

	public override void PlayEffect (object arg = null)
	{
		if(arg is Vector3)
		{
			var newPosition = ScriptAssistant.GetGroundPosition((Vector3)arg, Manager.GetInstance<TouchInteractionManager>().TheGround);
			if(newPosition.HasValue)
				this.transform.position = newPosition.Value + new Vector3(0,0.08f,0);
		}
		else if(arg is Actor)
		{
			// var newPosition = ScriptAssistant.GetGroundPosition(((Actor)arg).AsIActor.NavigationPosition, Manager.GetInstance<TouchInteractionManager>().TheGround);
			//Debug.Log (newPosition.Value.ToString());
			this.transform.position = ((Actor)arg).AsIActor.NavigationPosition + new Vector3(0,0.18f,0);

			this.transform.localScale *= (arg as Actor).AsIActor.Width;
		}

		this.animate = true;
		// this.renderer.enabled = true;
	}

	public override void StopEffect ()
	{
		this.animate = false;
		// this.renderer.enabled = false;
		this.transform.localScale = Vector3.one;
	}

	#endregion
	
	#region Update

	void Update () {
		if(firstThird != null && secondThird != null && animate)
		{
			pulseAngle += Time.deltaTime * 360f * pulseFrequency;
			//if(pulseAngle > 360f) pulseAngle -= 360f;
			this.transform.Rotate(Vector3.up, Time.deltaTime * this.rotationAnglesPerSecond);
			float pulse = Mathf.Cos(pulseAngle*Mathf.Deg2Rad) * 0.5f + 0.5f;
			
			firstThird.transform.localPosition = Quaternion.AngleAxis(0f, Vector3.up) * new Vector3(0f, 0f, pulse * pulseDistance + totalSeparationDistance);
			secondThird.transform.localPosition = Quaternion.AngleAxis(120f, Vector3.up) * new Vector3(0f, 0f, pulse * pulseDistance + totalSeparationDistance);
			thirdThird.transform.localPosition = Quaternion.AngleAxis(240f, Vector3.up) * new Vector3(0f, 0f, pulse * pulseDistance + totalSeparationDistance);
		}
	}
	
	#endregion
	
	#region Data
	
	private float pulseAngle = 0f;
	private float totalSeparationDistance = 0f;
	private bool animate = false;

	#endregion
}
