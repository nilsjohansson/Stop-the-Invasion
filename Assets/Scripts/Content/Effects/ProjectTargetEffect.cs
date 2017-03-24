using UnityEngine;
using System.Collections;

public class ProjectTargetEffect : IEffect {
	#region Configuration

	public GameObject RotatingProjector = null;
	public float RotationSpeed = 15f;

	#endregion

	#region Construction

	void Start () {
		this.originalPosition = this.transform.position;
		if(this.RotatingProjector!= null) this.originalPosition = this.RotatingProjector.transform.position;
	}
	
	#endregion

	#region implemented abstract members of IEffect

	public override void PlayEffect (object arg = null)
	{
		if(arg is Vector3)
		{
			var newPosition = ScriptAssistant.GetGroundPosition((Vector3)arg, Manager.GetInstance<TouchInteractionManager>().TheGround);
			if(newPosition.HasValue)
				this.transform.position = newPosition.Value + new Vector3(0, this.originalPosition.y, 0);
		}
		else if(arg is Actor)
		{
			// var newPosition = ScriptAssistant.GetGroundPosition(((Actor)arg).AsIActor.NavigationPosition, Manager.GetInstance<TouchInteractionManager>().TheGround);
			//Debug.Log (newPosition.Value.ToString());
			var height = this.originalPosition.y;
			if(((Actor)arg).Height * 0.35f > height) height = ((Actor)arg).Height * 0.35f;
			this.transform.position = ((Actor)arg).AsIActor.NavigationPosition + new Vector3(0, height,0);
			
			// this.transform.localScale *= (arg as Actor).AsIActor.Width;
		}
		
		this.animate = true;
	}

	public override void StopEffect ()
	{
		this.animate = false;
	}

	#endregion

	#region Update

	void Update () {
		if(this.animate)
		{
			if(this.RotatingProjector != null)
				this.RotatingProjector.transform.Rotate(Vector3.up, Time.deltaTime * this.RotationSpeed, Space.World);
		}
	}

	#endregion

	#region Data

	private Vector3 originalPosition;
	private bool animate = false;

	#endregion
}
