using UnityEngine;
using System.Collections;

public class SpawnActorAbility : ActiveAbility {
	#region Configuration
	
	public int MonitronPrice = 3;
	public GameObject MonitronBluePrints = null;
	
	#endregion
	
	#region Construction
	
	void Start()
	{

	}
	
	#endregion
	
	#region Overrides
	
	public override bool CanActivate ()
	{
		if(this.actorsMinerals == null) this.MyActor.TryGetProperty<MineralsProperty>(out this.actorsMinerals);
		return this.actorsMinerals != null && (int)this.actorsMinerals.PropertyValue >= MonitronPrice;
	}
	
	public override void Activate ()
	{
		this.inProgress = true;
		if(!this.actorsMinerals.ReduceMinerals(this.MonitronPrice, this.MyActor))
		{
			this.inProgress = false;
			FinishAbility(false);
			return;
		}
		
		GameObject go = (GameObject) Instantiate(MonitronBluePrints, ((MainBaseActor)this.MyActor).SpawnLocation.transform.position, Quaternion.identity);
		go.transform.parent = null;
		
		this.inProgress = false;
		FinishAbility(true);
	}
	
	public override InputType RequiredInput {
		get {
			return InputType.None;
		}
	}
	
	public override string PresentationName {
		get {
			return "Create Monitron unit";
		}
	}
	
	public override bool InProgress {
		get {
			return this.inProgress;
		}
	}
	
	#endregion
	
	#region Data
	
	private MineralsProperty actorsMinerals = null;
	private bool inProgress = false;
	
	#endregion
}
