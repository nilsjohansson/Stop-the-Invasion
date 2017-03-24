using UnityEngine;
using System.Collections;

public class MineralSpotActor : Actor {
	#region Properties

	public override string PresentationName {
		get {
			return "Mineral spot";
		}
	}

	public override Vector3 NavigationPosition {
		get 
		{
			if(!this.projectedPosition.HasValue)
				this.projectedPosition = ScriptAssistant.GetGroundPosition(this.transform.position + this.transform.rotation * Vector3.Scale (this.AttachedCharController.center, this.transform.localScale), Manager.GetInstance<TouchInteractionManager>().TheGround);
			return (projectedPosition.HasValue ? projectedPosition.Value : this.transform.position);
		}
	}
	
	public override float Height {
		get {
			return this.AttachedCharController.height * this.transform.lossyScale.y * 0.5f + (this.transform.position.y - this.NavigationPosition.y);
		}
	}

	public override float Width {
		get {
			return this.AttachedCharController.radius * 2f * this.transform.lossyScale.x;
		}
	}

	#endregion

	#region Data

	private Vector3? projectedPosition;

	#endregion
}
