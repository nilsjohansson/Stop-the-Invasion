using UnityEngine;
using System.Collections;

public class Monitron : Actor {
	#region Properties
	
	public override string PresentationName {
		get {
			return "Monitron";
		}
	}
	
	public override Vector3 NavigationPosition {
		get 
		{
			var projectedPosition = ScriptAssistant.GetGroundPosition(this.transform.position, Manager.GetInstance<TouchInteractionManager>().TheGround);
			return (projectedPosition.HasValue ? projectedPosition.Value : this.transform.position);
		}
	}

	public override float Height {
		get {
			return this.AttachedCharController.height * this.transform.lossyScale.y;
		}
	}

	public override float Width {
		get {
			return this.AttachedCharController.radius * 2f * this.transform.lossyScale.x;
		}
	}
	
	#endregion
}
