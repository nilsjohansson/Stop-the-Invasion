using UnityEngine;
using System.Collections;

public class MainBaseActor : Actor {
	
	#region Configuration
	
	public GameObject SpawnLocation = null;
	
	#endregion
	
	#region Actor properties
	
	public override string PresentationName {
		get {
			return "Main base";
		}
	}
	
	public override Vector3 NavigationPosition {
		get {
			var projectedPosition = ScriptAssistant.GetGroundPosition(this.transform.position, Manager.GetInstance<TouchInteractionManager>().TheGround);
			return (projectedPosition.HasValue ? projectedPosition.Value : this.transform.position);
		}
	}


	public override float Height {
		get {
			return 1f;
		}
	}
	#endregion
}
